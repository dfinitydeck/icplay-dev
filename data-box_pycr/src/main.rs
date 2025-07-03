mod handlers;
use handlers::*;

use axum::{
    extract::{Query, Form},
    routing::{get, post},
    Json, Router,
    response::IntoResponse,
};
use chrono::Utc;
use redis::{AsyncCommands, aio::Connection};
use serde::{Deserialize, Serialize};
use std::{
    collections::HashMap,
    fs::{self, OpenOptions},
    io::Write,
    net::SocketAddr,
};
use md5;

// ---- Shared Types ----
#[derive(Debug, Deserialize)]
struct IdParams {
    playerId: String,
}

#[derive(Debug, Deserialize)]
struct LeaderboardParams {
    playerId: String,
    #[serde(default)]
    rankType: Option<u8>,
    #[serde(default)]
    num: Option<u32>,
}

#[derive(Debug, Deserialize)]
struct ScoreParams {
    playerId: String,
    sign: String,
}

#[tokio::main]
async fn main() {
    let app = Router::new()
    .route("/login", get(login))
    .route("/user_data", get(user_data))
    .route("/leaderboard", get(leaderboard))
    .route("/submit_score", post(submit_score))
    .route("/heartbeat", get(heartbeat))
    .route("/transactions", get(transactions))
    .route("/config", get(config));

    let addr = SocketAddr::from(([127, 0, 0, 1], 3000));
    println!("Server running at http://{}", addr);
    axum::Server::bind(&addr).serve(app.into_make_service()).await.unwrap();
}

// ---------- Handlers ----------

async fn handle_login(Query(params): Query<IdParams>) -> impl IntoResponse {
    if invalid_id(&params.playerId) {
        return "playerId error".into_response();
    }

    let mut con = redis_conn().await;
    let value: Option<String> = con.hget("ddcr_player_info", &params.playerId).await.unwrap_or(None);
    let mut data = value
        .and_then(|v| serde_json::from_str(&v).ok())
        .unwrap_or_else(|| {
            let mut d = HashMap::new();
            d.insert("count".to_string(), serde_json::json!(10));
            d
        });

    data.insert("loginTs".to_string(), serde_json::json!(Utc::now().timestamp()));
    let _ = con.hset("ddcr_player_info", &params.playerId, serde_json::to_string(&data).unwrap()).await;

    "SUCCESS".into_response()
}

async fn handle_user_data(Query(params): Query<IdParams>) -> impl IntoResponse {
    let mut con = redis_conn().await;
    let value: Option<String> = con.hget("ddcr_player_info", &params.playerId).await.unwrap_or(None);
    let mut data = value
        .and_then(|v| serde_json::from_str(&v).ok())
        .unwrap_or_default();
    let all = get_all_money(&mut con).await;
    data.insert("allMondy".to_string(), serde_json::json!(all));
    Json(data).into_response()
}

async fn handle_leaderboard(Query(params): Query<LeaderboardParams>) -> impl IntoResponse {
    let mut con = redis_conn().await;
    let rank_type = params.rankType.unwrap_or(1);
    let num = params.num.unwrap_or(10);
    let rank_name = match rank_type {
        1 => "ddcr_rank_all_sort",
        2 => "ddcr_rank_week_sort",
        _ => return "rankType error".into_response(),
    };

    let ids: Vec<String> = con.zrevrange(rank_name, 0, num as isize - 1).await.unwrap_or_default();
    let mut rank = vec![];

    for id in ids {
        if let Some((pid, _)) = id.split_once('_') {
            if let Ok(Some(v)) = con.hget::<_, _, Option<String>>("ddcr_player_info_show", pid).await {
                if let Ok(obj) = serde_json::from_str(&v) {
                    rank.push(obj);
                }
            }
        }
    }

    let own = con.hget::<_, _, Option<String>>("ddcr_player_info_show", &params.playerId).await
        .ok()
        .and_then(|v| v.map(|s| serde_json::from_str::<HashMap<String, serde_json::Value>>(&s).unwrap_or_default()))
        .unwrap_or_default();

    let mut response = HashMap::new();
    response.insert("rank".to_string(), serde_json::json!(rank));
    let mut own_with_rank = own.clone();
    let rank_num = get_own_rank(&mut con, rank_name, &params.playerId).await;
    own_with_rank.insert("rank".to_string(), serde_json::json!(rank_num));
    response.insert("own".to_string(), serde_json::json!(own_with_rank));
    Json(response).into_response()
}

async fn handle_submit_score(Query(query): Query<ScoreParams>, Form(form): Form<HashMap<String, String>>) -> impl IntoResponse {
    let pid = &query.playerId;
    let data = form.get("data").cloned().unwrap_or_default();
    write_log(&data);

    let mut con = redis_conn().await;

    if !check_login(&mut con, pid).await {
        return "not login".into_response();
    }

    let sign_key: String = match con.get("ddcr_signKey").await {
        Ok(v) => v,
        Err(_) => return "ddcr_signKey not exist".into_response(),
    };

    let expected = format!("{:x}", md5::compute(format!("{}{}{}", pid, data, sign_key)));
    if expected != query.sign {
        return "sign error".into_response();
    }

    let array: HashMap<String, serde_json::Value> = serde_json::from_str(&data).unwrap_or_default();
    if !array.contains_key("score") {
        return "not score field".into_response();
    }

    let mut inner = con.hget::<_, _, Option<String>>("ddcr_player_info", pid).await
        .ok().flatten()
        .and_then(|v| serde_json::from_str(&v).ok())
        .unwrap_or_default();

    let count = inner.get("count").and_then(|v| v.as_i64()).unwrap_or(0);
    if count < 1 {
        return "count not enough".into_response();
    }

    inner.insert("count".to_string(), serde_json::json!(count - 1));
    let index = inner.get("gameIndex").and_then(|v| v.as_i64()).unwrap_or(1);
    let player_id_new = format!("{}_{}", pid, index);
    inner.insert("gameIndex".to_string(), serde_json::json!(index + 1));

    let score = array.get("score").and_then(|v| v.as_f64()).unwrap_or(0.0);
    let _ = con.hset("ddcr_player_info", pid, serde_json::to_string(&inner).unwrap()).await;
    let _ = con.zadd("ddcr_rank_all_sort", score, &player_id_new).await;
    let _ = con.zadd("ddcr_rank_week_sort", score, &player_id_new).await;
    let _ = con.hset("ddcr_player_info_show", pid, &data).await;

    control_data(&mut con).await;
    "SUCCESS".into_response()
}

async fn handle_heartbeat(Query(params): Query<IdParams>) -> impl IntoResponse {
    let mut con = redis_conn().await;
    if let Ok(Some(val)) = con.hget::<_, _, Option<String>>("ddcr_player_info", &params.playerId).await {
        let mut data = serde_json::from_str::<HashMap<String, serde_json::Value>>(&val).unwrap_or_default();
        data.insert("loginTs".to_string(), serde_json::json!(Utc::now().timestamp()));
        let _ = con.hset("ddcr_player_info", &params.playerId, serde_json::to_string(&data).unwrap()).await;
        "SUCCESS".into_response()
    } else {
        "not data".into_response()
    }
}

async fn handle_transactions(Query(params): Query<IdParams>) -> impl IntoResponse {
    let mut con = redis_conn().await;
    let all: HashMap<String, String> = con.hgetall("pay_info").await.unwrap_or_default();
    let mut logs = vec![];

    for val in all.values() {
        if let Ok(info) = serde_json::from_str::<HashMap<String, serde_json::Value>>(val) {
            if info.get("principal").map(|v| v == &params.playerId).unwrap_or(false) {
                let mut log = HashMap::new();
                if let Some(ts) = info.get("ts") {
                    log.insert("ts", ts.clone());
                }
                log.insert("money", info.get("money").cloned().unwrap_or(serde_json::json!("0")));
                log.insert("status", serde_json::json!(if info.get("status") == Some(&serde_json::json!("over")) { 1 } else { 0 }));
                logs.push(log);
            }
        }
    }

    Json(logs).into_response()
}

async fn handle_config() -> impl IntoResponse {
    fs::read_to_string("./payConfig.json").unwrap_or_else(|_| "{}".to_string()).into_response()
}

// ---------- Helpers ----------

fn invalid_id(id: &str) -> bool {
    id.is_empty() || id.contains('_')
}

async fn redis_conn() -> Connection {
    redis::Client::open("redis://127.0.0.1/")
        .unwrap()
        .get_tokio_connection()
        .await
        .unwrap()
}

async fn get_all_money(con: &mut Connection) -> String {
    let all: HashMap<String, String> = con.hgetall("pay_info").await.unwrap_or_default();
    let mut total = 0.0;
    for val in all.values() {
        if let Ok(info) = serde_json::from_str::<HashMap<String, serde_json::Value>>(val) {
            if let Some(money) = info.get("money").and_then(|v| v.as_f64()) {
                total += money;
            }
        }
    }
    total.to_string()
}

async fn get_own_rank(con: &mut Connection, board: &str, pid: &str) -> usize {
    let prefix = format!("{}_", pid);
    let mut cursor = 0;
    let mut max_score = None;
    let mut max_id = None;

    loop {
        let (next, values): (u64, Vec<(String, f64)>) = con.zscan(board, cursor).await.unwrap_or((0, vec![]));
        for (id, score) in values {
            if id.starts_with(&prefix) {
                if max_score.map_or(true, |s| score > s) {
                    max_score = Some(score);
                    max_id = Some(id);
                }
            }
        }
        if next == 0 {
            break;
        }
        cursor = next;
    }

    if let Some(id) = max_id {
        if let Ok(Some(rank)) = con.zrevrank(board, id).await {
            return rank + 1;
        }
    }

    0
}

async fn check_login(con: &mut Connection, pid: &str) -> bool {
    if let Ok(Some(value)) = con.hget::<_, _, Option<String>>("ddcr_player_info", pid).await {
        if let Ok(data) = serde_json::from_str::<HashMap<String, serde_json::Value>>(&value) {
            if let Some(ts) = data.get("loginTs").and_then(|v| v.as_i64()) {
                return Utc::now().timestamp() <= ts + 5;
            }
        }
    }
    false
}

async fn control_data(con: &mut Connection) {
    for key in &["ddcr_rank_all_sort", "ddcr_rank_week_sort"] {
        if let Ok(count) = con.zcard(*key).await {
            if count > 2000 {
                if let Ok(Some(lowest)) = con.zrange::<_, _, Option<Vec<String>>>(*key, 0, 0).await {
                    if let Some(user) = lowest.get(0) {
                        let _ = con.zrem(*key, user).await;
                    }
                }
            }
        }
    }
}

fn write_log(log: &str) {
    let date = chrono::Local::now().format("%Y%m%d").to_string();
    let path = format!("./log/{}.log", date);
    let time = chrono::Local::now().format("%H:%M:%S").to_string();
    let entry = format!("[{}]{}\n", time, log);
    let _ = OpenOptions::new()
        .create(true)
        .append(true)
        .open(&path)
        .and_then(|mut f| f.write_all(entry.as_bytes()));
}
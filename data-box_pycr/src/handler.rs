use axum::{extract::{Query, Form}, Json, response::IntoResponse};
use chrono::Utc;
use redis::{AsyncCommands, aio::Connection};
use serde::Deserialize;
use std::{collections::HashMap, fs};

use crate::{
    check_login, control_data, get_all_money, get_own_rank, invalid_id, redis_conn, write_log,
};

#[derive(Debug, Deserialize)]
pub struct IdParams {
    pub playerId: String,
}

#[derive(Debug, Deserialize)]
pub struct LeaderboardParams {
    pub playerId: String,
    #[serde(default)]
    pub rankType: Option<u8>,
    #[serde(default)]
    pub num: Option<u32>,
}

#[derive(Debug, Deserialize)]
pub struct ScoreParams {
    pub playerId: String,
    pub sign: String,
}

pub async fn login(Query(params): Query<IdParams>) -> impl IntoResponse {
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

pub async fn user_data(Query(params): Query<IdParams>) -> impl IntoResponse {
    let mut con = redis_conn().await;
    let value: Option<String> = con.hget("ddcr_player_info", &params.playerId).await.unwrap_or(None);
    let mut data = value
        .and_then(|v| serde_json::from_str(&v).ok())
        .unwrap_or_default();
    let all = get_all_money(&mut con).await;
    data.insert("allMondy".to_string(), serde_json::json!(all));
    Json(data).into_response()
}

pub async fn leaderboard(Query(params): Query<LeaderboardParams>) -> impl IntoResponse {
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

pub async fn heartbeat(Query(params): Query<IdParams>) -> impl IntoResponse {
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

pub async fn transactions(Query(params): Query<IdParams>) -> impl IntoResponse {
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

pub async fn config() -> impl IntoResponse {
    fs::read_to_string("./payConfig.json").unwrap_or_else(|_| "{}".to_string()).into_response()
}
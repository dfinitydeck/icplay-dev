use crate::{access_guard::anonymous_guard, payment::default_account_id};
use candid::Principal;
use canister_types::{
    payment::{QueryCommonReq, QueryOrderResp},
    user::{UserInfo, UserProfileInfo},
};
use ic_cdk::query;

use crate::data_store;

#[query]
fn my_profile() -> Option<UserInfo> {
    let user_pid = ic_cdk::caller();
    match data_store::user::get_user(user_pid) {
        Some(wrapper) => Some(wrapper.into_inner().to_user_info(user_pid)),
        None => None,
    }
}

#[query]
fn fetch_user_info(pid: Principal) -> Option<UserInfo> {
    match data_store::user::get_user(pid) {
        Some(wrapper) => Some(wrapper.into_inner().to_user_info(pid)),
        None => None,
    }
}

#[query]
fn fetch_users_info(pids: Vec<Principal>) -> Vec<UserInfo> {
    data_store::user::get_user_infos(pids)
}

#[query]
fn total_user_count() -> u64 {
    data_store::user::get_user_count()
}

#[query]
fn all_user_pids() -> Vec<Principal> {
    data_store::user::get_user_pids()
}

#[query]
fn fetch_avatar(user: Option<Principal>) -> String {
    let pid = user.unwrap_or_else(ic_cdk::caller);
    match data_store::user::get_user(pid) {
        Some(wrapper) => wrapper.into_inner().avatar.clone(),
        None => "".to_string(),
    }
}

#[query]
fn fetch_email(user: Option<Principal>) -> String {
    let pid = user.unwrap_or_else(ic_cdk::caller);
    match data_store::user::get_user(pid) {
        Some(wrapper) => wrapper.into_inner().email.clone(),
        None => "".to_string(),
    }
}

#[query]
fn fetch_user_spaces(user: Option<Principal>) -> Vec<UserProfileInfo> {
    let pid = user.unwrap_or_else(ic_cdk::caller);
    match data_store::user::get_user(pid) {
        Some(wrapper) => wrapper.into_inner().spaces.clone(),
        None => Vec::new(),
    }
}

#[query(guard = "anonymous_guard")]
pub fn query_user_orders(req: QueryCommonReq) -> QueryOrderResp {
    let caller = ic_cdk::caller();
    let (total, has_more, data) = data_store::payment::limit_orders(caller, &req);
    QueryOrderResp {
        page: req.page,
        total,
        has_more,
        data,
    }
}

#[ic_cdk::query]
pub fn canister_main_account() -> (String, Vec<u8>) {
    let account_id = default_account_id();
    let account_vec = account_id.as_ref().to_vec();
    (account_id.to_hex(), account_vec)
}

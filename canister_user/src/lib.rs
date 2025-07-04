mod cycles_ops;
mod user_init;
mod query_api;
mod update_api;
pub mod candid_generator;
mod access_guard;
mod payment;
mod data_store;
mod helper_utils;

const MAX_PROFILE_SIZE: u8 = 1;

ic_cdk::export_candid!();

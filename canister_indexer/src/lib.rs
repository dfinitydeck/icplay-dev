use ic_cdk::export_candid;

mod cycles_handler;
mod initialization;
mod query_operations;
mod update_operations;
pub mod candid_generator;
mod data_storage;

export_candid!();

pub const MAX_MSG_COUNT: u64 = 2000;
pub const MAX_HISTORY_MSG_COUNT: u64 = 20000;
pub const ARCHIVE_MSG_DEFAULT_CYCLES: u128 = 1_000_000_000_000;
pub const ARCHIVE_MSG_THRESHOLD: usize = 5000;
pub const ARCHIVE_MSG_MIGRATION_SIZE: usize = 500;

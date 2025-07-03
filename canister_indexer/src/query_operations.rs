use candid::Principal;
use canister_types::message::Message;
use ic_cdk::query;

use crate::data_storage;

#[query]
fn retrieve_msg_count() -> (Vec<(String, usize)>, usize) {
    data_storage::message::get_message_size()
}

#[query]
fn fetch_msg(msg_type: String, msg_id: String) -> Option<(Message, Principal)> {
    data_storage::message::get_message(&msg_type, &msg_id)
}

#[query]
fn get_msg_categories() -> Vec<String> {
    data_storage::message::get_message_keys()
}

#[query]
fn fetch_msg_batch(
    msg_type: String,
    max_count: usize,
    start_pos: usize,
) -> Vec<(Message, Principal)> {
    data_storage::message::get_message_list(msg_type.as_str(), max_count, start_pos)
}

#[query]
fn fetch_msg_by_user(
    msg_type: String,
    user_id: Principal,
    max_count: usize,
    start_pos: usize,
) -> Vec<(Message, Principal)> {
    data_storage::message::get_message_list_by_pid(msg_type.as_str(), user_id, max_count, start_pos)
}

#[query]
fn fetch_archive_msg_batch(
    msg_type: String,
    max_count: usize,
    start_pos: usize,
) -> Vec<(Message, Principal)> {
    data_storage::history::get_history_message_list(msg_type.as_str(), max_count, start_pos)
}

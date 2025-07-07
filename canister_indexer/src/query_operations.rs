use candid::Principal;
use canister_types::message::Message;
use ic_cdk::query;

use crate::data_storage;

/// Query function to retrieve message count statistics
/// 
/// Returns a tuple containing:
/// - Vec<(String, usize)>: List of message types with their respective counts
/// - usize: Total count of all messages across all types
#[query]
fn retrieve_msg_count() -> (Vec<(String, usize)>, usize) {
    data_storage::message::get_message_size()
}

/// Query function to fetch a specific message by its type and ID
/// 
/// # Arguments
/// * `msg_type` - The type/category of the message
/// * `msg_id` - The unique identifier of the message
/// 
/// # Returns
/// * `Option<(Message, Principal)>` - The message and its associated principal if found, None otherwise
#[query]
fn fetch_msg(msg_type: String, msg_id: String) -> Option<(Message, Principal)> {
    data_storage::message::get_message(&msg_type, &msg_id)
}

/// Query function to get all available message categories/types
/// 
/// # Returns
/// * `Vec<String>` - List of all message type keys currently stored
#[query]
fn get_msg_categories() -> Vec<String> {
    data_storage::message::get_message_keys()
}

/// Query function to fetch a batch of messages with pagination support
/// 
/// # Arguments
/// * `msg_type` - The type/category of messages to retrieve
/// * `max_count` - Maximum number of messages to return (pagination limit)
/// * `start_pos` - Starting position for pagination (offset)
/// 
/// # Returns
/// * `Vec<(Message, Principal)>` - List of messages with their associated principals
/// 
/// # Note
/// Messages are returned sorted by timestamp in descending order (newest first)
#[query]
fn fetch_msg_batch(
    msg_type: String,
    max_count: usize,
    start_pos: usize,
) -> Vec<(Message, Principal)> {
    data_storage::message::get_message_list(msg_type.as_str(), max_count, start_pos)
}

/// Query function to fetch messages filtered by user principal ID with pagination
/// 
/// # Arguments
/// * `msg_type` - The type/category of messages to retrieve
/// * `user_id` - The principal ID of the user whose messages to retrieve
/// * `max_count` - Maximum number of messages to return (pagination limit)
/// * `start_pos` - Starting position for pagination (offset)
/// 
/// # Returns
/// * `Vec<(Message, Principal)>` - List of messages associated with the specified user
/// 
/// # Note
/// This function filters messages by the principal ID that created them
#[query]
fn fetch_msg_by_user(
    msg_type: String,
    user_id: Principal,
    max_count: usize,
    start_pos: usize,
) -> Vec<(Message, Principal)> {
    data_storage::message::get_message_list_by_pid(msg_type.as_str(), user_id, max_count, start_pos)
}

/// Query function to fetch archived messages with pagination support
/// 
/// # Arguments
/// * `msg_type` - The type/category of archived messages to retrieve
/// * `max_count` - Maximum number of messages to return (pagination limit)
/// * `start_pos` - Starting position for pagination (offset)
/// 
/// # Returns
/// * `Vec<(Message, Principal)>` - List of archived messages with their associated principals
/// 
/// # Note
/// This function retrieves messages from the archive store, which contains
/// older messages that have been migrated from the main message store
#[query]
fn fetch_archive_msg_batch(
    msg_type: String,
    max_count: usize,
    start_pos: usize,
) -> Vec<(Message, Principal)> {
    data_storage::history::get_history_message_list(msg_type.as_str(), max_count, start_pos)
}

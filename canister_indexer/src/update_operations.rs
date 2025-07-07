use canister_types::message::Message;
use ic_cdk::update;

use crate::data_storage;

// Type alias for the result type used in this module
type Result_0<T, E> = Result<T, E>;

/// Process a single message asynchronously
/// 
/// This function handles the processing of individual messages by delegating
/// to the data storage layer. It validates the caller and processes the message
/// according to its type and payload.
/// 
/// # Arguments
/// * `msg` - The message to be processed
/// 
/// # Returns
/// * `Result_0<String, String>` - Success with message ID or error with description
/// 
/// # Errors
/// * Returns error if message processing fails
/// * Returns error if caller validation fails
#[update]
async fn process_single_msg(msg: Message) -> Result_0<String, String> {
    // Validate message structure before processing
    if msg.msg_id.is_empty() {
        return Err("Message ID cannot be empty".to_string());
    }
    
    if msg.payload_type.is_empty() {
        return Err("Payload type cannot be empty".to_string());
    }
    
    // Get the caller principal for authentication
    let caller = ic_cdk::caller();
    
    // Process the message through the data storage layer
    data_storage::message::process_message(msg, caller).await
}

/// Process multiple messages in batch asynchronously
/// 
/// This function handles batch processing of messages with improved error handling.
/// It processes each message individually and returns the count of successfully
/// processed messages. If any message fails, the entire operation fails.
/// 
/// # Arguments
/// * `messages` - Vector of messages to be processed
/// 
/// # Returns
/// * `Result_0<usize, String>` - Success with count of processed messages or error
/// 
/// # Errors
/// * Returns error if any message processing fails
/// * Returns error if input validation fails
/// * Returns error if caller validation fails
#[update]
async fn process_multiple_msgs(messages: Vec<Message>) -> Result_0<usize, String> {
    // Validate input parameters
    if messages.is_empty() {
        return Err("Cannot process empty message batch".to_string());
    }
    
    // Check for reasonable batch size to prevent DoS attacks
    const MAX_BATCH_SIZE: usize = 100;
    if messages.len() > MAX_BATCH_SIZE {
        return Err(format!(
            "Batch size {} exceeds maximum allowed size of {}",
            messages.len(),
            MAX_BATCH_SIZE
        ));
    }
    
    // Validate each message in the batch
    for (index, msg) in messages.iter().enumerate() {
        if msg.msg_id.is_empty() {
            return Err(format!("Message at index {} has empty ID", index));
        }
        
        if msg.payload_type.is_empty() {
            return Err(format!("Message at index {} has empty payload type", index));
        }
    }
    
    let mut processed_count = 0;
    let sender = ic_cdk::caller();
    
    // Process each message in the batch
    for (index, msg) in messages.into_iter().enumerate() {
        match data_storage::message::process_message(msg, sender).await {
            Ok(_) => {
                processed_count += 1;
            }
            Err(error) => {
                // Return detailed error information including the failed message index
                return Err(format!(
                    "Failed to process message at index {}: {}",
                    index,
                    error
                ));
            }
        }
    }
    
    Ok(processed_count)
}

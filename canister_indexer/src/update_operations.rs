use canister_types::message::Message;
use ic_cdk::update;

use crate::data_storage;

#[update]
async fn process_single_msg(msg: Message) -> Result_0<String, String> {
    data_storage::message::process_message(msg, ic_cdk::caller()).await
}

#[update]
async fn process_multiple_msgs(messages: Vec<Message>) -> Result_0<usize, String> {
    let mut processed_count = 0;
    let sender = ic_cdk::caller();

    for msg in messages {
        match data_storage::message::process_message(msg, sender).await {
            Ok(_) => processed_count += 1,
            Err(error) => return Err(error),
        }
    }

    Ok(processed_count)
}

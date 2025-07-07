use candid::{CandidType, Decode, Encode, Principal};

use canister_types::message::Message;
use ciborium::{from_reader, into_writer};
use ic_cdk_timers::TimerId;
use ic_stable_structures::{
    memory_manager::{MemoryId, MemoryManager, VirtualMemory},
    storable::Bound,
    DefaultMemoryImpl, StableBTreeMap, StableCell, Storable,
};
use serde::{Deserialize, Serialize};
use std::{borrow::Cow, cell::RefCell, collections::BTreeSet};
use crate::{ARCHIVE_MSG_MIGRATION_SIZE, ARCHIVE_MSG_THRESHOLD};

type MemSpace = VirtualMemory<DefaultMemoryImpl>;

/// Data processor configuration and state
#[derive(CandidType, Clone, Deserialize, Serialize, Debug)]
pub struct DataProcessor {
    pub identifier: String,
    pub controller: Principal,
    pub participant_count: u32,
}

impl Default for DataProcessor {
    fn default() -> Self {
        Self {
            identifier: String::from("default_processor"),
            controller: Principal::anonymous(),
            participant_count: 0,
        }
    }
}

impl Storable for DataProcessor {
    const BOUND: Bound = Bound::Unbounded;

    fn to_bytes(&self) -> Cow<[u8]> {
        let mut buffer = vec![];
        into_writer(self, &mut buffer).expect("failed to encode DataProcessor data");
        Cow::Owned(buffer)
    }

    fn from_bytes(bytes: Cow<'_, [u8]>) -> Self {
        from_reader(&bytes[..]).expect("failed to decode DataProcessor data")
    }
}

/// Collection of messages with associated principals
#[derive(CandidType, Clone, Deserialize, Serialize, Debug)]
pub struct MsgCollection(pub BTreeSet<(Message, Principal)>);

impl Storable for MsgCollection {
    const BOUND: Bound = Bound::Unbounded;

    fn from_bytes(bytes: std::borrow::Cow<[u8]>) -> Self {
        Decode!(bytes.as_ref(), Self).unwrap()
    }

    fn to_bytes(&self) -> std::borrow::Cow<[u8]> {
        std::borrow::Cow::Owned(Encode!(self).unwrap())
    }
}

impl MsgCollection {
    /// Extract the underlying message collection
    pub fn extract_content(self) -> BTreeSet<(Message, Principal)> {
        self.0
    }

    /// Create a new empty message collection
    pub fn create_new() -> Self {
        MsgCollection(BTreeSet::new())
    }

    /// Insert a message with its associated principal
    pub fn insert_msg(&mut self, msg: Message, principal: Principal) {
        self.0.insert((msg, principal));
    }

    /// Remove a message by its identifier
    pub fn remove_msg(&mut self, msg_identifier: &str) {
        self.0.retain(|(msg, _)| msg.msg_id != msg_identifier);
    }

    /// Find a message by its identifier
    pub fn find_msg(&self, msg_identifier: &str) -> Option<(Message, Principal)> {
        self.0
            .iter()
            .find(|(msg, _)| msg.msg_id == msg_identifier)
            .cloned()
    }

    /// Check if the collection has any content
    pub fn has_content(&self) -> bool {
        !self.0.is_empty()
    }

    /// Get the number of messages in the collection
    pub fn len(&self) -> usize {
        self.0.len()
    }

    /// Check if the collection is empty
    pub fn is_empty(&self) -> bool {
        self.0.is_empty()
    }
}

// Memory management constants
const PROCESSOR_MEM_ID: MemoryId = MemoryId::new(0);
const MSG_MEM_ID: MemoryId = MemoryId::new(1);
const ARCHIVE_MEM_ID: MemoryId = MemoryId::new(2);

// Thread-local storage for canister state
thread_local! {
    static PROCESSOR: RefCell<DataProcessor> = RefCell::new(DataProcessor::default());

    static MEM_MANAGER: RefCell<MemoryManager<DefaultMemoryImpl>> =
        RefCell::new(MemoryManager::init(DefaultMemoryImpl::default()));

    static PROCESSOR_STORE: RefCell<StableCell<DataProcessor, MemSpace>> = RefCell::new(
        StableCell::init(
            MEM_MANAGER.with_borrow(|m| m.get(PROCESSOR_MEM_ID)),
            DataProcessor::default()
        ).expect("failed to init PROCESSOR_STORE store")
    );

    static MSG_STORE: RefCell<StableBTreeMap<String, MsgCollection, MemSpace>> = RefCell::new(
        StableBTreeMap::init(
            MEM_MANAGER.with_borrow(|m| m.get(MSG_MEM_ID)),
        )
    );

    static ARCHIVE_STORE: RefCell<StableBTreeMap<String, MsgCollection, MemSpace>> = RefCell::new(
        StableBTreeMap::init(
            MEM_MANAGER.with_borrow(|m| m.get(ARCHIVE_MEM_ID)),
        )
    );

    pub static TIMER_LIST: RefCell<Vec<TimerId>> = RefCell::new(Vec::new());
}

/// Scheduler module for handling cleanup operations
pub mod scheduler {
    use super::*;
    use std::time::Duration;

    /// Setup periodic cleanup scheduler
    pub fn setup_cleanup_scheduler() {
        let interval = Duration::from_secs(10);
        let cleanup_job = async {
            execute_cleanup_task().await;
        };
        let timer_id = ic_cdk_timers::set_timer(interval, move || {
            ic_cdk::spawn(cleanup_job);
        });
        TIMER_LIST.with(|timer_list| timer_list.borrow_mut().push(timer_id));
    }
    
    /// Execute cleanup task to migrate old messages to archive
    async fn execute_cleanup_task() {
        // Collect keys that need migration from MSG_STORE
        let keys_to_migrate = MSG_STORE.with(|msg_store| {
            let msg_store = msg_store.borrow();
    
            let mut keys = Vec::new();
            for (key, msg_collection) in msg_store.iter() {
                if msg_collection.len() > ARCHIVE_MSG_THRESHOLD {
                    ic_cdk::println!("execute_cleanup_task: key {} exceeds threshold, migrating data to ARCHIVE_STORE", key);
                    keys.push(key.clone());
                }
            }
            keys
        });
    
        // Process each key for migration and deletion
        for key in keys_to_migrate {
            if let Err(e) = migrate_messages_for_key(&key).await {
                ic_cdk::println!("execute_cleanup_task: failed to migrate key {}: {}", key, e);
            }
        }
    
        ic_cdk::println!("execute_cleanup_task: Completed cleanup task");
    
        // Re-schedule the cleanup task
        setup_cleanup_scheduler();
    }

    /// Migrate messages for a specific key from MSG_STORE to ARCHIVE_STORE
    async fn migrate_messages_for_key(key: &str) -> Result<(), String> {
        // Step 1: Extract messages to migrate
        let msgs_to_migrate = MSG_STORE.with(|msg_store| {
            let msg_store = msg_store.borrow();
            if let Some(msg_collection) = msg_store.get(key) {
                // Collect the oldest messages for migration
                let msgs: Vec<_> = msg_collection
                    .0
                    .iter()
                    .take(ARCHIVE_MSG_MIGRATION_SIZE)
                    .cloned()
                    .collect();
                Some(msgs)
            } else {
                None
            }
        });

        // Proceed only if there are messages to migrate
        if let Some(msgs_to_migrate) = msgs_to_migrate {
            // Step 2: Save data in ARCHIVE_STORE
            ARCHIVE_STORE.with(|archive_store| {
                let mut archive_store = archive_store.borrow_mut();
                
                // Retrieve existing archive set or create a new one
                let mut archive_set = archive_store.get(key).unwrap_or_else(MsgCollection::create_new);

                // Add messages to ARCHIVE_STORE
                for (msg, principal) in &msgs_to_migrate {
                    archive_set.insert_msg(msg.clone(), principal.clone());
                }

                // Reinsert the modified archive set
                archive_store.insert(key.to_string(), archive_set);

                ic_cdk::println!(
                    "execute_cleanup_task: migrated {} messages from key {} to ARCHIVE_STORE",
                    msgs_to_migrate.len(), key
                );
            });

            // Step 3: Remove migrated messages from MSG_STORE
            MSG_STORE.with(|msg_store| {
                let mut msg_store = msg_store.borrow_mut();
                if let Some(mut msg_collection) = msg_store.get(key) {
                    for (msg, principal) in msgs_to_migrate {
                        msg_collection.0.remove(&(msg, principal));
                    }

                    // Reinsert the modified message set
                    msg_store.insert(key.to_string(), msg_collection);
                    
                    ic_cdk::println!(
                        "execute_cleanup_task: removed {} messages from key {} in MSG_STORE",
                        ARCHIVE_MSG_MIGRATION_SIZE, key
                    );
                }
            });
        }

        Ok(())
    }
}

/// State management module
pub mod state {
    use super::*;

    /// Execute a function with immutable access to the processor state
    #[allow(dead_code)]
    pub fn with<R>(f: impl FnOnce(&DataProcessor) -> R) -> R {
        PROCESSOR.with(|r| f(&r.borrow()))
    }

    /// Execute a function with mutable access to the processor state
    pub fn with_mut<R>(f: impl FnOnce(&mut DataProcessor) -> R) -> R {
        PROCESSOR.with(|r| f(&mut r.borrow_mut()))
    }

    /// Load processor state from stable storage
    pub fn load() {
        PROCESSOR_STORE.with(|r| {
            let s = r.borrow().get().clone();
            PROCESSOR.with(|h| {
                *h.borrow_mut() = s;
            });
        });
    }

    /// Save processor state to stable storage
    pub fn save() {
        PROCESSOR.with(|h| {
            PROCESSOR_STORE.with(|r| {
                r.borrow_mut()
                    .set(h.borrow().clone())
                    .expect("failed to set PROCESSOR_STORE data");
            });
        });
    }
}

/// Message management module
pub mod message {
    use super::*;
    use canister_types::message::{MessageType, MsgSharePlay, MsgUserInfo, MsgUserPost};
    use ic_cdk::print;

    /// Get message size statistics for all message types
    pub fn get_message_size() -> (Vec<(String, usize)>, usize) {
        MSG_STORE.with(|store| {
            let store_ref = store.borrow();
            let mut set_sizes = Vec::new();
            let mut total_size = 0;
    
            for (key, wrapper) in store_ref.iter() {
                let set_size = wrapper.len();
                total_size += set_size;
                set_sizes.push((key.clone(), set_size));
            }
    
            (set_sizes, total_size)
        })
    }

    /// Retrieve a message by its message ID and type
    pub fn get_message(message_type: &str, message_id: &str) -> Option<(Message, Principal)> {
        MSG_STORE.with(|store| {
            store
                .borrow()
                .get(&message_type.to_string())
                .and_then(|wrapper| wrapper.find_msg(message_id))
        })
    }

    /// Get all message type keys
    pub fn get_message_keys() -> Vec<String> {
        MSG_STORE.with(|store| {
            let store_ref = store.borrow();
            store_ref.iter().map(|(key, _)| key.clone()).collect()
        })
    }

    /// Add a new message to the store
    pub fn create_message(message_type: &str, message: Message, principal: Principal) {
        let mut message_count = 0;

        MSG_STORE.with(|store| {
            let mut store_ref = store.borrow_mut();
            let message_type_key = message_type.to_string();

            if let Some(wrapper) = store_ref.get(&message_type_key) {
                let mut cloned_wrapper = wrapper.clone();
                message_count = cloned_wrapper.len();
                cloned_wrapper.insert_msg(message, principal);
                store_ref.insert(message_type_key, cloned_wrapper);
            } else {
                let mut new_wrapper = MsgCollection::create_new();
                new_wrapper.insert_msg(message, principal);
                store_ref.insert(message_type_key, new_wrapper);
            }
        });

        // Setup cleanup scheduler if message count exceeds threshold
        if message_count > ARCHIVE_MSG_THRESHOLD {
            scheduler::setup_cleanup_scheduler();
        }
    }

    /// Retrieve a list of messages for a given message type with pagination
    pub fn get_message_list(
        message_type: &str,
        limit: usize,
        offset: usize,
    ) -> Vec<(Message, Principal)> {
        MSG_STORE.with(|store| {
            let store_ref = store.borrow();
    
            if let Some(wrapper) = store_ref.get(&message_type.to_string()) {
                let mut messages: Vec<_> = wrapper.0.iter().cloned().collect();
                messages.sort_by(|a, b| b.0.timestamp.cmp(&a.0.timestamp));
                messages.into_iter().skip(offset).take(limit).collect()
            } else {
                Vec::new()
            }
        })
    }

    /// Retrieve messages filtered by principal ID with pagination
    pub fn get_message_list_by_pid(
        message_type: &str,
        pid: Principal,
        limit: usize,
        offset: usize,
    ) -> Vec<(Message, Principal)> {
        MSG_STORE.with(|store| {
            let store_ref = store.borrow();

            if let Some(wrapper) = store_ref.get(&message_type.to_string()) {
                wrapper
                    .0
                    .iter()
                    .filter(|(_, p)| p == &pid)
                    .skip(offset)
                    .take(limit)
                    .cloned()
                    .collect()
            } else {
                Vec::new()
            }
        })
    }

    /// Delete a message by its message ID and message type
    pub fn delete_message(message_type: &str, message_id: &str) -> Result_0<(), String> {
        // Try to delete from MSG_STORE first
        let message_delete_result = delete_from_store(message_type, message_id, &MSG_STORE);
    
        // If not found in MSG_STORE, try ARCHIVE_STORE
        if let Err(_) = message_delete_result {
            delete_from_store(message_type, message_id, &ARCHIVE_STORE)
        } else {
            message_delete_result
        }
    }

    /// Helper function to delete message from a specific store
    fn delete_from_store(
        message_type: &str,
        message_id: &str,
        store: &'static std::thread::LocalKey<RefCell<StableBTreeMap<String, MsgCollection, MemSpace>>>
    ) -> Result_0<(), String> {
        store.with(|store_ref| {
            let mut store_ref = store_ref.borrow_mut();
            let message_type_key = message_type.to_string();

            if let Some(wrapper) = store_ref.get(&message_type_key) {
                let mut cloned_wrapper = wrapper.clone();
                cloned_wrapper.remove_msg(message_id);
    
                if cloned_wrapper.has_content() {
                    store_ref.insert(message_type_key, cloned_wrapper);
                } else {
                    store_ref.remove(&message_type_key);
                }
                Ok(())
            } else {
                Err(format!("Message not found in store for type '{}'", message_type))
            }
        })
    }

    /// Process incoming message based on its payload type and message type
    pub async fn process_message(msg: Message, caller: Principal) -> Result_0<String, String> {
        let msg_id = msg.msg_id.clone();
    
        // Match on the payload type and decode accordingly
        match msg.payload_type.as_str() {
            "MsgUserInfo" => {
                let user_info: MsgUserInfo = msg.decode_payload()?;
                print(format!(
                    "Received user info for id {}: {:?}",
                    &msg_id, user_info
                ));
                handle_message_operation(&msg.msg_type, "MsgUserInfo", &msg_id, &msg, caller).await?;
            }
            "MsgUserPost" => {
                let user_post: MsgUserPost = msg.decode_payload()?;
                print(format!(
                    "Received user post for id {}: {:?}",
                    &msg_id, user_post
                ));
                handle_message_operation(&msg.msg_type, "MsgUserPost", &msg_id, &msg, caller).await?;
            }
            "MsgSharePlay" => {
                let share_game: MsgSharePlay = msg.decode_payload()?;
                handle_message_operation(&msg.msg_type, "MsgSharePlay", &msg_id, &msg, caller).await?;
            }
            _ => {
                return Err(format!(
                    "Unknown payload_type for id {}: {}",
                    &msg_id, msg.payload_type
                ));
            }
        }
    
        Ok(msg_id)
    }
    
    /// Utility function to handle message operations (Create, Delete, Update)
    async fn handle_message_operation(
        msg_type: &MessageType,
        msg_name: &str,
        msg_id: &String,
        msg: &Message,
        caller: Principal,
    ) -> Result_0<(), String> {
        match msg_type {
            MessageType::Create => {
                create_message(msg_name, msg.clone(), caller);
            }
            MessageType::Delete => {
                delete_message(msg_name, msg_id)?;
            }
            MessageType::Update => {
                // For Update, delete the existing message first, then create a new one
                delete_message(msg_name, msg_id)?;
                create_message(msg_name, msg.clone(), caller);
            }
            _ => {
                return Err(format!(
                    "Unsupported message type for id {}: {:?}",
                    msg_id, msg_type
                ));
            }
        }
        Ok(())
    }
}

/// History management module for archived messages
pub mod history {
    use super::*;

    /// Retrieve archived messages for a given message type with pagination
    pub fn get_history_message_list(
        message_type: &str,
        limit: usize,
        offset: usize,
    ) -> Vec<(Message, Principal)> {
        ARCHIVE_STORE.with(|store| {
            let store_ref = store.borrow();

            if let Some(wrapper) = store_ref.get(&message_type.to_string()) {
                wrapper
                    .0
                    .iter()
                    .skip(offset)
                    .take(limit)
                    .cloned()
                    .collect()
            } else {
                Vec::new()
            }
        })
    }
}
use candid::{CandidType, Principal};
use canister_types::constant::Environment;
use ic_cdk::{init, post_upgrade, pre_upgrade, trap};
use serde::Deserialize;

use crate::data_store;

#[derive(CandidType, Clone, Debug, Deserialize)]
pub enum UserCanisterArgs {
    Init(UserStateInitArgs),
    Upgrade(UserStateUpgradeArgs),
}

#[derive(CandidType, Clone, Debug, Deserialize)]
pub struct UserStateInitArgs {
    pub uname: String,
    pub uowner: Principal,
    pub uenv: Environment,
    pub dao_id: Principal,
    pub indexer_id: Option<Principal>,
}

#[derive(CandidType, Clone, Debug, Deserialize)]
pub struct UserStateUpgradeArgs {
    pub uname: Option<String>,
    pub uowner: Option<Principal>,
    pub uenv: Option<Environment>,
    pub dao_id: Option<Principal>,
    pub indexer_id: Option<Principal>,
}

#[init]
fn user_init(args: Option<UserCanisterArgs>) {
    match args {
        Some(UserCanisterArgs::Init(init_args)) => {
            data_store::state::with_mut(|state_ref| {
                state_ref.name = init_args.uname;
                state_ref.owner = init_args.uowner;
                state_ref.dao_canister_id = init_args.dao_id;
                state_ref.env = init_args.uenv;
                state_ref.indexer_canister_id = init_args.indexer_id.unwrap_or(Principal::anonymous());
                state_ref.user_count = 0;
            });
            data_store::state::save();
        }
        Some(UserCanisterArgs::Upgrade(_)) => {
            ic_cdk::trap("Cannot initialize with Upgrade args. Please use Init args.");
        }
        None => {
            trap("No initialization arguments provided. Using default initialization.");
        }
    }
}

#[pre_upgrade]
fn before_upgrade() {
    data_store::state::save();
}

#[post_upgrade]
fn after_upgrade(args: Option<UserCanisterArgs>) {
    data_store::state::load();
    match args {
        Some(UserCanisterArgs::Upgrade(upgrade_args)) => {
            data_store::state::with_mut(|state_ref| {
                if let Some(name) = upgrade_args.uname {
                    state_ref.name = name;
                }
                if let Some(owner) = upgrade_args.uowner {
                    state_ref.owner = owner;
                }
                if let Some(dao_id) = upgrade_args.dao_id {
                    state_ref.dao_canister_id = dao_id;
                }
                if let Some(indexer_id) = upgrade_args.indexer_id {
                    state_ref.indexer_canister_id = indexer_id;
                }
                if let Some(env) = upgrade_args.uenv {
                    state_ref.env = env;
                }
            });
        }
        Some(UserCanisterArgs::Init(_)) => {
            ic_cdk::trap("Cannot upgrade with Init args. Please use Upgrade args.");
        }
        None => {
            // No arguments provided, continue using current state
        }
    }
}

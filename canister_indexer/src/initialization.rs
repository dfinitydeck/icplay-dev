use canister_types::indexer::CanisterArgs;

use crate::data_storage;

#[ic_cdk::init]
fn initialize_system(args: Option<CanisterArgs>) {
    match args {
        Some(CanisterArgs::Init(init_params)) => {
            data_storage::state::with_mut(|processor| {
                if !init_params.name.is_empty() {
                    processor.identifier = init_params.name;
                }
                processor.controller = init_params.owner;
                processor.participant_count = 0;
            });
            data_storage::state::save();
        }
        Some(CanisterArgs::Upgrade(_)) => {
            ic_cdk::trap(
                "Cannot initialize the canister with an Upgrade args. Please provide an Init args.",
            );
        }
        None => {
            ic_cdk::trap("No initialization arguments provided");
        }
    }
}

#[ic_cdk::pre_upgrade]
fn before_upgrade() {
    data_storage::state::save();
}

#[ic_cdk::post_upgrade]
fn after_upgrade(upgrade_args: Option<CanisterArgs>) {
    data_storage::state::load();
    match upgrade_args {
        Some(CanisterArgs::Upgrade(upgrade_params)) => {
            data_storage::state::with_mut(|processor| {
                if let Some(new_owner) = upgrade_params.owner {
                    processor.controller = new_owner;
                }
                if let Some(new_user_count) = upgrade_params.user_count {
                    processor.participant_count = new_user_count;
                }
            });
        }
        Some(CanisterArgs::Init(_)) => {
            ic_cdk::trap(
                "Cannot upgrade the canister with an Init args. Please provide an Upgrade args.",
            );
        }
        _ => {}
    }
}

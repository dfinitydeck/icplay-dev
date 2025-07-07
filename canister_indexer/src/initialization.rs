use canister_types::indexer::CanisterArgs;

use crate::data_storage;

/// Initialize the indexer canister with provided arguments
/// 
/// This function handles the initial setup of the canister state including:
/// - Setting the processor identifier and controller
/// - Initializing participant count
/// - Validating input arguments
/// 
/// # Arguments
/// * `args` - Optional initialization arguments containing either Init or Upgrade parameters
/// 
/// # Panics
/// This function will trap (panic) if:
/// - Upgrade arguments are provided during initialization
/// - No arguments are provided
#[ic_cdk::init]
fn initialize_system(args: Option<CanisterArgs>) {
    match args {
        Some(CanisterArgs::Init(init_params)) => {
            // Initialize the data processor with provided parameters
            data_storage::state::with_mut(|processor| {
                // Set processor identifier if provided (non-empty)
                if !init_params.name.is_empty() {
                    processor.identifier = init_params.name;
                }
                
                // Set the controller (owner) of the canister
                processor.controller = init_params.owner;
                
                // Initialize participant count to zero
                processor.participant_count = 0;
            });
            
            // Persist the initialized state
            data_storage::state::save();
        }
        Some(CanisterArgs::Upgrade(_)) => {
            ic_cdk::trap(
                "Cannot initialize the canister with Upgrade arguments. Please provide Init arguments.",
            );
        }
        None => {
            ic_cdk::trap("No initialization arguments provided. Please provide Init arguments.");
        }
    }
}

/// Pre-upgrade hook to save current state before upgrade
/// 
/// This function is called before the canister is upgraded and ensures
/// that all current state is properly persisted to stable storage.
#[ic_cdk::pre_upgrade]
fn before_upgrade() {
    data_storage::state::save();
}

/// Post-upgrade hook to restore and update state after upgrade
/// 
/// This function handles the state restoration and optional parameter updates
/// after a canister upgrade. It loads the saved state and applies any
/// new configuration parameters provided in the upgrade arguments.
/// 
/// # Arguments
/// * `upgrade_args` - Optional upgrade arguments containing new configuration
/// 
/// # Panics
/// This function will trap if Init arguments are provided during upgrade
#[ic_cdk::post_upgrade]
fn after_upgrade(upgrade_args: Option<CanisterArgs>) {
    // Restore state from stable storage
    data_storage::state::load();
    
    match upgrade_args {
        Some(CanisterArgs::Upgrade(upgrade_params)) => {
            // Apply optional parameter updates
            data_storage::state::with_mut(|processor| {
                // Update controller if new owner is provided
                if let Some(new_owner) = upgrade_params.owner {
                    processor.controller = new_owner;
                }
                
                // Update participant count if new count is provided
                if let Some(new_user_count) = upgrade_params.user_count {
                    processor.participant_count = new_user_count;
                }
            });
        }
        Some(CanisterArgs::Init(_)) => {
            ic_cdk::trap(
                "Cannot upgrade the canister with Init arguments. Please provide Upgrade arguments.",
            );
        }
        None => {
            // No upgrade parameters provided, just restore state
            // This is a valid scenario for upgrades without parameter changes
        }
    }
}

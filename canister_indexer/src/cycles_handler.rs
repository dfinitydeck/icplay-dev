use ic_cdk::{query, update};

/// Result structure for cycle transfer operations
#[derive(candid::CandidType, candid::Deserialize, Debug)]
pub struct CycleTransferResult {
    /// Number of cycles received in the transfer
    received: u64,
}

/// Query function to get the current cycle balance of the canister
/// 
/// Returns the current balance as a candid::Nat
#[query]
pub fn get_cycle_balance() -> candid::Nat {
    candid::Nat::from(ic_cdk::api::canister_balance128())
}

/// Update function to accept incoming cycles
/// 
/// This function accepts all available cycles from the caller.
/// Returns a CycleTransferResult containing the number of cycles received.
/// If no cycles are available, returns 0.
#[update]
pub fn accept_cycles() -> CycleTransferResult {
    // Get the number of cycles available for acceptance
    let available_cycles = ic_cdk::api::call::msg_cycles_available128();

    // If no cycles are available, return early with 0 received
    if available_cycles == 0 {
        return CycleTransferResult { received: 0 };
    }

    // Accept all available cycles
    let accepted_cycles = ic_cdk::api::call::msg_cycles_accept128(available_cycles);
    
    // Verify that we accepted the expected number of cycles
    // This assertion ensures the cycle acceptance worked correctly
    assert_eq!(accepted_cycles, available_cycles, "Failed to accept expected number of cycles");

    CycleTransferResult {
        received: accepted_cycles as u64,
    }
}

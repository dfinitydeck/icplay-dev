// Cycle management utilities for canister
use ic_cdk::{query, update};

/// Result of cycle acceptance operation
#[derive(candid::CandidType, candid::Deserialize, Debug)]
pub struct CycleAcceptResult {
    pub transferred: candid::Nat,
}

/// Query the current available cycles of the canister
#[query]
pub fn get_available_cycles() -> candid::Nat {
    candid::Nat::from(ic_cdk::api::canister_balance128())
}

/// Accept all available cycles sent with the call and return the amount accepted
#[update]
pub fn transfer_cycles() -> CycleAcceptResult {
    let available_amount = ic_cdk::api::call::msg_cycles_available128();
    if available_amount == 0 {
        return CycleAcceptResult { transferred: candid::Nat::from(0u8) };
    }
    let transferred_amount = ic_cdk::api::call::msg_cycles_accept128(available_amount);
    debug_assert!(transferred_amount == available_amount, "Transferred amount does not match available amount");
    CycleAcceptResult {
        transferred: candid::Nat::from(transferred_amount),
    }
}

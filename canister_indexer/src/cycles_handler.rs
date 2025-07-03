use ic_cdk::{query, update};

#[derive(candid::CandidType, candid::Deserialize, Debug)]
pub struct CycleTransferResult {
    received: u64,
}

#[query]
pub fn get_cycle_balance() -> candid::Nat {
    return candid::Nat::from(ic_cdk::api::canister_balance128());
}

#[update]
pub fn accept_cycles() -> CycleTransferResult {
    let available_cycles = ic_cdk::api::call::msg_cycles_available128();

    if available_cycles == 0 {
        return CycleTransferResult { received: 0 };
    }
    let accepted_cycles = ic_cdk::api::call::msg_cycles_accept128(available_cycles);
    assert!(accepted_cycles == available_cycles);
    CycleTransferResult {
        received: accepted_cycles as u64,
    }
}

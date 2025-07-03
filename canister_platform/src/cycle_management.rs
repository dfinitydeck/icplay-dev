use ic_cdk::{query, update};

#[derive(candid::CandidType, candid::Deserialize, Debug)]
pub struct CycleAcceptResult {
    transferred: u64,
}

#[query]
pub fn get_available_cycles() -> candid::Nat {
    return candid::Nat::from(ic_cdk::api::canister_balance128());
}

#[update]
pub fn transfer_cycles() -> CycleAcceptResult {
    let available_amount = ic_cdk::api::call::msg_cycles_available128();

    if available_amount == 0 {
        return CycleAcceptResult { transferred: 0 };
    }
    let transferred_amount = ic_cdk::api::call::msg_cycles_accept128(available_amount);
    assert!(transferred_amount == available_amount);
    CycleAcceptResult {
        transferred: transferred_amount as u64,
    }
}

use canister_types::canister::{StatusRequest, StatusResponse};
use ic_cdk::{query, update};

#[derive(candid::CandidType, candid::Deserialize, Debug)]
pub struct CycleAcceptResult {
    received: u64,
}

#[query]
pub fn get_cycle_balance() -> candid::Nat {
    return candid::Nat::from(ic_cdk::api::canister_balance128());
}

#[update]
pub fn accept_cycles() -> CycleAcceptResult {
    let available_cycles = ic_cdk::api::call::msg_cycles_available128();

    if available_cycles == 0 {
        return CycleAcceptResult { received: 0 };
    }
    let accepted_cycles = ic_cdk::api::call::msg_cycles_accept128(available_cycles);
    assert!(accepted_cycles == available_cycles);
    CycleAcceptResult {
        received: accepted_cycles as u64,
    }
}

#[query]
pub fn query_canister_status(req: StatusRequest) -> StatusResponse {
    let cycles = fetch_value(req.cycles, fetch_current_cycles);
    let memory = fetch_value(req.memory_size, fetch_current_memory);
    let heap = fetch_value(req.heap_memory_size, fetch_current_heap);

    StatusResponse {
        cycles,
        memory_size: memory,
        heap_memory_size: heap,
    }
}

fn fetch_value<T, F>(flag: bool, getter: F) -> Option<T>
where
    F: Fn() -> T,
{
    if flag {
        Some(getter())
    } else {
        None
    }
}

fn fetch_current_cycles() -> u64 {
    crate::helper_utils::get_cycles()
}

fn fetch_current_memory() -> u64 {
    crate::helper_utils::get_stable_memory_size() + crate::helper_utils::get_heap_memory_size()
}

fn fetch_current_heap() -> u64 {
    crate::helper_utils::get_heap_memory_size()
}

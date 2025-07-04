use candid::Principal;
use ic_ledger_types::{AccountIdentifier, Subaccount};
use sha2::{Digest, Sha224};

#[cfg(target_arch = "wasm32")]
const WASM_PAGE_SIZE: u64 = 65536;

pub fn get_cycles() -> u64 {
    #[cfg(target_arch = "wasm32")]
    {
        ic_cdk::api::canister_balance()
    }
    #[cfg(not(target_arch = "wasm32"))]
    {
        0
    }
}

pub fn get_stable_memory_size() -> u64 {
    #[cfg(target_arch = "wasm32")]
    {
        (ic_cdk::api::stable::stable_size() as u64) * WASM_PAGE_SIZE
    }
    #[cfg(not(target_arch = "wasm32"))]
    {
        0
    }
}

pub fn get_heap_memory_size() -> u64 {
    #[cfg(target_arch = "wasm32")]
    {
        (core::arch::wasm32::memory_size(0) as u64) * WASM_PAGE_SIZE
    }

    #[cfg(not(target_arch = "wasm32"))]
    {
        0
    }
}

pub fn validate_page_params(page: usize, size: usize) -> (usize, usize) {
    let page = if page > 0 { page } else { 1 };
    let size = if size > 0 && size <= 100 { size } else { 10 };
    (page, size)
}

pub fn new_subaccount(opt_vec: Option<[u8; 32]>) -> Option<Subaccount> {
    match opt_vec {
        Some(vec) => Some(Subaccount(vec)),
        None => None,
    }
}

pub fn account_id(principal: Principal, subaccount: Option<[u8; 32]>) -> AccountIdentifier {
    let subaccount = subaccount
        .map(Subaccount)
        .unwrap_or_else(|| Subaccount([0; 32]));
    AccountIdentifier::new(&principal, &subaccount)
}

pub fn generate_order_subaccount(caller: Principal, payid: u64) -> Vec<u8> {
    // Create SHA-224 hasher
    let mut hasher = Sha224::new();

    // Domain separator length (0x0A)
    hasher.update([0x0A]);

    // Domain separator ("payid")
    hasher.update("payid".as_bytes());

    // Counter (payid) as a random seed, encoded in big-endian byte order
    let payid_bytes = payid.to_be_bytes();
    hasher.update(payid_bytes);

    // Caller principal (as blob)
    hasher.update(caller.as_slice());

    // Finalize hash and get result
    let hash_sum = hasher.finalize();

    // Compute CRC32 checksum of the hash
    let crc32 = crc32fast::hash(&hash_sum);

    // Prepare buffer for final result (CRC32 + hash)
    let mut buffer = Vec::with_capacity(32);

    // Append CRC32 (4 bytes, big-endian) to buffer
    buffer.extend_from_slice(&crc32.to_be_bytes());

    // Append hash to buffer
    buffer.extend_from_slice(&hash_sum);

    buffer
}
//! This module provides a function to generate the Candid interface for the canister.

use crate::{cycle_management::CycleAcceptResult, data_store::PlatformState};
use candid::{export_service, Principal};
use canister_types::{
    bucket::Token,
    platform::{CanisterArgs, GameMetadata, GameUnit},
};
use ic_cdk::query;
use serde_bytes::ByteBuf;

/// Query function to generate and return the Candid interface as a String.
/// This is used for exporting the Candid interface for the canister.
#[query(name = "__get_candid_interface_tmp_hack")]
fn generate_candid_interface() -> String {
    export_service!();
    __export_service()
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Test to save the generated Candid interface to a .did file in the current directory.
    #[test]
    fn save_candid_interface() {
        use std::env;
        use std::fs::write;
        use std::path::PathBuf;

        let output_dir = PathBuf::from(env::current_dir().unwrap());
        write(output_dir.join("canister_platform.did"), generate_candid_interface()).expect("Write failed.");
    }
}

use crate::{cycle_management::CycleAcceptResult, data_store::PlatformState};
use candid::{export_service, Principal};
use canister_types::{
    bucket::Token,
    platform::{CanisterArgs, GameMetadata, GameUnit},
};
use ic_cdk::query;
use serde_bytes::ByteBuf;

#[query(name = "__get_candid_interface_tmp_hack")]
fn generate_candid_interface() -> String {
    export_service!();
    __export_service()
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn save_candid_interface() {
        use std::env;
        use std::fs::write;
        use std::path::PathBuf;

        let output_dir = PathBuf::from(env::current_dir().unwrap());
        write(output_dir.join("canister_platform.did"), generate_candid_interface()).expect("Write failed.");
    }
}

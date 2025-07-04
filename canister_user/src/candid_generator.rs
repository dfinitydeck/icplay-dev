use candid::{export_service, Principal};
use ic_cdk::query;

use crate::cycles_ops::CycleAcceptResult;
use crate::user_init::UserCanisterArgs;
use canister_types::{
    canister::{StatusRequest, StatusResponse},
    payment::{PaymentInfo, QueryCommonReq, QueryOrderResp},
    user::{Attribute, UpdateUserInfo, UserInfo, UserProfileInfo},
    ByteN,
};
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
        write(output_dir.join("canister_user.did"), generate_candid_interface()).expect("Write failed.");
    }
}

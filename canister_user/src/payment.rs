use candid::{Nat, Principal};
use canister_types::constant::CanisterType;
use ic_cdk::api::call::{call, RejectionCode};

use crate::data_store::state;
use crate::helper_utils::{account_id, new_subaccount};
use ic_ledger_types::{
    AccountIdentifier, Memo, Subaccount, Tokens, TransferArgs as ICPTransferArgs,
};
use icrc_ledger_types::icrc1::account::Account;
use icrc_ledger_types::icrc1::transfer::TransferArg as ICRC1TransferArg;
use std::convert::TryFrom;

// Helper function: convert Nat to u64
fn nat_to_u64(nat: Nat) -> u64 {
    match u64::try_from(nat.0) {
        Ok(value) => value,
        Err(_) => {
            panic!("Value is too large to fit into a u64");
        }
    }
}

pub async fn get_token_balance(token: &str, account: Account) -> u64 {
    let env = state::get_env();
    match token {
        "ICP" => {
            let ledger_canister_id =
                Principal::from_text(env.get_canister_pid(CanisterType::Ledger)).unwrap();
            let amount: Result_0<(Nat,), _> =
                call(ledger_canister_id, "icrc1_balance_of", (account,)).await;
            match amount {
                Ok((value,)) => nat_to_u64(value),
                Err(_) => 0,
            }
        }
        "CKBTC" => {
            let ckbtc_ledger_canister_id =
                Principal::from_text(env.get_canister_pid(CanisterType::CkBTCLedger)).unwrap();
            let amount: Result_0<(Nat,), _> =
                call(ckbtc_ledger_canister_id, "icrc1_balance_of", (account,)).await;
            match amount {
                Ok((value,)) => nat_to_u64(value),
                Err(_) => 0,
            }
        }
        _ => 0,
    }
}

// 处理ICRC1转账的函数，支持ICP和CKBTC
pub async fn execute_icrc1_transfer(
    token: &str,
    from: Option<[u8; 32]>,
    to: Account,
    amount: u64,
) -> Result_0<u64, String> {
    let fee = get_token_fee(token);
    let args = ICRC1TransferArg {
        to,
        fee: Some(Nat::from(fee)),
        memo: None,
        from_subaccount: from,
        created_at_time: None,
        amount: Nat::from(amount),
    };
    let env = state::get_env();
    match token {
        "ICP" => {
            let ledger_canister_id =
                Principal::from_text(env.get_canister_pid(CanisterType::Ledger)).unwrap();
            let result: Result_0<(Nat,), (RejectionCode, String)> =
                call(ledger_canister_id, "icrc1_transfer", (args,)).await;

            match result {
                Ok((height,)) => Ok(nat_to_u64(height)),
                Err((rejection_code, err_msg)) => Err(format!(
                    "Canister call failed: {:?}, {}",
                    rejection_code, err_msg
                )),
            }
        }
        "CKBTC" => {
            let ckbtc_ledger_canister_id =
                Principal::from_text(env.get_canister_pid(CanisterType::CkBTCLedger)).unwrap();
            let result: Result_0<(Nat,), (RejectionCode, String)> =
                call(ckbtc_ledger_canister_id, "icrc1_transfer", (args,)).await;

            match result {
                Ok((height,)) => Ok(nat_to_u64(height)),
                Err((rejection_code, err_msg)) => Err(format!(
                    "Canister call failed: {:?}, {}",
                    rejection_code, err_msg
                )),
            }
        }
        _ => Err("Unsupported token".to_string()),
    }
}

// 使用Ledger canister处理ICP转账的函数
pub async fn execute_icp_transfer(
    from_subaccount: Option<Subaccount>,
    to: AccountIdentifier,
    amount: u64,
) -> Result_0<u64, String> {
    let env = state::get_env();
    let ledger_canister_id =
        Principal::from_text(env.get_canister_pid(CanisterType::Ledger)).unwrap();
    let fee = get_token_fee("ICP");

    let args = ICPTransferArgs {
        to,
        fee: Tokens::from_e8s(fee),
        memo: Memo(0),
        amount: Tokens::from_e8s(amount),
        from_subaccount,
        created_at_time: None,
    };

    ic_ledger_types::transfer(ledger_canister_id, args)
        .await
        .map_err(|e| format!("failed to call ledger: {:?}", e))?
        .map_err(|e| format!("ledger transfer error {:?}", e))
}

pub fn get_token_fee(token: &str) -> u64 {
    match token {
        "ICP" => 10_000, // ICP手续费示例
        "CKBTC" => 10,   // CKBTC手续费示例
        _ => 0,
    }
}

// 主要的代币转账逻辑处理函数
pub async fn execute_token_transfer(
    token: &str,
    from: Option<[u8; 32]>,
    to: Vec<u8>,
    amount: u64,
) -> Result_0<u64, String> {
    match token {
        "ICP" => {
            let to_account = AccountIdentifier::from_slice(&to)
                .map_err(|e| format!("Invalid ICP account identifier: {:?}", e))?;
            let from_subaccount = new_subaccount(from);
            execute_icp_transfer(from_subaccount, to_account, amount).await
        }
        "CKBTC" => {
            let account = Account {
                owner: Principal::from_slice(&to),
                subaccount: None,
            };
            execute_icrc1_transfer(token, from, account, amount).await
        }
        _ => Err("Unsupported token".to_string()),
    }
}

pub fn default_account_id() -> AccountIdentifier {
    let canister_pid = ic_cdk::id();
    account_id(canister_pid, None)
}

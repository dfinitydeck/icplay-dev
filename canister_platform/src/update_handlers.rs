use crate::access_control::{anonymous_guard, controller_guard, owner_guard};
use candid::Principal;
use canister_types::{
    bucket::Token,
    cose::{
        cose_sign1,
        coset::{iana::Algorithm::ES256K, CborSerializable},
        sha256, Token as CoseToken, BUCKET_TOKEN_AAD, PLATFORM_TOKEN_AAD,
    },
    platform::GameUnit,
    SECONDS,
};
use ic_cdk::update;
use serde_bytes::ByteBuf;

use crate::{
    crypto_utils,
    data_store::{self, game},
};

#[update(guard = "anonymous_guard")]
fn add_unit_to_game(game_id: u64, unit: GameUnit) -> Result<(), String> {
    game::add_unit_to_game(game_id, unit)
        .map_err(|err| format!("Failed to add unit: {}", err))
}

#[update]
fn delete_unit_from_game(game_id: u64, position: u64) -> Result<(), String> {
    game::delete_unit_from_game(game_id, position)
        .map_err(|err| format!("Failed to delete unit: {}", err))
}

#[update]
fn delete_unit_from_game_by_share(
    game_id: u64,
    save_canister_id: Principal,
    unit_id: u64,
) -> Result<(), String> {
    game::delete_unit_from_game_by_share(game_id, save_canister_id, unit_id)
        .map_err(|err| format!("Failed to delete remote unit: {}", err))
}

#[update]
fn batch_add_units_to_game(game_id: u64, units: Vec<GameUnit>) -> Result<(), String> {
    for unit in units {
        game::add_unit_to_game(game_id, unit.clone())
            .map_err(|err| format!("Failed to add unit: {}", err))?;
    }
    Ok(())
}

#[ic_cdk::update(guard = "controller_guard")]
async fn sign_access_token(token: Token) -> Result<ByteBuf, String> {
    let current_time = ic_cdk::api::time() / SECONDS;
    let (ecdsa_key_name, token_expiration) =
        data_store::state::with(|r| (r.ecdsa_key_name.clone(), r.token_expiration));
    let mut claims = CoseToken::from(token).to_cwt(current_time as i64, token_expiration as i64);
    claims.issuer = Some(ic_cdk::id().to_text());
    let mut sign1: canister_types::cose::coset::CoseSign1 = cose_sign1(claims, ES256K, None)?;
    let tbs_data = sign1.tbs_data(BUCKET_TOKEN_AAD);
    let message_hash = sha256(&tbs_data);

    let sig = crypto_utils::sign_with(
        &ecdsa_key_name,
        vec![PLATFORM_TOKEN_AAD.to_vec()],
        message_hash,
    )
    .await?;
    sign1.signature = sig;
    let token = sign1.to_vec().map_err(|err| err.to_string())?;
    Ok(ByteBuf::from(token))
}

#[ic_cdk::update(guard = "owner_guard")]
async fn access_token(audience_canister: Principal) -> Result<ByteBuf, String> {
    let subject = ic_cdk::caller();

    let token = CoseToken {
        subject,
        audience: audience_canister,
        policies: String::from("Folder.*:1 Bucket.Read.*"),
    };

    let current_time = ic_cdk::api::time() / SECONDS;
    let (ecdsa_key_name, token_expiration) =
        data_store::state::with(|r| (r.ecdsa_key_name.clone(), r.token_expiration));

    let mut claims = token.to_cwt(current_time as i64, token_expiration as i64);
    claims.issuer = Some(ic_cdk::id().to_text());
    let mut sign1 = cose_sign1(claims, ES256K, None)?;
    let tbs_data = sign1.tbs_data(BUCKET_TOKEN_AAD);
    let message_hash = sha256(&tbs_data);

    let sig = crypto_utils::sign_with(
        &ecdsa_key_name,
        vec![PLATFORM_TOKEN_AAD.to_vec()],
        message_hash,
    )
    .await?;
    sign1.signature = sig;
    let token = sign1.to_vec().map_err(|err| err.to_string())?;
    Ok(ByteBuf::from(token))
}
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

/// Adds a unit to the specified game.
/// Arguments:
/// * `game_id` - The ID of the game.
/// * `unit` - The unit to add.
#[update(guard = "anonymous_guard")]
fn add_unit_to_game(game_id: u64, unit: GameUnit) -> Result<(), String> {
    game::add_unit_to_game(game_id, unit)
        .map_err(|err| format!("Failed to add unit: {}", err))
}

/// Deletes a unit from the specified game by its position.
/// Arguments:
/// * `game_id` - The ID of the game.
/// * `position` - The position of the unit to delete.
#[update]
fn delete_unit_from_game(game_id: u64, position: u64) -> Result<(), String> {
    game::delete_unit_from_game(game_id, position)
        .map_err(|err| format!("Failed to delete unit: {}", err))
}

/// Deletes a unit from a shared game by unit ID and canister ID.
/// Arguments:
/// * `game_id` - The ID of the game.
/// * `save_canister_id` - The canister ID where the unit is saved.
/// * `unit_id` - The ID of the unit to delete.
#[update]
fn delete_unit_from_game_by_share(
    game_id: u64,
    save_canister_id: Principal,
    unit_id: u64,
) -> Result<(), String> {
    game::delete_unit_from_game_by_share(game_id, save_canister_id, unit_id)
        .map_err(|err| format!("Failed to delete remote unit: {}", err))
}

/// Adds multiple units to the specified game in batch.
/// Arguments:
/// * `game_id` - The ID of the game.
/// * `units` - The list of units to add.
#[update]
fn batch_add_units_to_game(game_id: u64, units: Vec<GameUnit>) -> Result<(), String> {
    units.iter().try_for_each(|unit| {
        game::add_unit_to_game(game_id, unit.clone())
            .map_err(|err| format!("Failed to add unit: {}", err))
    })
}

/// Signs a token for access control. Only controller can call this.
/// Arguments:
/// * `token` - The token to sign.
#[ic_cdk::update(guard = "controller_guard")]
async fn sign_access_token(token: Token) -> Result<ByteBuf, String> {
    sign_token(token, BUCKET_TOKEN_AAD).await
}

/// Issues an access token for a specific audience. Only owner can call this.
/// Arguments:
/// * `audience_canister` - The canister principal for which the token is issued.
#[ic_cdk::update(guard = "owner_guard")]
async fn access_token(audience_canister: Principal) -> Result<ByteBuf, String> {
    let subject = ic_cdk::caller();
    let token = CoseToken {
        subject,
        audience: audience_canister,
        policies: String::from("Folder.*:1 Bucket.Read.*"),
    };
    sign_token(token, BUCKET_TOKEN_AAD).await
}

/// Internal helper to sign a token and return the signed bytes.
async fn sign_token<T: Into<CoseToken>>(token: T, aad: &'static [u8]) -> Result<ByteBuf, String> {
    let current_time = ic_cdk::api::time() / SECONDS;
    let (ecdsa_key_name, token_expiration) =
        data_store::state::with(|r| (r.ecdsa_key_name.clone(), r.token_expiration));
    let mut claims = token.into().to_cwt(current_time as i64, token_expiration as i64);
    claims.issuer = Some(ic_cdk::id().to_text());
    let mut sign1 = cose_sign1(claims, ES256K, None)
        .map_err(|err| format!("COSE sign1 error: {}", err))?;
    let tbs_data = sign1.tbs_data(aad);
    let message_hash = sha256(&tbs_data);
    let sig = crypto_utils::sign_with(
        &ecdsa_key_name,
        vec![PLATFORM_TOKEN_AAD.to_vec()],
        message_hash,
    )
    .await
    .map_err(|err| format!("Signature error: {}", err))?;
    sign1.signature = sig;
    let token = sign1.to_vec().map_err(|err| err.to_string())?;
    Ok(ByteBuf::from(token))
}
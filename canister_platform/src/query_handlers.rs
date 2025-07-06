// Query handlers for platform and game information
// All comments are in English for clarity

use canister_types::platform::GameMetadata;
use crate::data_store::{self, PlatformState};

/// Returns the current platform state information.
#[ic_cdk::query]
fn fetch_platform_info() -> Result<PlatformState, String> {
    Ok(data_store::state::with(|state| PlatformState {
        name: state.name.clone(),
        owner: state.owner,
        ecdsa_key_name: state.ecdsa_key_name.clone(),
        ecdsa_token_public_key: state.ecdsa_token_public_key.clone(),
        token_expiration: state.token_expiration,
        next_channel_id: state.next_channel_id,
        space_count: state.space_count,
    }))
}

/// Returns metadata for a specific game by its ID.
#[ic_cdk::query]
fn fetch_game_info(game_id: u64) -> Result<GameMetadata, String> {
    match data_store::game::get_game(game_id) {
        Some(game) => Ok(game.into_inner()),
        None => Err("Game not found".to_string()),
    }
}

/// Returns a list of all game metadata.
#[ic_cdk::query]
fn fetch_game_list() -> Vec<GameMetadata> {
    data_store::game::get_game_list()
}
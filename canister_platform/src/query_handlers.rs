// Query handlers for platform and game information
// All comments are in English for clarity

use canister_types::platform::GameMetadata;
use crate::data_store::{self, PlatformState};

/// Query: Get current platform state information.
#[ic_cdk::query]
fn fetch_platform_info() -> Result<PlatformState, String> {
    data_store::state::with(|state| {
        Ok(PlatformState {
            name: state.name.clone(),
            owner: state.owner,
            ecdsa_key_name: state.ecdsa_key_name.clone(),
            ecdsa_token_public_key: state.ecdsa_token_public_key.clone(),
            token_expiration: state.token_expiration,
            next_channel_id: state.next_channel_id,
            space_count: state.space_count,
        })
    })
}

/// Query: Get metadata for a specific game by its ID.
#[ic_cdk::query]
fn fetch_game_info(game_id: u64) -> Result<GameMetadata, String> {
    data_store::game::get_game(game_id)
        .map(|game| game.into_inner())
        .ok_or_else(|| "Game not found".to_string())
}

/// Query: Get a list of all game metadata.
#[ic_cdk::query]
fn fetch_game_list() -> Vec<GameMetadata> {
    data_store::game::get_game_list()
}
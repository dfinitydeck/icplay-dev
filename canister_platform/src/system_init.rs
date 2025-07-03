use crate::data_store;
use canister_types::platform::{CanisterArgs, GameCategory, GameMetadata, GameType};
use core::time::Duration;

#[ic_cdk::init]
fn platform_initialize(params: Option<CanisterArgs>) {
    match params {
        Some(CanisterArgs::Init(init_data)) => {
            data_store::state::with_mut(|plat| {
                if !init_data.name.is_empty() {
                    plat.name = init_data.name;
                }
                plat.owner = init_data.owner;
                plat.ecdsa_key_name = init_data.ecdsa_key_name;
                plat.token_expiration = init_data.token_expiration;
            });

            if init_data.init_channel == true {
                let mut game_id: u64 = 1;

                // Initialize predefined game entries
                let game_list = vec![
                    //(),
                ];

                for (name, category, game_type, image) in game_list {
                    let new_game = GameMetadata::new(
                        game_id,
                        name.to_string(),
                        init_data.owner,
                        game_type,
                        Some(category),
                        image,
                    );
                    data_store::game::add_game(game_id, new_game);
                    game_id += 1;
                }

                // Initialize games
                let radio_game_list = vec![
                    //(),
                ];

                for (name, category, game_type, image) in reward_game_list {
                    let new_game = GameMetadata::new(
                        game_id,
                        name.to_string(),
                        init_data.owner,
                        game_type,
                        Some(category),
                        image,
                    );
                    data_store::game::add_game(game_id, new_game);
                    game_id += 1;
                }

                data_store::state::with_mut(|plat| {
                    plat.next_channel_id = game_id;
                });
            }
        }
        Some(CanisterArgs::Upgrade(_)) => {
            ic_cdk::trap(
                "Cannot initialize the canister with an Upgrade args. Please provide an Init args.",
            );
        }
        None => {
            ic_cdk::trap("No initialization arguments provided");
        }
    }

    ic_cdk_timers::set_timer(Duration::from_secs(0), || {
        ic_cdk::spawn(async {
            if let Err(err) = data_store::state::init_ecdsa_public_key().await {
                ic_cdk::println!("Error initializing ECDSA public key: {}", err);
            }
        })
    });
}

#[ic_cdk::pre_upgrade]
fn before_platform_upgrade() {
    data_store::state::save();
}

#[ic_cdk::post_upgrade]
fn after_platform_upgrade(upgrade_params: Option<CanisterArgs>) {
    data_store::state::load();
    match upgrade_params {
        Some(CanisterArgs::Upgrade(upgrade_data)) => {
            data_store::state::with_mut(|plat| {
                if let Some(new_owner) = upgrade_data.owner {
                    plat.owner = new_owner;
                }
                if let Some(new_token_expiration) = upgrade_data.token_expiration {
                    plat.token_expiration = new_token_expiration;
                }
            });
            data_store::state::save();
        }
        Some(CanisterArgs::Init(_)) => {
            ic_cdk::trap(
                "Cannot upgrade the canister with an Init args. Please provide an Upgrade args.",
            );
        }
        _ => {}
    }
}
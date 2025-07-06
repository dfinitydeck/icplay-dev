use crate::data_store;
use canister_types::platform::{CanisterArgs, GameCategory, GameMetadata, GameType};
use core::time::Duration;

fn add_games(game_list: Vec<(&str, GameCategory, GameType, Option<String>)>, owner: String, mut game_id: u64) -> u64 {
    for (name, category, game_type, image) in game_list {
        let new_game = GameMetadata::new(
            game_id,
            name.to_string(),
            owner.clone(),
            game_type,
            Some(category),
            image,
        );
        data_store::game::add_game(game_id, new_game);
        game_id += 1;
    }
    game_id
}

#[ic_cdk::init]
fn platform_initialize(params: Option<CanisterArgs>) {
    match params {
        Some(CanisterArgs::Init(init_data)) => {
            data_store::state::with_mut(|plat| {
                if !init_data.name.is_empty() {
                    plat.name = init_data.name;
                }
                plat.owner = init_data.owner.clone();
                plat.ecdsa_key_name = init_data.ecdsa_key_name;
                plat.token_expiration = init_data.token_expiration;
            });

            if init_data.init_channel {
                let mut game_id: u64 = 1;

                // Predefined game list (add as needed)
                let game_list: Vec<(&str, GameCategory, GameType, Option<String>)> = vec![
                    // ("GameName", GameCategory::Category, GameType::Type, Some("image_url".to_string())),
                ];

                game_id = add_games(game_list, init_data.owner.clone(), game_id);

                // Reward game list (add as needed)
                let reward_game_list: Vec<(&str, GameCategory, GameType, Option<String>)> = vec![
                    // ("RewardGame", GameCategory::Category, GameType::Type, Some("image_url".to_string())),
                ];

                game_id = add_games(reward_game_list, init_data.owner, game_id);

                data_store::state::with_mut(|plat| {
                    plat.next_channel_id = game_id;
                });
            }
        }
        Some(CanisterArgs::Upgrade(_)) => {
            ic_cdk::trap(
                "Init argument is Upgrade, initialization is not allowed. Please provide Init type argument.",
            );
        }
        None => {
            ic_cdk::trap("No initialization argument provided");
        }
    }

    ic_cdk_timers::set_timer(Duration::from_secs(0), || {
        ic_cdk::spawn(async {
            if let Err(err) = data_store::state::init_ecdsa_public_key().await {
                ic_cdk::println!("Failed to initialize ECDSA public key: {}", err);
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
    if let Some(CanisterArgs::Upgrade(upgrade_data)) = upgrade_params {
        data_store::state::with_mut(|plat| {
            if let Some(new_owner) = upgrade_data.owner {
                plat.owner = new_owner;
            }
            if let Some(new_token_expiration) = upgrade_data.token_expiration {
                plat.token_expiration = new_token_expiration;
            }
        });
        data_store::state::save();
    } else if let Some(CanisterArgs::Init(_)) = upgrade_params {
        ic_cdk::trap(
            "Upgrade argument is Init, upgrade is not allowed. Please provide Upgrade type argument.",
        );
    }
}
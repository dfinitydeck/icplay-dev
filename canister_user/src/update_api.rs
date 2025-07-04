use crate::{
    access_guard::{anonymous_guard, owner_guard},
    MAX_PROFILE_SIZE,
};
use candid::Principal;
use canister_types::{
    error::{CustomError, ErrorCode},
    payment::{PaymentInfo, PaymentType, TokenPrice},
    space::{CanisterArgs, GameBoxArgs, GameBoxInitArgs, ProfileInitArgs, ProfileBoxArgs},
    user::{Attribute, UpdateUserInfo, User, UserInfo, UserProfileInfo},
    ByteN,
};
use ic_cdk::{api::call::CallResult, caller, update};
use serde_bytes::ByteBuf;

use crate::data_store;

async fn core_create_user_space() -> Result<Principal, String> {
    let user_pid = ic_cdk::caller();
    if data_store::user::get_user(user_pid).is_none() {
        return Err(CustomError::new(ErrorCode::NoDataFound, Some("User not registered")).to_string());
    }
    let user_space_count = data_store::user::get_user_profiles_count(user_pid);
    if user_space_count >= MAX_PROFILE_SIZE {
        return Err(CustomError::new(ErrorCode::MaximumRecords, Some("User Profiles Count")).to_string());
    }
    let dao_id = data_store::state::with(|state| state.dao_canister_id);
    let env = data_store::state::with(|state| state.env.clone());
    if dao_id == Principal::anonymous() {
        return Err(CustomError::new(ErrorCode::StateNotSetting, Some("dao_canister_id")).to_string());
    }
    let init_args = ProfileBoxArgs {
        space_arg: Some(CanisterArgs::Init(ProfileInitArgs {
            owner: user_pid,
            dao_canister: dao_id,
            env,
            ..ProfileInitArgs::default()
        })),
        oss_arg: GameBoxArgs::Init(GameBoxInitArgs {
            default_admin_user: Some(user_pid),
            visibility: 1,
            ..GameBoxInitArgs::default()
        }),
    };
    let result: CallResult<(Result<(Principal, Principal), String>,)> = ic_cdk::api::call::call(
        dao_id,
        "create_profile_and_game_box",
        (init_args,),
    ).await;
    match result {
        Ok((response,)) => match response {
            Ok((new_profile_id, new_game_box_id)) => {
                data_store::user::add_profile_to_user(
                    user_pid,
                    UserProfileInfo {
                        profile_id: new_rpofile_id,
                        game_box_id: vec![new_game_box_id],
                    },
                ).map_err(|_| CustomError::new(ErrorCode::DataUpdateError, Some("add_profile_to_user")).to_string())?;
                Ok(new_profile_id)
            }
            Err(err_msg) => Err(CustomError::new(ErrorCode::RemoteCallCreateError, Some(&err_msg)).to_string()),
        },
        Err(err) => {
            ic_cdk::println!("{:?}", err);
            Err(CustomError::new(ErrorCode::RemoteCallCreateError, Some("create_profile_and_game_box")).to_string())
        }
    }
}

#[update(guard = "anonymous_guard")]
fn user_login() -> Result<UserInfo, String> {
    let user_pid = caller();
    match data_store::user::get_user(user_pid) {
        Some(user) => Ok(user.into_inner().to_user_info(user_pid)),
        None => {
            let new_user = User::new();
            data_store::user::add_user(user_pid, new_user.clone());
            Ok(new_user.to_user_info(user_pid))
        }
    }
}

#[update(guard = "owner_guard")]
fn admin_login(user_pid: Principal) -> Result<UserInfo, String> {
    match data_store::user::get_user(user_pid) {
        Some(user) => Ok(user.into_inner().to_user_info(user_pid)),
        None => {
            let new_user = User::new();
            data_store::user::add_user(user_pid, new_user.clone());
            Ok(new_user.to_user_info(user_pid))
        }
    }
}

#[ic_cdk::update(guard = "anonymous_guard")]
fn update_avatar(new_avatar: String) -> Result<bool, String> {
    let user_pid = ic_cdk::caller();
    let user_result = data_store::user::get_user(user_pid);
    match user_result {
        Some(_) => {
            data_store::user::update_user(user_pid, |user| {
                user.avatar = new_avatar.clone();
                user.updated_at = ic_cdk::api::time();
            }).map_err(|_| CustomError::new(ErrorCode::DataUpdateError, Some("User Avatar")).to_string())?;
            Ok(true)
        }
        None => Ok(false),
    }
}

#[ic_cdk::update(guard = "anonymous_guard")]
fn update_email(email: String) -> Result<bool, String> {
    let user_pid = ic_cdk::caller();
    let user_result = data_store::user::get_user(user_pid);
    match user_result {
        Some(_) => {
            data_store::user::update_user(user_pid, |user| {
                user.email = email.clone();
                user.updated_at = ic_cdk::api::time();
            }).map_err(|_| CustomError::new(ErrorCode::DataUpdateError, Some("User Email")).to_string())?;
            Ok(true)
        }
        None => Ok(false),
    }
}

#[ic_cdk::update(guard = "anonymous_guard")]
fn update_public_key(
    trusted_ecdsa_pub_key: Option<ByteBuf>,
    trusted_eddsa_pub_key: Option<ByteN<32>>,
) -> Result<bool, String> {
    let user_pid = ic_cdk::caller();
    let user_result = data_store::user::get_user(user_pid);
    match user_result {
        Some(_) => {
            data_store::user::update_user(user_pid, |user| {
                user.trusted_ecdsa_pub_key = trusted_ecdsa_pub_key.clone();
                user.trusted_eddsa_pub_key = trusted_eddsa_pub_key.clone();
                user.updated_at = ic_cdk::api::time();
            }).map_err(|_| CustomError::new(ErrorCode::DataUpdateError, Some("User Public Key")).to_string())?;
            Ok(true)
        }
        None => Ok(false),
    }
}

#[ic_cdk::update(guard = "anonymous_guard")]
fn add_user_attr(new_attr: Attribute) -> Result<bool, String> {
    let user_pid = ic_cdk::caller();
    let user_wrapper = data_store::user::get_user(user_pid);
    match user_wrapper {
        Some(_) => {
            data_store::user::update_user(user_pid, |user| {
                user.attributes.retain(|attr| attr.key != new_attr.key);
                user.attributes.push(new_attr);
                user.updated_at = ic_cdk::api::time();
            }).map_err(|_| CustomError::new(ErrorCode::DataUpdateError, Some("User Attribute")).to_string())?;
            Ok(true)
        }
        None => Err("User not found".to_string()),
    }
}

#[ic_cdk::update(guard = "anonymous_guard")]
fn update_user_info(update_info: UpdateUserInfo) -> Result<bool, String> {
    let user_pid = ic_cdk::caller();
    let user_result = data_store::user::get_user(user_pid);
    match user_result {
        Some(_) => {
            data_store::user::update_user(user_pid, |user| {
                if let Some(avatar) = &update_info.avatar {
                    user.avatar = avatar.clone();
                }
                if let Some(artist_name) = &update_info.artist_name {
                    user.artist_name = artist_name.clone();
                }
                if let Some(location) = &update_info.location {
                    user.location = location.clone();
                }
                if let Some(genre) = &update_info.genre {
                    user.genre = genre.clone();
                }
                if let Some(website) = &update_info.website {
                    user.website = website.clone();
                }
                if let Some(bio) = &update_info.bio {
                    user.bio = bio.clone();
                }
                if let Some(handler) = &update_info.handler {
                    user.handler = handler.clone();
                }
                if let Some(music_content_type) = &update_info.music_content_type {
                    user.music_content_type = Some(music_content_type.clone());
                }
                if let Some(born) = update_info.born {
                    user.born = Some(born);
                }
                if let Some(confirm_agreement) = update_info.confirm_agreement {
                    user.confirm_agreement = confirm_agreement;
                }
                user.updated_at = ic_cdk::api::time();
            }).map_err(|_| CustomError::new(ErrorCode::DataUpdateError, Some("User Info")).to_string())?;
            Ok(true)
        }
        None => Ok(false),
    }
}

#[update(guard = "anonymous_guard")]
async fn create_user_space_by_invite(invite_code: String) -> Result<Principal, String> {
    if !data_store::state::check_invite_code(&invite_code) {
        return Err(CustomError::new(ErrorCode::NoDataFound, Some("Invalid or missing invite code")).to_string());
    }
    data_store::state::delete_invite_code(invite_code)?;
    core_create_user_space().await
}

#[update(guard = "anonymous_guard")]
async fn create_user_space_by_pay(order_id: u64) -> Result<Principal, String> {
    let user_pid = ic_cdk::caller();
    if !data_store::payment::check_payment_order(user_pid, order_id) {
        return Err(CustomError::new(ErrorCode::DataInvalid, Some("Invalid Payment Order Info")).to_string());
    }
    core_create_user_space().await
}

#[update(guard = "owner_guard")]
async fn update_dao_id(dao_id: Principal) -> Result<Principal, String> {
    data_store::state::with_mut(|r| {
        r.dao_canister_id = dao_id;
    });
    Ok(dao_id)
}

#[ic_cdk::update(guard = "owner_guard")]
fn add_user_space(user_pid: Principal, space_info: UserProfileInfo) -> Result<bool, String> {
    let user_wrapper = data_store::user::get_user(user_pid);
    match user_wrapper {
        Some(_) => {
            data_store::user::update_user(user_pid, |user| {
                user.spaces.push(space_info);
            }).map_err(|_| CustomError::new(ErrorCode::DataUpdateError, Some("User space info")).to_string())?;
            Ok(true)
        }
        None => Err(CustomError::new(ErrorCode::NoDataFound, Some("User")).to_string()),
    }
}

#[ic_cdk::update(guard = "anonymous_guard")]
fn create_pay_order(source: String) -> Result<Option<PaymentInfo>, String> {
    let payer = caller();
    let mut payment_info: Option<PaymentInfo> = None;
    data_store::state::load();
    data_store::state::with_mut(|space| {
        let new_order_id = space.next_order_id;
        let token_price = TokenPrice::new_for_creation_space();
        let payment_type = PaymentType::CreationPrice(token_price.clone());
        payment_info = Some(data_store::payment::create_payment_order(
            new_order_id,
            payer,
            source,
            token_price.token_name,
            token_price.price,
            payment_type,
        ));
        space.total_orders += 1;
        space.next_order_id += 1;
    });
    data_store::state::save();
    Ok(payment_info)
}

#[ic_cdk::update(guard = "anonymous_guard")]
async fn confirm_pay_order(pay_id: u64) -> Result<bool, String> {
    let result = data_store::payment::confirm_payment_order(pay_id).await;
    result
}

#[ic_cdk::update(guard = "anonymous_guard")]
async fn refund_pay_order(pay_id: u64, to: Vec<u8>) -> Result<bool, String> {
    let from = caller();
    let result = data_store::payment::refund_payment_order(pay_id, from, to).await;
    result
}

#[ic_cdk::update(guard = "owner_guard")]
async fn add_invite(invite_code: String) -> Result<String, String> {
    data_store::state::add_invite_code(invite_code)
}

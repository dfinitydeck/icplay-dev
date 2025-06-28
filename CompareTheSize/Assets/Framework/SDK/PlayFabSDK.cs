using UnityEngine;
using System;
using System.Collections.Generic;
#if G_USE_PLAYFAB
using PlayFab;
using PlayFab.ClientModels;
#endif

namespace Framework
{
    /// <summary>
    /// PlayFab SDK
    /// </summary>
    public class PlayFabSDK : Singleton<PlayFabSDK>
    {

#if G_USE_PLAYFAB
        public bool isInit = false;

        public string loginUserId = string.Empty;
        public string loginTime = string.Empty;

        private string entityId = string.Empty;
        private string entityType = string.Empty;

        private Action<bool> onInitFinish;

        public PlayFabSDK()
        {

        }

        public void Init(string titleID = "", Action<bool> initCB = null)
        {
            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                // If TitleId is not set, set a default value
                if (titleID != "")
                {
                    PlayFabSettings.staticSettings.TitleId = titleID;
                }
                else
                {
                    Debug.LogError("PlayFabSettings TitleId is null");
                    return;
                }
            }

            onInitFinish = initCB;

#if UNITY_IOS
            // Temporarily use device verification for registration login
            // PlayFabClientAPI.LoginWithGameCenter(new LoginWithGameCenterRequest(), OnLoginSuccess, OnLoginFailure);
            var request = new LoginWithIOSDeviceIDRequest { DeviceId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true };
            PlayFabClientAPI.LoginWithIOSDeviceID(request, OnLoginSuccess, OnLoginFailure);
#elif UNITY_ANDROID
            // Temporarily use device verification for registration login
            // PlayGamesPlatform.Instance.Authenticate((SignInStatus status)=> {
            //     if (status == SignInStatus.Success)
            //     {
            //         PlayGamesPlatform.Instance.RequestServerSideAccess(false, (string serverAuthCode)=> {
            //             var request = new LoginWithGooglePlayGamesServicesRequest
            //             {
            //                 TitleId = PlayFabSettings.TitleId
            //                 ServerAuthCode = serverAuthCode,
            //                 CreateAccount = true,
            //             };

            //             PlayFabClientAPI.LoginWithGooglePlayGamesServices(request, OnLoginSuccess, OnLoginFailure);
            //         });
            //     }
            // });


            var request = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = SystemInfo.deviceUniqueIdentifier, CreateAccount = true };
            PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccess, OnLoginFailure);
#else
            var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true };
            PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
#endif
        }

        private void OnLoginSuccess(LoginResult result)
        {
            loginUserId = result.PlayFabId;
            loginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            entityId = result.EntityToken.Entity.Id;
            entityType = result.EntityToken.Entity.Type;

            isInit = true;
            Debug.Log($"PlayFab is Login！ {result.PlayFabId}");

            onInitFinish?.Invoke(true);
        }

        private void OnLoginFailure(PlayFabError error)
        {
            isInit = false;
            Debug.LogError(error.GenerateErrorReport());

            onInitFinish?.Invoke(false);
        }

        /// <summary>
        /// Game data is a set of plain text key-value pairs (Key/Value Pairs, KVP for short), used to store and manage game configuration information on the server backend.
        /// Game data is further subdivided into basic game data (Title Data) and internal game data (Title Internal Data). The former can be directly accessed by clients,
        /// while the latter can only be accessed by the PlayFab backend or authorized game services deployed by developers themselves.
        /// </summary>
        #region Using Game Data (TitleData)
        public void GetTitleData(Action<Dictionary<string, string>> cb)
        {
            var request = new PlayFab.ClientModels.GetTitleDataRequest();
            PlayFab.PlayFabClientAPI.GetTitleData(request, result =>
                {
                    cb?.Invoke(result.Data);
                    // foreach (var dataPair in result.Data)
                    // {
                    //     Debug.Log($"GetTitleData == {dataPair.Key} == {dataPair.Value}");
                    // }
                },
                error =>
                {
                    Debug.LogError($"GetTitleData == {error.GenerateErrorReport()}");
                }
            );
        }
        #endregion

        /// <summary>
        /// Player data (UserData): is data applied to individual players or player groups (shared), stored by PlayFab as key-value pair (KVP) information.
        /// </summary>
        /// <param name="cb"></param>
        #region Using User Data (UserData)
        public void GetUserData(Action<Dictionary<string, PlayFab.ClientModels.UserDataRecord>> cb)
        {
            var request = new PlayFab.ClientModels.GetUserDataRequest();
            PlayFab.PlayFabClientAPI.GetUserData(request, result =>
                {
                    cb?.Invoke(result.Data);
                    // foreach (var dataPair in result.Data)
                    // {
                    //     Debug.Log($"GetUserDataRequest == {dataPair.Key} == {dataPair.Value.Value}");
                    // }
                },
                error =>
                {
                    Debug.LogError($"GetUserDataRequest == {error.GenerateErrorReport()}");
                }
            );
        }

        public void SetUserData(string key, string value, Action<bool, string> cb = null)
        {
            var updateUserDataRequest = new PlayFab.ClientModels.UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {key, value},
                }
            };

            PlayFab.PlayFabClientAPI.UpdateUserData(updateUserDataRequest, result =>
                {
                    cb?.Invoke(true, result.DataVersion + "");
                    // Debug.Log($"UpdateUserDataRequest == Success {result.DataVersion}");
                },
                error =>
                {
                    cb?.Invoke(false, "");
                    Debug.LogError($"UpdateUserDataRequest == {error.GenerateErrorReport()}, key == {key}, value == {value}");
                }
            );
        }

        public void SetUserData(Dictionary<string, string> dictionary, Action<bool, string> cb = null)
        {
            var updateUserDataRequest = new PlayFab.ClientModels.UpdateUserDataRequest
            {
                Data = dictionary
            };

            PlayFab.PlayFabClientAPI.UpdateUserData(updateUserDataRequest, result =>
                {
                    cb?.Invoke(true, result.DataVersion + "");
                    // Debug.Log($"UpdateUserDataRequest == Success {result.DataVersion}");
                },
                error =>
                {
                    cb?.Invoke(false, "");
                    Debug.LogError($"UpdateUserDataRequest == {error.GenerateErrorReport()}, count == {dictionary.Count}");
                }
            );
        }
        #endregion

        /// <summary>
        /// Entities are the most basic addressable "content" when PlayFab API operates. Each entity has a "type" and ID, which together form the unique identifier of that entity.
        /// Some types of entities are built into PlayFab, such as namespace, game (title), group,
        /// all games shared player entity (master_player_account) and this game player entity (title_player_account), etc.
        /// Each entity has a profile that contains various resources owned by that entity. For example, objects, files, language settings, policies, etc.
        /// There are parent-child relationships between entities, and these relationships determine the permissions for resource access between entities.
        /// </summary>
        /// <param name="entityKey"></param>
        /// <param name="entityType"></param>
        #region Using Entity Data (EntityData)
        public void SetEntityObject(string entityKey, string entityType)
        {
            // // Sample data
            // var debugData = new Dictionary<string, object>()
            // {
            //     {"Atk", 100},
            //     {"Hp", 1000},
            // };
            //     var dataList = new List<SetObject>()
            // {
            //     new SetObject()
            //     {
            //         ObjectName = "MyPlayerData",
            //         DataObject = debugData,
            //     },
            // };

            // var newSetObjectRequest = new SetObjectsRequest()
            // {
            //     Entity = new PlayFab.DataModels.EntityKey() { Id = entityKey, Type = entityType },
            //     Objects = dataList,
            // };

            // PlayFabDataAPI.SetObjects(newSetObjectRequest, setResult =>
            //     {
            //         Debug.Log($"SetEntityObject == Success with version {setResult.ProfileVersion}");
            //     },
            //     error =>
            //     {
            //         Debug.LogError(error.ErrorMessage);
            //     });
        }

        public void GetEntityObject(string entityKey, string entityType, Action cb)
        {
            // var newGetObjectRequest = new GetObjectsRequest()
            // {
            //     Entity = new PlayFab.DataModels.EntityKey() { Id = entityKey, Type = entityType },
            // };

            // PlayFabDataAPI.GetObjects(newGetObjectRequest, getResult =>
            // {
            //     cb?.Invoke(getResult.Objects);
            //     // Debug.Log($"GetEntityObject == Success with version {getResult.ProfileVersion}");
            //     // foreach (var dataPair in getResult.Objects)
            //     // {
            //     //     Debug.Log($"GetEntityObject == {dataPair.Key} == {dataPair.Value.DataObject}");
            //     // }
            // },
            // error =>
            // {
            //     Debug.LogError(error.ErrorMessage);
            // });
        }
        #endregion

        #region Purchase Items
        public static void PurchaseHealthPotion(Action<string> onPurchaseFinish)
        {
            // var purchaseItemRequest = new PlayFab.ClientModels.PurchaseItemRequest
            // {
            //     CatalogVersion = "Items",
            //     ItemId = "HealthPotion",
            //     Price = 10,
            //     VirtualCurrency = "CN"
            // };

            // PlayFab.PlayFabClientAPI.PurchaseItem(purchaseItemRequest, result =>
            //     {
            //         Debug.Log($"PurchaseHealthPotion == {result.Request.GetType().Name} Success {result.Items[0].ItemId}");
            //         onPurchaseFinish?.Invoke(result.Items[0].ItemInstanceId);
            //     },
            //     error =>
            //     {
            //         Debug.LogError($"PurchaseHealthPotion == {error.GenerateErrorReport()}");
            //         onPurchaseFinish?.Invoke(string.Empty);
            //     }
            // );
        }

        public static void GetUserInventory()
        {
            // var request = new PlayFab.ClientModels.GetUserInventoryRequest();

            // PlayFab.PlayFabClientAPI.GetUserInventory(request, result =>
            //     {
            //         foreach (var item in result.Inventory)
            //         {
            //             Debug.Log($"GetUserInventory == {item.ItemId} == {item.DisplayName} : {item.ItemInstanceId} count: {(item.RemainingUses.HasValue ? item.RemainingUses.Value : 0)}");
            //         }
            //     },
            //     error =>
            //     {
            //         Debug.LogError($"GetUserInventory == {error.GenerateErrorReport()}");
            //     }
            // );
        }

        public static void ConsumePotion(string itemInstanceId)
        {
            // var consumeItemRequest = new PlayFab.ClientModels.ConsumeItemRequest
            // {
            //     ItemInstanceId = itemInstanceId,
            //     ConsumeCount = 1
            // };

            // PlayFab.PlayFabClientAPI.ConsumeItem(consumeItemRequest, result =>
            //     {
            //         Debug.Log($"ConsumePotion == {result.Request.GetType().Name}-{itemInstanceId} Success");
            //     },
            //     error =>
            //     {
            //         Debug.LogError($"ConsumePotion == {error.GenerateErrorReport()}");
            //     }
            // );
        }

        #endregion

        #region Using Leaderboards
        public static void SubmitHighScore(int highScore)
        {
            // var request = new PlayFab.ClientModels.UpdatePlayerStatisticsRequest
            // {
            //     Statistics = new List<PlayFab.ClientModels.StatisticUpdate>
            // {
            //     new PlayFab.ClientModels.StatisticUpdate
            //     {
            //         StatisticName = "Daily High Score",
            //         Value = highScore
            //     }
            // }
            // };

            // PlayFab.PlayFabClientAPI.UpdatePlayerStatistics(request, result =>
            //     {
            //         Debug.Log($"SubmitHighScore == {result.Request.GetType().Name} Success");
            //     },
            //     error =>
            //     {
            //         Debug.LogError($"SubmitHighScore == {error.GenerateErrorReport()}");
            //     }
            // );
        }

        public static void GetLeaderboard(int highScoreCount)
        {
            // var request = new PlayFab.ClientModels.GetLeaderboardRequest
            // {
            //     StatisticName = "Daily High Score",
            //     StartPosition = 0,
            //     MaxResultsCount = 10
            // };

            // PlayFab.PlayFabClientAPI.GetLeaderboard(request, result =>
            //     {
            //         Debug.Log($"GetLeaderboard == {result.Request.GetType().Name} Success");

            //         foreach (var player in result.Leaderboard)
            //         {
            //             Debug.Log($"GetLeaderboard == {player.Position} == {player.PlayFabId} == {player.StatValue}");
            //         }
            //     },
            //     error =>
            //     {
            //         Debug.LogError($"GetLeaderboard == {error.GenerateErrorReport()}");
            //     }
            // );
        }

        public static void GetLeaderboardAroundPlayer(int highScoreCount)
        {
            // var request = new PlayFab.ClientModels.GetLeaderboardAroundPlayerRequest
            // {
            //     StatisticName = "Daily High Score",
            //     MaxResultsCount = highScoreCount
            // };

            // PlayFab.PlayFabClientAPI.GetLeaderboardAroundPlayer(request, result =>
            //     {
            //         Debug.Log($"GetLeaderboardAroundPlayer == {result.Request.GetType().Name} Success");

            //         foreach (var player in result.Leaderboard)
            //         {
            //             Debug.Log($"GetLeaderboardAroundPlayer == {player.Position} == {player.PlayFabId} == {player.StatValue}");
            //         }
            //     },
            //     error =>
            //     {
            //         Debug.LogError($"GetLeaderboardAroundPlayer == {error.GenerateErrorReport()}");
            //     }
            // );
        }
        #endregion

#endif
    }

}
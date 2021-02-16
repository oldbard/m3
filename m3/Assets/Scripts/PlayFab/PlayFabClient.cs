using GameData;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Client
{
    public class PlayFabClient
    {
        #region Declarations

        public readonly List<string> ContentTitleDataKeys = new List<string>
        { 
            "DestroyAnimationTime",
            "DropAnimationTime",
            "FullColorBlinkDelay",
            "GameDuration",
            "HintAnimationTime",
            "HintCycles",
            "PointsPerTile",
            "StartToBlinkDelay",
            "SwapAnimationTime",
            "TimeToShowHint",
            "TimeToShowWarning",
            "Upgradables"
        };

        string _androidId;
        string _iOSId;
        string _customId;

        Dictionary<string, int> _currenciesData;
        Dictionary<string, string> _gameData;
        List<CatalogConfigData> _catalog = new List<CatalogConfigData>();

        string _userName;

        bool _loggingIn;
        bool _gettingGameData;
        bool _gettingCatalog;
        bool _updatingPlayerName;

        bool _pendingUserName;

        #endregion

        #region API Calls

        public async Task<(Dictionary<string, int>, bool)> Login()
        {
            LoginWithDeviceId();

            while(_loggingIn)
            {
                await Task.Yield();
            }

            return (_currenciesData, _pendingUserName);
        }

        /// <summary>
        /// Logins the with device identifier (iOS & Android only).
        /// </summary>
        void LoginWithDeviceId()
        {
            _loggingIn = true;

            if (FillDeviceId())
            {
                if (!string.IsNullOrEmpty(_androidId))
                {
                    Debug.Log("Using Android Device ID: " + _androidId);
                    var request = new LoginWithAndroidDeviceIDRequest
                    {
                        AndroidDeviceId = _androidId,
                        TitleId = PlayFabSettings.TitleId,
                        CreateAccount = true
                    };

                    PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccessful, OnLoginFailed);
                }
                else if (!string.IsNullOrEmpty(_iOSId))
                {
                    Debug.Log("Using IOS Device ID: " + _iOSId);
                    var request = new LoginWithIOSDeviceIDRequest
                    {
                        DeviceId = _iOSId,
                        TitleId = PlayFabSettings.TitleId,
                        CreateAccount = true
                    };

                    PlayFabClientAPI.LoginWithIOSDeviceID(request, OnLoginSuccessful, OnLoginFailed);
                }
            }
            else
            {
                Debug.Log("Using custom device ID: " + _customId);
                var request = new LoginWithCustomIDRequest
                {
                    CustomId = _customId,
                    TitleId = PlayFabSettings.TitleId,
                    CreateAccount = true,
                    InfoRequestParameters = new GetPlayerCombinedInfoRequestParams()
                    {
                        GetPlayerProfile = true,
                        GetUserInventory = true,
                        GetUserVirtualCurrency = true
                    }
                };

                PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccessful, OnLoginFailed);
            }
        }

        public async Task<Dictionary<string, string>> GetGameData()
        {
            RequestGameData();

            while(_gettingGameData)
            {
                await Task.Yield();
            }

            return _gameData;
        }

        void RequestGameData()
        {
            _gettingGameData = true;

            var request = new GetTitleDataRequest { Keys = ContentTitleDataKeys };
            PlayFabClientAPI.GetTitleData(request, OnGetTitleDataSuccessful, OnGetTitleDataFailed);
        }

        public async Task<string> SetUserName(string userName)
        {
            UpdatePlayerName(userName);

            while(_updatingPlayerName)
            {
                await Task.Yield();
            }

            return _userName;
        }

        void UpdatePlayerName(string userName)
        {
            _updatingPlayerName = true;

            var request = new UpdateUserTitleDisplayNameRequest { DisplayName = userName };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnSetDisplayNameSuccessful, OnSetDisplayNameFailed);
        }

        public async Task<List<CatalogConfigData>> GetCatalogItems()
        {
            RequestCatalogItems();

            while (_gettingCatalog)
            {
                await Task.Yield();
            }

            return _catalog;
        }

        void RequestCatalogItems()
        {
            _gettingCatalog = true;

            var req = new GetCatalogItemsRequest { CatalogVersion = "1" };

            PlayFabClientAPI.GetCatalogItems(req, OnCatalogRequestSuccessful, OnCatalogRequestFailed);
        }

        #endregion

        #region Callbacks

        /// <summary>
        /// Called on a successful login attempt
        /// </summary>
        /// <param name="result">Result object returned from PlayFab server</param>
        void OnLoginSuccessful(LoginResult result)
        {
            Debug.Log("Login Successful");

            if(result.InfoResultPayload.PlayerProfile == null || string.IsNullOrEmpty(result.InfoResultPayload.PlayerProfile.DisplayName))
            {
                Debug.Log("Pending User Name");
                _pendingUserName = true;
            }
            else
            {
                Debug.Log("User Name is " + result.InfoResultPayload.PlayerProfile.DisplayName);
            }
            _currenciesData = result.InfoResultPayload.UserVirtualCurrency;

            _loggingIn = false;
        }

        /// <summary>
        /// Called on a failed login attempt
        /// </summary>
        /// <param name="result">Result object returned from PlayFab server</param>
        void OnLoginFailed(PlayFabError error)
        {
            throw new Exception($"Login Failed! {error.ErrorMessage}");
        }

        void OnSetDisplayNameSuccessful(UpdateUserTitleDisplayNameResult result)
        {
            _userName = result.DisplayName;
            
            Debug.Log("Successfully changed Display Name");

            _updatingPlayerName = false;
        }

        void OnSetDisplayNameFailed(PlayFabError error)
        {
            throw new Exception($"Failed to change Display Name. {error.ErrorMessage}");
        }

        void OnGetTitleDataSuccessful(GetTitleDataResult result)
        {
            Debug.Log("Managed to get the title data");
            _gameData = result.Data;

            _gettingGameData = false;
        }

        void OnGetTitleDataFailed(PlayFabError error)
        {
            throw new Exception($"Failed to get the title data. {error.ErrorMessage}");
        }

        void OnCatalogRequestSuccessful(GetCatalogItemsResult result)
        {
            const string SoftCurrency = "SC";

            foreach (var catalogItem in result.Catalog)
            {
                var item = new CatalogConfigData();
                item.Parse(catalogItem.ItemId, catalogItem.VirtualCurrencyPrices[SoftCurrency],
                    catalogItem.CustomData);

                _catalog.Add(item);
            }

            _gettingCatalog = false;
        }

        void OnCatalogRequestFailed(PlayFabError error)
        {
            throw new Exception($"Failed to get shop catalog. {error.ErrorMessage}");
        }

        #endregion

        #region Utils

        /// <summary>
        /// Gets the device identifier and updates the static variables
        /// </summary>
        /// <returns><c>true</c>, if device identifier was obtained, <c>false</c> otherwise.</returns>
        public bool FillDeviceId(bool silent = false) // silent suppresses the error
        {
            if (CheckForSupportedMobilePlatform())
            {
#if UNITY_ANDROID
                //http://answers.unity3d.com/questions/430630/how-can-i-get-android-id-.html
                AndroidJavaClass clsUnity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject objActivity = clsUnity.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject objResolver = objActivity.Call<AndroidJavaObject>("getContentResolver");
                AndroidJavaClass clsSecure = new AndroidJavaClass("android.provider.Settings$Secure");
                _androidId = clsSecure.CallStatic<string>("getString", objResolver, "android_id");
#endif

#if UNITY_IPHONE
			_iOSId = UnityEngine.iOS.Device.vendorIdentifier;
#endif
                return true;
            }
            else
            {
                _customId = SystemInfo.deviceUniqueIdentifier;
                return false;
            }
        }

        /// <summary>
        /// Check to see if our current platform is supported (iOS & Android)
        /// </summary>
        /// <returns><c>true</c>, for supported mobile platform, <c>false</c> otherwise.</returns>
        public bool CheckForSupportedMobilePlatform()
        {
            return Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
        }

        #endregion
    }
}
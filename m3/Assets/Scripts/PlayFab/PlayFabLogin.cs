using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

namespace Server
{
    public class PlayFabLogin : MonoBehaviour
    {
        #region Declarations

        [SerializeField] Image _loginPanel;
        [SerializeField] Button _loginButton;

        string _userName;

        string _androidId;
        string _iOSId;
        string _customId;

        #endregion

        void Start()
        {
            _loginButton.enabled = false;

            LoginWithDeviceId();
        }

        #region API Calls

        /// <summary>
        /// Logins the with device identifier (iOS & Android only).
        /// </summary>
        void LoginWithDeviceId()
        {
            if(FillDeviceId())
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
                        GetPlayerProfile = true
                    }
                };

                PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccessful, OnLoginFailed);
            }
        }

        void UpdatePlayerName()
        {
            if(IsUserNameValid())
            {
                var request = new UpdateUserTitleDisplayNameRequest { DisplayName = _userName };
                PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnSetDisplayNameSuccessful, OnSetDisplayNameFailed);

            }
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

            if(string.IsNullOrEmpty(result.InfoResultPayload.PlayerProfile.DisplayName))
            {
                Debug.Log("Pending User Name");
                _loginButton.enabled = true;
            }
            else
            {
                Debug.Log("User Name is " + result.InfoResultPayload.PlayerProfile.DisplayName);
                _loginPanel.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called on a failed login attempt
        /// </summary>
        /// <param name="result">Result object returned from PlayFab server</param>
        void OnLoginFailed(PlayFabError error)
        {
            Debug.Log("Login Failed!");
        }

        void OnSetDisplayNameSuccessful(UpdateUserTitleDisplayNameResult result)
        {
            Debug.Log("Successfully changed Display Name");
            _loginPanel.gameObject.SetActive(false);
        }

        void OnSetDisplayNameFailed(PlayFabError error)
        {
            Debug.Log("Failed to change Display Name");
        }

        #endregion

        #region Utils

        bool IsUserNameValid()
        {
            if (string.IsNullOrEmpty(_userName))
                return false;

            if (_userName.Length < 3 || _userName.Length > 20)
                return false;

            return true;
        }

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

        #region UI

        public void SetUserName(string userName)
        {
            _userName = userName;
        }

        public void OnLogin()
        {
            if(!IsUserNameValid())
            {
                Debug.LogWarning("Player Name is not set!");
            }
            else
            {
                UpdatePlayerName();
            }
        }

        #endregion
    }
}
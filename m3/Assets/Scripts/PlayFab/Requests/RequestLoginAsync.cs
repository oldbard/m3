using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Requests
{
    /// <summary>
    /// Request to attempt to login to PlayFab
    /// </summary>
    public class RequestLoginAsync : IRequestAsync
    {
        string _androidId;
        string _iOSId;
        string _customId;

        bool _success;
        
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        public async Task<IResultAsync> Process()
        {
            LoginWithDeviceId();

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new LoginResultAsync(_success);
        }

        /// <summary>
        /// Logins the with device identifier (iOS & Android only).
        /// </summary>
        void LoginWithDeviceId()
        {
            _isProcessing = true;

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
                };

                PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccessful, OnLoginFailed);
            }
        }

        /// <summary>
        /// Called on a successful login attempt
        /// </summary>
        /// <param name="result">Result object returned from PlayFab server</param>
        void OnLoginSuccessful(LoginResult result)
        {
            Debug.Log("Login Successful");

            _success = true;

            _isProcessing = false;
        }

        /// <summary>
        /// Called on a failed login attempt
        /// </summary>
        /// <param name="result">Result object returned from PlayFab server</param>
        void OnLoginFailed(PlayFabError error)
        {
            _isProcessing = false;

            throw new Exception($"Login Failed! {error.ErrorMessage}");
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
    }

    public class LoginResultAsync : IResultAsync
    {
        public bool Success;

        public LoginResultAsync(bool success)
        {
            Success = success;
        }
    }
}
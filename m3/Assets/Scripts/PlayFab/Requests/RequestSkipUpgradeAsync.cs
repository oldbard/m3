using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Requests
{
    /// <summary>
    /// Request which calls the CloudScript function SkipDurationUpgrade to skip the upgrade timer
    /// </summary>
    public class RequestSkipUpgradeAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        bool _durationUpgradeSkipSuccesful;

        public async Task<IResultAsync> Process()
        {
            CallFunctionStartTimerDurationUpgrade();

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new DurationUpgradeSkipResultAsync(_durationUpgradeSkipSuccesful);
        }

        void CallFunctionStartTimerDurationUpgrade()
        {
            _isProcessing = true;

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "SkipDurationUpgrade"
            };
            PlayFabClientAPI.ExecuteCloudScript(request, OnSkipTimerDurationUpgradeSuccessful, OnSkipTimerDurationUpgradeFailed);
        }

        void OnSkipTimerDurationUpgradeSuccessful(ExecuteCloudScriptResult result)
        {
            if(result.Error != null)
            {
                ThrowError(result.Error.Message);
            }

            if(bool.TryParse(result.FunctionResult.ToString(), out var success))
            {
                if (success)
                {
#if UNITY_EDITOR
                    Debug.Log("Upgrade Skipped!");
#endif
                    _durationUpgradeSkipSuccesful = true;
                }
            }

            _isProcessing = false;
        }

        void OnSkipTimerDurationUpgradeFailed(PlayFabError error)
        {
            ThrowError(error.ErrorMessage);
        }

        void ThrowError(string error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to start timer duration upgrade. {error}");
        }
    }

    public class DurationUpgradeSkipResultAsync : IResultAsync
    {
        public bool Success;

        public DurationUpgradeSkipResultAsync(bool success)
        {
            Success = success;
        }
    }
}
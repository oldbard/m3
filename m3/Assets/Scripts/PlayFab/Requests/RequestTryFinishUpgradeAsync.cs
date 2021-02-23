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
    /// Request which calls the CloudScript function TryFinishUpgradingDuration to try and finish an upgrade
    /// </summary>
    public class RequestTryFinishUpgradeAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        bool _durationUpgradeFinishedSuccesful;

        public async Task<IResultAsync> Process()
        {
            CallFunctionTryFinishTimerDurationUpgrade();

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new DurationUpgradeTryFinishResultAsync(_durationUpgradeFinishedSuccesful);
        }

        void CallFunctionTryFinishTimerDurationUpgrade()
        {
            _isProcessing = true;

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "TryFinishUpgradingDuration"
            };
            PlayFabClientAPI.ExecuteCloudScript(request, OnTryFinishDurationUpgradeSuccessful, OnTryFinishDurationUpgradeFailed);
        }

        void OnTryFinishDurationUpgradeSuccessful(ExecuteCloudScriptResult result)
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
                    Debug.Log("Upgrade Finished!");
#endif
                    _durationUpgradeFinishedSuccesful = true;
                }
            }

            _isProcessing = false;
        }

        void OnTryFinishDurationUpgradeFailed(PlayFabError error)
        {
            ThrowError(error.ErrorMessage);
        }

        void ThrowError(string error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to finish timer duration upgrade. {error}");
        }
    }

    public class DurationUpgradeTryFinishResultAsync : IResultAsync
    {
        public bool Success;

        public DurationUpgradeTryFinishResultAsync(bool success)
        {
            Success = success;
        }
    }
}
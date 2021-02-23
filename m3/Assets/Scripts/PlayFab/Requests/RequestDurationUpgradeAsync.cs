using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading.Tasks;

namespace Requests
{
    public class RequestDurationUpgradeAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        bool _durationUpgradeSuccesful;

        public async Task<IResultAsync> Process()
        {
            CallFunctionStartTimerDurationUpgrade();

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new DurationUpgradeResultAsync(_durationUpgradeSuccesful);
        }

        void CallFunctionStartTimerDurationUpgrade()
        {
            _isProcessing = true;

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "StartDurationUpgrade"
            };
            PlayFabClientAPI.ExecuteCloudScript(request, OnStartTimerDurationUpgradeSuccessful, OnStartTimerDurationUpgradeFailed);
        }

        void OnStartTimerDurationUpgradeSuccessful(ExecuteCloudScriptResult result)
        {
            if(result.Error != null)
            {
                ThrowError(result.Error.Message);
            }

            if(bool.TryParse(result.FunctionResult.ToString(), out var success))
            {
                if (success)
                {
                    UnityEngine.Debug.Log("Upgrade Started!");
                    _durationUpgradeSuccesful = true;
                }
            }

            _isProcessing = false;
        }

        void OnStartTimerDurationUpgradeFailed(PlayFabError error)
        {
            ThrowError(error.ErrorMessage);
        }

        void ThrowError(string error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to start timer duration upgrade. {error}");
        }
    }

    public class DurationUpgradeResultAsync : IResultAsync
    {
        public bool Success;

        public DurationUpgradeResultAsync(bool success)
        {
            Success = success;
        }
    }
}
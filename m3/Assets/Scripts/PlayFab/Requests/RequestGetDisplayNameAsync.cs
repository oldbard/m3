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
    /// Request to get the player Display Name
    /// </summary>
    public class RequestGetDisplayNameAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        string _displayName;

        public async Task<IResultAsync> Process()
        {
            GetPlayerName();

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new GetDisplayNameResultAsync(_displayName);
        }

        void GetPlayerName()
        {
            _isProcessing = true;

            var request = new GetAccountInfoRequest();
            PlayFabClientAPI.GetAccountInfo(request, OnSetDisplayNameSuccessful, OnSetDisplayNameFailed);
        }

        void OnSetDisplayNameSuccessful(GetAccountInfoResult result)
        {
            _displayName = result.AccountInfo.TitleInfo.DisplayName;

#if UNITY_EDITOR
            Debug.Log("Successfully changed Display Name");
#endif

            _isProcessing = false;
        }

        void OnSetDisplayNameFailed(PlayFabError error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to change Display Name. {error.ErrorMessage}");
        }
    }

    public class GetDisplayNameResultAsync : IResultAsync
    {
        public string DisplayName;

        public GetDisplayNameResultAsync(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
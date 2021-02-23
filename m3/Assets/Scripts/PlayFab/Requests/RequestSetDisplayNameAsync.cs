using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Requests
{
    public class RequestSetDisplayNameAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        string _displayName;
        string _resultDisplayName;

        public RequestSetDisplayNameAsync(string displayName)
        {
            _displayName = displayName;
        }

        public async Task<IResultAsync> Process()
        {
            UpdatePlayerName(_displayName);

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new SetDisplayNameResultAsync(_resultDisplayName);
        }

        void UpdatePlayerName(string userName)
        {
            _isProcessing = true;

            var request = new UpdateUserTitleDisplayNameRequest { DisplayName = userName };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnSetDisplayNameSuccessful, OnSetDisplayNameFailed);
        }

        void OnSetDisplayNameSuccessful(UpdateUserTitleDisplayNameResult result)
        {
            _resultDisplayName = result.DisplayName;

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

    public class SetDisplayNameResultAsync : IResultAsync
    {
        public string DisplayName;

        public SetDisplayNameResultAsync(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
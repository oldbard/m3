using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Requests
{
    public class RequestPlayerDataAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        Dictionary<string, UserDataRecord> _playerData;
        List<string> _keys;

        public RequestPlayerDataAsync(List<string> keys)
        {
            _keys = keys;
        }

        public async Task<IResultAsync> Process()
        {
            RequestPlayerData(_keys);

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new GamePlayerDataResultAsync(_playerData);
        }

        void RequestPlayerData(List<string> keys)
        {
            _isProcessing = true;

            var request = new GetUserDataRequest { Keys = keys };
            PlayFabClientAPI.GetUserReadOnlyData(request, OnGetPlayerDataSuccessful, OnGetPlayerDataFailed);
        }

        void OnGetPlayerDataSuccessful(GetUserDataResult result)
        {
#if UNITY_EDITOR
            Debug.Log("Managed to get the player data");
#endif
            _playerData = result.Data;

            _isProcessing = false;
        }

        void OnGetPlayerDataFailed(PlayFabError error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to get the player data. {error.ErrorMessage}");
        }
    }

    public class GamePlayerDataResultAsync : IResultAsync
    {
        public Dictionary<string, string> PlayerData;

        public GamePlayerDataResultAsync(Dictionary<string, UserDataRecord> playerData)
        {
            PlayerData = new Dictionary<string, string>(playerData.Count);

            foreach (var data in playerData)
            {
                PlayerData.Add(data.Key, data.Value.Value);
            }
        }
    }
}
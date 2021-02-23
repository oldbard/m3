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
    public class RequestTitleDataAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        Dictionary<string, string> _gameData;
        List<string> _keys;

        public RequestTitleDataAsync(List<string> keys)
        {
            _keys = keys;
        }

        public async Task<IResultAsync> Process()
        {
            RequestTitleData(_keys);

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new GameTitleDataResultAsync(_gameData);
        }

        void RequestTitleData(List<string> keys)
        {
            _isProcessing = true;

            var request = new GetTitleDataRequest { Keys = keys };
            PlayFabClientAPI.GetTitleData(request, OnGetTitleDataSuccessful, OnGetTitleDataFailed);
        }

        void OnGetTitleDataSuccessful(GetTitleDataResult result)
        {
#if UNITY_EDITOR
            Debug.Log("Managed to get the title data");
#endif
            _gameData = result.Data;

            _isProcessing = false;
        }

        void OnGetTitleDataFailed(PlayFabError error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to get the title data. {error.ErrorMessage}");
        }
    }

    public class GameTitleDataResultAsync : IResultAsync
    {
        public Dictionary<string, string> GameData;

        public GameTitleDataResultAsync(Dictionary<string, string> gameData)
        {
            GameData = gameData;
        }
    }
}
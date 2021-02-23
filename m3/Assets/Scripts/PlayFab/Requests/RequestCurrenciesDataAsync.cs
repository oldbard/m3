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
    /// Request to get the player currencies data
    /// </summary>
    public class RequestCurrenciesDataAsync : IRequestAsync
    {
        const string SOFT_CURENCY = "SC";
        const string HARD_CURENCY = "HC";

        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        uint _sc;
        uint _hc;

        public async Task<IResultAsync> Process()
        {
            RequestCurrenciesData();

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new GameCurrenciesDataResultAsync(_sc, _hc);
        }

        void RequestCurrenciesData()
        {
            _isProcessing = true;

            var request = new GetUserInventoryRequest();
            PlayFabClientAPI.GetUserInventory(request, OnGetCurrenciesDataSuccessful, OnGetCurrenciesDataFailed);
        }

        void OnGetCurrenciesDataSuccessful(GetUserInventoryResult result)
        {
#if UNITY_EDITOR
            Debug.Log("Managed to get the title data");
#endif
            _sc = (uint)result.VirtualCurrency[SOFT_CURENCY];
            _hc = (uint)result.VirtualCurrency[HARD_CURENCY];

            _isProcessing = false;
        }

        void OnGetCurrenciesDataFailed(PlayFabError error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to get the title data. {error.ErrorMessage}");
        }
    }

    public class GameCurrenciesDataResultAsync : IResultAsync
    {
        public uint SC;
        public uint HC;

        public GameCurrenciesDataResultAsync(uint sc, uint hc)
        {
            SC = sc;
            HC = hc;
        }
    }
}
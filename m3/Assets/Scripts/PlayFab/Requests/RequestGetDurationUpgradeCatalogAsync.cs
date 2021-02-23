using GameData;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading.Tasks;

namespace Requests
{
    /// <summary>
    /// Request to get the Timer catalog item
    /// </summary>
    public class RequestGetDurationUpgradeCatalogAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        CatalogConfigData _durationUpgrade;

        public async Task<IResultAsync> Process()
        {
            RequestCatalogItems();

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new DurationUpgradeCatalogResultAsync(_durationUpgrade);
        }

        void RequestCatalogItems()
        {
            _isProcessing = true;

            var req = new GetCatalogItemsRequest { CatalogVersion = "1" };

            PlayFabClientAPI.GetCatalogItems(req, OnCatalogRequestSuccessful, OnCatalogRequestFailed);
        }

        void OnCatalogRequestSuccessful(GetCatalogItemsResult result)
        {
            const string SoftCurrency = "SC";
            const string HardCurrency = "HC";

            var catalogItem = result.Catalog[0];

            _durationUpgrade = new CatalogConfigData();
            _durationUpgrade.Parse(catalogItem.ItemId, catalogItem.VirtualCurrencyPrices[SoftCurrency],
                catalogItem.VirtualCurrencyPrices[HardCurrency], catalogItem.CustomData);

            _isProcessing = false;
        }

        void OnCatalogRequestFailed(PlayFabError error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to get shop catalog. {error.ErrorMessage}");
        }
    }

    public class DurationUpgradeCatalogResultAsync : IResultAsync
    {
        public CatalogConfigData DurationUpgradeItem;

        public DurationUpgradeCatalogResultAsync(CatalogConfigData durationUpgradeItem)
        {
            DurationUpgradeItem = durationUpgradeItem;
        }
    }
}
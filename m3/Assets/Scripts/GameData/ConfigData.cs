using PlayFab;
using PlayFab.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GameData
{
    public class GamePersistentData
    {
        public ConfigData ConfigData;
        public UserData UserData;

        public static GamePersistentData Instance;

        public GamePersistentData()
        {
            Instance = this;

            ConfigData = new ConfigData();
            UserData = new UserData();
        }

        public void ParseData(Dictionary<string, string> configData,
            Dictionary<string, int> currenciesData, List<CatalogConfigData> catalog)
        {
            Instance = this;

            ConfigData.Catalog = catalog;

            ConfigData.ParseData(configData);
            UserData.ParseData(currenciesData);
        }
    }

    public class ConfigData
    {
        // Keys
        const string DestroyAnimationTimeKey = "DestroyAnimationTime";
        const string DropAnimationTimeKey = "DropAnimationTime";
        const string FullColorBlinkDelayKey = "FullColorBlinkDelay";
        const string GameDurationKey = "GameDuration";
        const string HintAnimationTimeKey = "HintAnimationTime";
        const string HintCyclesKey = "HintCycles";
        const string PointsPerTileKey = "PointsPerTile";
        const string StartToBlinkDelayKey = "StartToBlinkDelay";
        const string SwapAnimationTimeKey = "SwapAnimationTime";
        const string TimeToShowHintKey = "TimeToShowHint";
        const string TimeToShowWarningKey = "TimeToShowWarning";
        const string UpgradablesKey = "Upgradables";

        // Timers
        public float DestroyAnimationTime = 0.5f;
        public float DropAnimationTime = 0.5f;
        public float SwapAnimationTime = 0.5f;
        public float HintAnimationTime = 0.5f;
        public int TimeToShowHint = 10;
        public int HintCycles = 3;
        public int GameDuration = 60;
        public int TimeToShowWarning = 5;

        // Blink Timers
        public int StartBlinkDelay = 500;
        public int FullColorBlinkDelay = 200;

        // Score
        public int PointsPerTile = 10;

        // Upgradables
        public List<CatalogConfigData> Catalog;

        public void ParseData(Dictionary<string, string> configData)
        {
            try
            {
                float.TryParse(configData[DestroyAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out DestroyAnimationTime);
                float.TryParse(configData[DropAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out DropAnimationTime);
                float.TryParse(configData[SwapAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out SwapAnimationTime);
                float.TryParse(configData[HintAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out HintAnimationTime);
                int.TryParse(configData[HintCyclesKey], out HintCycles);
                int.TryParse(configData[GameDurationKey], out GameDuration);
                int.TryParse(configData[TimeToShowHintKey], out TimeToShowHint);
                int.TryParse(configData[TimeToShowWarningKey], out TimeToShowWarning);
                int.TryParse(configData[StartToBlinkDelayKey], out StartBlinkDelay);
                int.TryParse(configData[FullColorBlinkDelayKey], out FullColorBlinkDelay);
                int.TryParse(configData[PointsPerTileKey], out PointsPerTile);

                var upgradablesData = configData[UpgradablesKey];
                ParseUpgradablesData(upgradablesData);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        void ParseUpgradablesData(string upgradablesData)
        {
            var plugin = PluginManager.GetPlugin<ISerializerPlugin>
                (PluginContract.PlayFab_Serializer);

            var list = (List<object>)plugin.DeserializeObject(upgradablesData);

            foreach (var item in list)
            {
                var jsonObject = (JsonObject)item;

                var enumerator = jsonObject.GetEnumerator();

                enumerator.MoveNext();

                var catalogItem = GetCatalogItem(enumerator.Current.Key);

                int level;
                int.TryParse(enumerator.Current.Value.ToString(), out level);

                catalogItem.SetLevel(level);
            }
        }

        CatalogConfigData GetCatalogItem(string id)
        {
            foreach (var item in Catalog)
            {
                if (string.CompareOrdinal(item.ID, id) == 0)
                    return item;
            }
            return null;
        }
    }

    public class UserData
    {
        // Keys
        const string GoldKey = "SC";
        const string GemsKey = "HC";

        int _gold;
        int _gems;

        public int Gold
        {
            get => _gold;
        }

        public int Gems
        {
            get => _gems;
        }

        public Action<int> GoldChanged;
        public Action<int> GemsChanged;

        public void ParseData(Dictionary<string, int> currenciesData)
        {
            try
            {
                AddGold(currenciesData[GoldKey]);
                AddGems(currenciesData[GemsKey]);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddGold(int amount)
        {
            _gold += amount;
            GoldChanged?.Invoke(_gold);
        }

        public void AddGems(int amount)
        {
            _gems += amount;
            GemsChanged?.Invoke(_gems);
        }
    }
}
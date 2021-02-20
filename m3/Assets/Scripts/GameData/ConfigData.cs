using PlayFab;
using PlayFab.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GameData
{
    public class GamePersistentData
    {
        const string UpgradablesKey = "Upgradables";

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
            Dictionary<string, int> currenciesData, CatalogConfigData catalog)
        {
            Instance = this;

            ConfigData.Catalog = catalog;

            ConfigData.ParseData(configData);

            var upgradablesData = configData[UpgradablesKey];
            UserData.ParseData(currenciesData, upgradablesData);
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

        // Timers
        public float DestroyAnimationTime = 0.5f;
        public float DropAnimationTime = 0.5f;
        public float SwapAnimationTime = 0.5f;
        public float HintAnimationTime = 0.5f;
        public int TimeToShowHint = 10;
        public int HintCycles = 3;
        public int TimeToShowWarning = 5;
        
        int _initialGameDuration = 60;

        // Blink Timers
        public int StartBlinkDelay = 500;
        public int FullColorBlinkDelay = 200;

        // Score
        public int PointsPerTile = 10;

        // Upgradables
        public CatalogConfigData Catalog;

        public int GameDuration
        {
            get
            {
                var duration = _initialGameDuration;
                var level = GamePersistentData.Instance.UserData.DurationLevel;

                for (int i = 1; i < level; i++)
                {
                    duration += Catalog.UpgradePerLevel;
                }

                return duration;
            }
        }


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
                int.TryParse(configData[GameDurationKey], out _initialGameDuration);
                int.TryParse(configData[TimeToShowHintKey], out TimeToShowHint);
                int.TryParse(configData[TimeToShowWarningKey], out TimeToShowWarning);
                int.TryParse(configData[StartToBlinkDelayKey], out StartBlinkDelay);
                int.TryParse(configData[FullColorBlinkDelayKey], out FullColorBlinkDelay);
                int.TryParse(configData[PointsPerTileKey], out PointsPerTile);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

    public class UserData
    {
        // Keys
        const string GoldKey = "SC";
        const string GemsKey = "HC";
        const string UpgradablesLevelKey = "Level";
        const string UpgradablesTimestampKey = "UpgradeStartTimestamp";

        int _gold;

        public int Gold
        {
            get => _gold;
        }

        int _gems;

        public int Gems
        {
            get => _gems;
        }

        int _matchDurationLevel;

        public int DurationLevel
        {
            get => _matchDurationLevel;
        }

        long _matchDurationUpgradeTimestamp;

        public long UpgradeStartedTimeStamp
        {
            get => _matchDurationUpgradeTimestamp;
        }

        public bool IsUpgradingTimer
        {
            get => _matchDurationUpgradeTimestamp > 0;
        }

        public Action<int> GoldChanged;
        public Action<int> GemsChanged;
        public Action DataLoaded;

        public void ParseData(Dictionary<string, int> currenciesData, string upgradablesData)
        {
            try
            {
                AddGold(currenciesData[GoldKey]);
                AddGems(currenciesData[GemsKey]);
                ParseUpgradablesData(upgradablesData);

                DataLoaded?.Invoke();
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

            var jsonObject = (JsonObject)plugin.DeserializeObject(upgradablesData);

            object jsonValue;
            int level;
            long timeStamp;

            if (jsonObject.TryGetValue(UpgradablesLevelKey, out jsonValue))
            {
                int.TryParse(jsonValue.ToString(), out level);
            }
            else
            {
                throw new Exception($"Could not parse the field {UpgradablesLevelKey}");
            }

            if (jsonObject.TryGetValue(UpgradablesTimestampKey, out jsonValue))
            {
                long.TryParse(jsonValue.ToString(), out timeStamp);
            }
            else
            {
                throw new Exception($"Could not parse the field {UpgradablesTimestampKey}");
            }

            _matchDurationLevel = level;
            _matchDurationUpgradeTimestamp = timeStamp;
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

        // TODO: Remove this
        public void SetTimer(long timeStamp)
        {
            _matchDurationUpgradeTimestamp = timeStamp;
        }

        // TODO: Remove this
        public void SetTimerLevel(int level)
        {
            _matchDurationLevel = level;
        }
    }
}
using GameServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GameData
{
    public class GamePersistentData : IService
    {
        public ConfigData ConfigData;
        public UserData UserData;

        public Dictionary<string, int> Leaderboard;

        public GamePersistentData()
        {
            Services.RegisterService<GamePersistentData>(this);

            ConfigData = new ConfigData();
            UserData = new UserData();
        }

        public void ParseData(Dictionary<string, string> configData, Dictionary<string, string> playerData,
            uint sc, uint hc, CatalogConfigData catalog)
        {
            ConfigData.Catalog = catalog;

            ConfigData.ParseData(configData);

            ParseUpgradablesData(playerData);
            ParseCurrenciesData(sc, hc);
        }

        public void ParseUpgradablesData(Dictionary<string, string> configData)
        {
            UserData.ParseUpgradablesData(configData);
        }

        public void ParseCurrenciesData(Dictionary<string, int> currenciesData)
        {
            UserData.ParseData(currenciesData);
        }

        public void ParseCurrenciesData(uint sc, uint hc)
        {
            UserData.ParseCurrenciesData(sc, hc);
        }

        public void SetLeaderboard(Dictionary<string, int> leaderboard)
        {
            var ordered = leaderboard.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            Leaderboard = ordered;
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


        GamePersistentData _gamePersistentData;
        
        GamePersistentData GamePersistentData
        {
            get
            {
                if(_gamePersistentData == null)
                {
                    _gamePersistentData = Services.Resolve<GamePersistentData>();
                }

                return _gamePersistentData;
            }
        }

        public int GameDuration
        {
            get
            {
                var duration = _initialGameDuration;
                var level = GamePersistentData.UserData.DurationLevel;

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
        const string UpgradablesLevelKey = "UpgradeDurationLevel";
        const string UpgradablesTimestampKey = "UpgradeDurationTimestamp";

        uint _gold;

        public uint Gold
        {
            get => _gold;
        }

        uint _gems;

        public uint Gems
        {
            get => _gems;
        }

        int _matchDurationLevel = 1;

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

        public Action<uint> GoldChanged;
        public Action<uint> GemsChanged;
        public Action DataLoaded;

        public void ParseData(Dictionary<string, int> currenciesData)
        {
            try
            {
                var sc = (uint)currenciesData[GoldKey];
                var hc = (uint)currenciesData[GemsKey];

                ParseCurrenciesData(sc, hc);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void ParseCurrenciesData(uint sc, uint hc)
        {
            SetGold(sc);
            SetGems(hc);

            DataLoaded?.Invoke();
        }

        public void ParseUpgradablesData(Dictionary<string, string> configData)
        {
            if (configData.Count == 0) return;

            int.TryParse(configData[UpgradablesLevelKey], out _matchDurationLevel);
            long.TryParse(configData[UpgradablesTimestampKey], out _matchDurationUpgradeTimestamp);
        }

        public void SetGold(uint amount)
        {
            _gold = amount;
            GoldChanged?.Invoke(_gold);
        }

        public void DecreaseGold(uint amount)
        {
            _gold -= amount;
            GoldChanged?.Invoke(_gold);
        }

        public void SetGems(uint amount)
        {
            _gems = amount;
            GemsChanged?.Invoke(_gems);
        }

        public void DecreaseGems(uint amount)
        {
            _gems -= amount;
            GemsChanged?.Invoke(_gems);
        }

        public void SetDurationUpgradeTime(long timeStamp)
        {
            _matchDurationUpgradeTimestamp = timeStamp;
        }
    }
}
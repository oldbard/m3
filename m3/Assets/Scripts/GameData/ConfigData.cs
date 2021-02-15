using System;
using System.Collections.Generic;
using System.Globalization;

namespace Data
{
    public class GamePersistenData
    {
        public ConfigData ConfigData;
        public UserData UserData;

        public static GamePersistenData Instance;

        public GamePersistenData(Dictionary<string, string> configData, Dictionary<string, int> currenciesData)
        {
            Instance = this;

            ConfigData = new ConfigData(configData);
            UserData = new UserData(currenciesData);
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
        readonly public float DestroyAnimationTime = 0.5f;
        readonly public float DropAnimationTime = 0.5f;
        readonly public float SwapAnimationTime = 0.5f;
        readonly public float HintAnimationTime = 0.5f;
        readonly public int TimeToShowHint = 10;
        readonly public int HintCycles = 3;
        readonly public int GameDuration = 60;
        readonly public int TimeToShowWarning = 5;

        // Blink Timers
        public int StartBlinkDelay = 500;
        public int FullColorBlinkDelay = 200;

        // Score
        public int PointsPerTile = 10;

        public ConfigData(Dictionary<string, string> configData)
        {

            try
            {
                float.TryParse(configData[DestroyAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out DestroyAnimationTime);
                float.TryParse(configData[DestroyAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out DestroyAnimationTime);
                float.TryParse(configData[DropAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out DropAnimationTime);
                float.TryParse(configData[SwapAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out SwapAnimationTime);
                float.TryParse(configData[HintAnimationTimeKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out HintAnimationTime);
                int.TryParse(configData[TimeToShowHintKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out TimeToShowHint);
                int.TryParse(configData[HintCyclesKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out HintCycles);
                int.TryParse(configData[GameDurationKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out GameDuration);
                int.TryParse(configData[TimeToShowWarningKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out TimeToShowWarning);
                int.TryParse(configData[StartToBlinkDelayKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out StartBlinkDelay);
                int.TryParse(configData[FullColorBlinkDelayKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out FullColorBlinkDelay);
                int.TryParse(configData[PointsPerTileKey], NumberStyles.Any, CultureInfo.InvariantCulture,
                    out PointsPerTile);
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

        public UserData(Dictionary<string, int> currenciesData)
        {
            try
            {
                _gold = currenciesData[GoldKey];
                _gems = currenciesData[GemsKey];
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
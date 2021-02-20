using PlayFab;
using PlayFab.Json;
using Shared;
using System;
using System.Globalization;

namespace GameData
{
    public class CatalogConfigData
    {
        const string UpgradePerLevelKey = "UpgradePerLevel";
        const string UpgradeCostMultiplierKey = "UpgradeMultiplier";
        const string InitialValueKey = "InitialValue";
        const string InitialDurationKey = "Duration";

        public GridManager.TileType TileType;
        public string ID;

        int _upgradePerLevel;
        int _initialValue;
        uint _initialCost;
        uint _initialSkipCost;
        float _upgradeCostMultiplier;
        int _initialUpgradeDuration;

        public int UpgradePerLevel { get => _upgradePerLevel; }

        public int CurrentValue
        {
            get
            {
                var curValue = _initialValue;
                var level = GamePersistentData.Instance.UserData.DurationLevel;

                for(int i = 1; i < level; i++)
                {
                    curValue += _upgradePerLevel;
                }

                return curValue;
            }
        }

        public uint UpgradeCost
        {
            get
            {
                var cost = (float)_initialCost;
                var level = GamePersistentData.Instance.UserData.DurationLevel;

                for (var i = 1; i < level; i++)
                {
                    cost *= _upgradeCostMultiplier;
                }

                return (uint)Math.Ceiling(cost);
            }
        }

        public uint SkipUpgradeCost
        {
            get
            {
                var cost = (float)_initialSkipCost;
                var level = GamePersistentData.Instance.UserData.DurationLevel;

                for (var i = 1; i < level; i++)
                {
                    cost *= _upgradeCostMultiplier;
                }

                return (uint)Math.Ceiling(cost);
            }
        }

        public int UpgradeDuration
        {
            get
            {
                var duration = (float)_initialUpgradeDuration;
                var level = GamePersistentData.Instance.UserData.DurationLevel;

                for (var i = 1; i < level; i++)
                {
                    duration *= _upgradeCostMultiplier;
                }

                return (int)Math.Ceiling(duration);
            }
        }

        public void Parse(string id, uint cost, uint skipCost, string customData)
        {
            try
            {
                ID = id;

                _initialCost = cost;
                _initialSkipCost = skipCost;

                var jsonObject = (JsonObject)PluginManager.GetPlugin<ISerializerPlugin>
                    (PluginContract.PlayFab_Serializer).DeserializeObject(customData);
                
                object jsonValue;
                if(jsonObject.TryGetValue(UpgradePerLevelKey, out jsonValue))
                {
                    int.TryParse(jsonValue.ToString(), out _upgradePerLevel);
                }
                else
                {
                    throw new Exception($"Could not parse the field {UpgradePerLevelKey}");
                }

                if(jsonObject.TryGetValue(UpgradeCostMultiplierKey, out jsonValue))
                {
                    float.TryParse(jsonValue.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out _upgradeCostMultiplier);
                }
                else
                {
                    throw new Exception($"Could not parse the field {UpgradeCostMultiplierKey}");
                }

                if (jsonObject.TryGetValue(InitialValueKey, out jsonValue))
                {
                    int.TryParse(jsonValue.ToString(), out _initialValue);
                }
                else
                {
                    throw new Exception($"Could not parse the field {InitialValueKey}");
                }

                if (jsonObject.TryGetValue(InitialDurationKey, out jsonValue))
                {
                    int.TryParse(jsonValue.ToString(), out _initialUpgradeDuration);
                }
                else
                {
                    throw new Exception($"Could not parse the field {InitialDurationKey}");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
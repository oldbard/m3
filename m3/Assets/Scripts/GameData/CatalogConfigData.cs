using PlayFab;
using PlayFab.Json;
using Shared;
using System;
using System.Globalization;

namespace GameData
{
    public class CatalogConfigData
    {
  //      const string LevelKey = "Level";
        const string UpgradePerLevelKey = "UpgradePerLevel";
        const string UpgradeCostMultiplierKey = "UpgradeCostMultiplier";

        public GridManager.TileType TileType;
        public string ID;

        int _level = 1;
        int _upgradePerLevel;
        int _initialScorePoints;
        uint _initialCost;
        float _upgradeCostMultiplier;

        public int ScorePoints
        {
            get
            {
                var score = _initialScorePoints;

                for(int i = 1; i < _level; i++)
                {
                    score += _upgradePerLevel;
                }

                return score;
            }
        }

        public uint UpgradeCost
        {
            get
            {
                var cost = (float)_initialCost;

                for(var i = 1; i < _level + 1; i++)
                {
                    cost *= _upgradeCostMultiplier;
                }

                return (uint)Math.Ceiling(cost);
            }
        }

        public void Parse(string id, uint cost, string customData)
        {
            try
            {
                _initialCost = cost;

                ParseType(id);

                var jsonObject = (JsonObject)PluginManager.GetPlugin<ISerializerPlugin>
                    (PluginContract.PlayFab_Serializer).DeserializeObject(customData);
                
                object jsonValue;
                if (jsonObject.TryGetValue(UpgradePerLevelKey, out jsonValue))
                {
                    int.TryParse(jsonValue.ToString(), out _upgradePerLevel);
                }
                else
                {
                    throw new Exception($"Could not parse the field {UpgradePerLevelKey}");
                }

                if (jsonObject.TryGetValue(UpgradeCostMultiplierKey, out jsonValue))
                {
                    float.TryParse(jsonValue.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                        out _upgradeCostMultiplier);
                }
                else
                {
                    throw new Exception($"Could not parse the field {UpgradeCostMultiplierKey}");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        void ParseType(string id)
        {
            ID = id;

            switch (id)
            {
                case "BlueTile":
                    TileType = GridManager.TileType.Blue;
                    break;
                case "GreenTile":
                    TileType = GridManager.TileType.Green;
                    break;
                case "OrangeTile":
                    TileType = GridManager.TileType.Orange;
                    break;
                case "RedTile":
                    TileType = GridManager.TileType.Red;
                    break;
                case "YellowTile":
                    TileType = GridManager.TileType.Yellow;
                    break;
            }
        }

        public void SetLevel(int level)
        {
            _level = level;
        }
    }
}
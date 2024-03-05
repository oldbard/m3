using OldBard.Services.Match3.Grid;
using OldBard.Services.Match3.Grid.Data;
using OldBard.Services.Match3.Grid.Views;
using UnityEngine;

namespace OldBard.Match3.Config
{
    /// <summary>
    /// Game Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "Config", menuName = "M3/Config", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Input")]
        public float DragDetectionThreshold = 0.3f;

        [Header("Timers")]
        public float DropAnimationTime = 0.5f;
        public float SwapAnimationTime = 0.5f;
        public float DestroyAnimationTime = 0.5f;
        public float HintAnimationTime = 0.5f;
        public int TimeToShowHint = 10;
        public int HintCycles = 3;
        public int GameDuration = 60;
        public int TimeToShowWarning = 5;

        [Header("Blink Timers")]
        public int StartBlinkDelay = 500;
        public int FullColorBlinkDelay = 200;

        [Header("Score")]
        public int PointsPerTile = 10;

        [Header("Prefabs")]
        public GameObject TileBackgroundPrefab;
        public TileView TilePrefab;
        public int TilesVariations;

        [Header("Animations")]
        public AnimationCurve LerpAnimationCurve;

        [Header("Tiles")]
        [SerializeField] TileData[] _tilesData;

        /// <summary>
        /// Number of Tile Types
        /// </summary>
        public int NumberOfTileTypes => _tilesData.Length;

        /// <summary>
        /// Gets the TileViewData for the specific type and variation index
        /// </summary>
        /// <param name="tileType">The requested type</param>
        /// <param name="variation">The specific variation index</param>
        /// <returns>The TileViewData for the specific information</returns>
        public TileViewData GetViewData(TileType tileType, int variation)
        {
            foreach(TileData tileData in _tilesData)
            {
                if(tileData.TileType == tileType)
                {
                    return tileData.ViewData[variation];
                }
            }

            return default;
        }
    }
}

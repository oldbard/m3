using OldBard.Services.Match3.Grid.View;
using UnityEngine;

namespace OldBard.Services.Match3.Grid.Data
{
    /// <summary>
    /// Game Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "M3/GameConfig", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Input")]
        [SerializeField] float _dragDetectionThreshold = 10f;

        [Header("Timers")]
        [SerializeField] float _dropAnimationTime = 0.5f;
        [SerializeField] float _swapAnimationTime = 0.5f;
        [SerializeField] float _destroyAnimationTime = 0.5f;
        [SerializeField] float _hintAnimationTime = 0.5f;
        [SerializeField] int _timeToShowHint = 10;
        [SerializeField] int _hintCycles = 3;
        [SerializeField] int _gameDuration = 20;
        [SerializeField] int _timeToShowWarning = 5;

        [Header("Blink Timers")]
        [SerializeField] int _startBlinkDelay = 500;
        [SerializeField] int _fullColorBlinkDelay = 200;

        [Header("Score")]
        [SerializeField] int _pointsPerTile = 10;

        [Header("Prefabs")]
        [SerializeField] GameObject _tileBackgroundPrefab;
        [SerializeField] TileView _tilePrefab;
        [SerializeField] int _tilesVariations = 2;

        [Header("Animations")]
        [SerializeField] AnimationCurve _lerpAnimationCurve;

        [Header("Tiles")]
        [SerializeField] TileData[] _tilesData;

        public float DragDetectionThreshold => _dragDetectionThreshold;
        public float DropAnimationTime => _dropAnimationTime;
        public float SwapAnimationTime => _swapAnimationTime;
        public float DestroyAnimationTime => _destroyAnimationTime;
        public float HintAnimationTime => _hintAnimationTime;
        public int TimeToShowHint => _timeToShowHint;
        public int HintCycles => _hintCycles;
        public int GameDuration => _gameDuration;
        public int TimeToShowWarning => _timeToShowWarning;
        public int StartBlinkDelay => _startBlinkDelay;
        public int FullColorBlinkDelay => _fullColorBlinkDelay;
        public int PointsPerTile => _pointsPerTile;
        public GameObject TileBackgroundPrefab => _tileBackgroundPrefab;
        public TileView TilePrefab => _tilePrefab;
        public int TilesVariations => _tilesVariations;
        public AnimationCurve LerpAnimationCurve => _lerpAnimationCurve;

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

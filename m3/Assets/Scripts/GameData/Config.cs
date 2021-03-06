﻿using System;
using UI;
using UnityEngine;
using Shared;

namespace GameData
{
    /// <summary>
    /// Struct to hold the Tiles possible configurations
    /// </summary>
    [Serializable]
    public struct TileData
    {
        public GridManager.TileType TileType;
        public TileViewData[] ViewData;
    }

    /// <summary>
    /// Struct to hold view information for the Tiles creation
    /// </summary>
    [Serializable]
    public struct TileViewData
    {
        public Sprite Body;
        public Sprite Eye;
        public Sprite Mouth;
        public Sprite Shadow;
        public Sprite Selected;
        public RuntimeAnimatorController Animation;
    }

    /// <summary>
    /// Game Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "Config", menuName = "M3/Config", order = 1)]
    public class Config : ScriptableObject
    {
        /// <summary>
        /// Enumeration of the available Tile Types
        /// </summary>
        /*public enum TileType
        {
            Blue,
            Green,
            Orange,
            Red,
            Yellow,
            Count
        }*/

        [Header("Grid")]
        public int DefaultGridWidth;
        public int DefaultGridHeight;

        [Header("Grid View")]
        public float TileViewWidth;
        public float TileViewHeight;

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

        [Header("BGM")]
        [SerializeField] AudioClip[] _bgms;
        public AudioClip GameOverSound;
        public AudioClip GameOverHighScoreSound;
        public AudioClip SwapSound;
        public AudioClip MatchSound;
        public AudioClip TimeoutSound;

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
        public TileViewData GetViewData(GridManager.TileType tileType, int variation)
        {
            for(var i = 0; i < _tilesData.Length; i++)
            {
                var tileData = _tilesData[i];
                if(tileData.TileType == tileType)
                {
                    return tileData.ViewData[variation];
                }
            }

            return default;
        }

        /// <summary>
        /// The total of BGMs available
        /// </summary>
        public int TotalBGMs => _bgms.Length;
        
        /// <summary>
        /// Gets a BGM based on the given variation
        /// </summary>
        /// <param name="variation">Variation index</param>
        /// <returns>The requested AudioClip</returns>
        public AudioClip GetBGM(int variation)
        {
            return _bgms[variation];
        }
    }
}

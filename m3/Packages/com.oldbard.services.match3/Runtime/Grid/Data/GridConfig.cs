﻿using UnityEngine;

namespace OldBard.Services.Match3.Grid
{
    [CreateAssetMenu(fileName = "GridConfig", menuName = "M3/GridConfig", order = 1)]
    public class GridConfig : ScriptableObject
    {
        [Header("Grid")]
        [SerializeField] int _defaultGridWidth;
        [SerializeField] int _defaultGridHeight;

        [Header("Grid View")]
        [SerializeField] float _tileViewWidth;
        [SerializeField] float _tileViewHeight;
        [SerializeField] int _yCascadePositionOffset;

        public int DefaultGridWidth => _defaultGridWidth;
        public int DefaultGridHeight => _defaultGridHeight;
        public float TileViewWidth => _tileViewWidth;
        public float TileViewHeight => _tileViewHeight;
        public int YCascadePositionOffset => _yCascadePositionOffset;
    }
}
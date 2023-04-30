using UnityEngine;

namespace OldBard.Services.Match3.Grid
{
    [CreateAssetMenu(fileName = "GridSettings", menuName = "M3/GridSettings", order = 1)]
    public class GridSettings : ScriptableObject
    {
        [Header("Grid")]
        public int DefaultGridWidth;
        public int DefaultGridHeight;

        [Header("Grid View")]
        public float TileViewWidth;
        public float TileViewHeight;
    }
}
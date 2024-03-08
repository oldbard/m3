using System;

namespace OldBard.Services.Match3.Grid.Tiles.Data
{
    /// <summary>
    /// Struct to hold the Tiles possible configurations
    /// </summary>
    [Serializable]
    public struct TileData
    {
        public TileType TileType;
        public TileViewData[] ViewData;
    }
}

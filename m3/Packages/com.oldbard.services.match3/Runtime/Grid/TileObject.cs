using System;

namespace OldBard.Services.Match3.Grid
{
    /// <summary>
    /// TileObject model data.
    /// </summary>
    [Serializable]
    public class TileObject
    {
        public int PosX;
        public int PosY;
        public TileType TileType;
        public bool Valid;
    }
}

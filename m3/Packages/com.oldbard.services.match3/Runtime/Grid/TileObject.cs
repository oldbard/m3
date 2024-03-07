using System;

namespace OldBard.Services.Match3.Grid
{
    /// <summary>
    /// TileData model data.
    /// </summary>
    [Serializable]
    public class TileObject
    {
        public int PosX;
        public int PosY;
        public TileType TileType;
        public bool Valid;

        public override string ToString()
        {
            return $"({PosX}, {PosY}), {TileType}, {Valid}";
        }
    }
}

using System;

namespace OldBard.Services.Match3.Grid
{
    public interface IGridService : IDisposable
    {
        GridSettings GridSettings { get; }

        string DebugGrid();

        TileObject this[int x, int y] { get; }

        void SetTile(TileObject tile, int x, int y);

        int GridWidth { get; }
        int GridHeight { get; }

        bool HasPossibleMatch { get; }
    }
}
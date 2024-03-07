using System;
using System.Collections.Generic;
using UnityEngine;

namespace OldBard.Services.Match3.Grid
{
    public interface IGridService : IDisposable
    {
        GridConfig GridConfig { get; }

        TileInstance this[int x, int y] { get; }

        Vector3 GridOffset { get; }

        IReadOnlyList<TileInstance> Tiles { get; }

        int GridWidth { get; }
        int GridHeight { get; }

        void SetTile(TileInstance tile, int x, int y);

        void MoveTilesDown(List<TileInstance> tilesToUpdateView);

        bool GetFirstPossibleMatch(List<TileInstance> tiles);

        TileInstance GetNeighbourTile(TileInstance tile, DragDirection dir);

        bool TrySwapTiles(TileInstance tile1, TileInstance tile2, List<TileInstance> tiles);

        void ReleaseTiles(List<TileInstance> matchedTiles);

        void InvalidateTiles(List<TileInstance> matchedTiles);

        bool TryFindAndProcessMatches(List<TileInstance> matches);

        string DebugGrid();
    }
}
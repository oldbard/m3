using System;
using System.Collections.Generic;

namespace OldBard.Services.Match3.Grid
{
    public class GridModel : IDisposable
    {
        /// <summary>
        /// Array with all the tiles in the match
        /// </summary>
        TileInstance[] _tiles;

        public IReadOnlyList<TileInstance> Tiles => _tiles;

        public readonly int GridWidth;
        public readonly int GridHeight;
        
        /// <summary>
        /// Returns / Sets the Tile for the given coordinates
        /// </summary>
        /// <param name="x">Grid Column</param>
        /// <param name="y">Grid Row</param>
        public TileInstance this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
                {
                    return null;
                }

                return _tiles[x + y * GridWidth];

            }
            set
            {
                if (value != null)
                {
                    value.PosX = x;
                    value.PosY = y;
                }

                _tiles[x + y * GridWidth] = value;
            }
        }

        public GridModel(int width, int height)
        {
            GridWidth = width;
            GridHeight = height;

            var maxTiles = GridWidth * GridHeight;

            _tiles = new TileInstance[maxTiles];
        }

        public string DebugGrid()
        {
            var sb = new System.Text.StringBuilder();

            for (var y = 0; y < GridHeight; y++)
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    sb.Append($"{this[x, y].TileType}: ({x}, {y}), ");
                }

                sb.AppendLine("");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Swaps the positions of the given tiles
        /// </summary>
        /// <param name="tile1">Tile to swap position</param>
        /// <param name="tile2">Tile to swap position</param>
        public void SwapTilesPos(TileInstance tile1, TileInstance tile2)
        {
            var prevX = tile1.PosX;
            var prevY = tile1.PosY;

            this[tile2.PosX, tile2.PosY] = tile1;
            this[prevX, prevY] = tile2;
        }

        /// <summary>
        /// Gets the neighbour TileObject in the given direction
        /// </summary>
        /// <param name="tile">Tile to get the neighbour of</param>
        /// <param name="dir">The direction of the dragging</param>
        /// <returns>The Neighbour TileObject. Or Null if in an invalid position.</returns>
        public TileInstance GetNeighbourTile(TileInstance tile, DragDirection dir)
        {
            return dir switch
            {
                DragDirection.Up => tile.PosY > 0 ?
                                        this[tile.PosX, tile.PosY - 1] : null,
                DragDirection.Right => tile.PosX < GridWidth - 1 ?
                                        this[tile.PosX + 1, tile.PosY] : null,
                DragDirection.Down => tile.PosY < GridHeight ?
                                        this[tile.PosX, tile.PosY + 1] : null,
                DragDirection.Left => tile.PosX > 0 ?
                                        this[tile.PosX - 1, tile.PosY] : null,
                _ => null,
            };
        }

        /// <summary>
        /// Looks up for a tile in the given column <paramref name="x"/> starting by a
        /// given row <paramref name="fromY"/>
        /// </summary>
        /// <param name="x">Column</param>
        /// <param name="fromY">Initial row</param>
        /// <returns>The first tile above the given coordinates</returns>
        public TileInstance GetTileUp(int x, int fromY)
        {
            for (var y = fromY; y < GridHeight; y++)
            {
                TileInstance tile = this[x, y];
                if (tile == null)
                {
                    return null;
                }

                if (tile.IsValid)
                {
                    return tile;
                }
            }

            return null;
        }

        public void Dispose()
        {
            _tiles = null;
        }
    }
}

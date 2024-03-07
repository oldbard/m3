using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = System.Random;

namespace OldBard.Services.Match3.Grid
{
    class GridMatchesHelper
    {
        readonly IGridService _gridService;

        readonly int _tilesVariations;
        readonly Random _random;

        internal GridMatchesHelper(IGridService gridService, int variations, int randomSeed)
        {
            _gridService = gridService;

            _tilesVariations = variations;
            _random = new Random(randomSeed);
        }

        public void CreatePossibleMatch(List<TileInstance> possibleMatchTiles)
        {
            using(ListPool<TileInstance>.Get(out List<TileInstance> possibleMatches))
            {
                for(var x = _gridService.GridWidth - 1; x >= 0; x--)
                {
                    for(var y = _gridService.GridHeight - 1; y >= 0; y--)
                    {
                        TileInstance tile = _gridService[x, y];

                        if(tile == null)
                        {
                            continue;
                        }

                        possibleMatches.Clear();

                        FillHorizontalMatchesPair(tile, possibleMatches);
                        if(possibleMatches.Count > 1)
                        {
                            TileInstance changedTile = CreateHorizontalPreMatch(possibleMatches);
                            possibleMatchTiles.Add(changedTile);

                            return;
                        }

                        possibleMatches.Clear();

                        FillVerticalMatchesPair(tile, possibleMatchTiles);
                        if(possibleMatchTiles.Count > 1)
                        {
                            TileInstance changedTile = CreateVerticalPreMatch(possibleMatchTiles);
                            possibleMatchTiles.Add(changedTile);

                            return;
                        }
                    }
                }
            }

            CreateNewHorizontalPreMatch(possibleMatchTiles);
        }

        void CreateNewHorizontalPreMatch(List<TileInstance> possibleMatchTiles)
        {
            var initialX = _random.Next(0, _gridService.GridWidth - 4);
            var targetY = _random.Next(0, _gridService.GridHeight - 1);
            var tileType = (TileType) _random.Next(0, _tilesVariations);

            TileInstance tile = _gridService[initialX, targetY];
            tile.SetTileType(tileType);
            possibleMatchTiles.Add(tile);

            tile = _gridService[initialX + 1, targetY];
            tile.SetTileType(tileType);
            possibleMatchTiles.Add(tile);

            tile = _gridService[initialX + 3, targetY];
            tile.SetTileType(tileType);
            possibleMatchTiles.Add(tile);

            Debug.Log($"Found a pre match! Set tile at {0}, {targetY} to {tile.TileType}");
        }

        TileInstance CreateHorizontalPreMatch(List<TileInstance> tiles)
        {
            TileInstance tile1 = tiles[0];
            TileInstance tile2 = tiles[1];

            var targetX = 0;
            var targetY = tile1.PosY;

            if (tile1.PosX > tile2.PosX)
            {
                (tile2, tile1) = (tile1, tile2);
            }

            if (tile2.PosX - tile1.PosX > 1)
            {
                if (tile2.PosX < _gridService.GridWidth - 1)
                {
                    targetX = tile2.PosX + 1;
                }
                else
                {
                    targetX = tile1.PosX - 1;
                }
            }
            else
            {
                if (tile2.PosX + 2 < _gridService.GridWidth - 1)
                {
                    targetX = tile2.PosX + 2;
                }
                else if (tile1.PosX - 2 >= 0)
                {
                    targetX = tile1.PosX - 2;
                }
                else
                {
                    if (tile2.PosY > 0)
                    {
                        targetY = tile2.PosY > 0 ? tile2.PosY - 1 : tile2.PosY + 1;
                    }
                    targetX = tile2.PosX + 1;
                }
            }

            TileInstance tile = _gridService[targetX, targetY];
            Debug.Log($"Found a pre match! Set tile at {targetX}, {targetY} to {tile1.TileType}");
            tile.SetTileType(tile1.TileType);

            return tile;
        }

        TileInstance CreateVerticalPreMatch(List<TileInstance> tiles)
        {
            TileInstance tile1 = tiles[0];
            TileInstance tile2 = tiles[1];

            var targetX = tile1.PosX;
            var targetY = 0;

            if (tile1.PosY > tile2.PosY)
            {
                (tile2, tile1) = (tile1, tile2);
            }

            if (tile2.PosY - tile1.PosY > 1)
            {
                if (tile2.PosY < _gridService.GridHeight - 1)
                {
                    targetY = tile2.PosY + 1;
                }
                else
                {
                    targetY = tile1.PosY - 1;
                }
            }
            else
            {
                if (tile2.PosY + 2 < _gridService.GridHeight - 1)
                {
                    targetY = tile2.PosY + 2;
                }
                else if (tile1.PosY - 2 >= 0)
                {
                    targetY = tile1.PosY - 2;
                }
                else
                {
                    if (tile2.PosX > 0)
                    {
                        targetX = tile2.PosX > 0 ? tile2.PosX - 1 : tile2.PosX + 1;
                    }
                    targetY = tile2.PosY + 1;
                }
            }

            TileInstance tile = _gridService[targetX, targetY];
            Debug.Log($"Found a pre match! Set tile at {targetX}, {targetY} to {tile1.TileType}");
            tile.SetTileType(tile1.TileType);

            return tile;
        }

        /// <summary>
        /// Searches for a match around the <paramref name="tile"/>
        /// </summary>
        /// <param name="tile">The tile candidate of the match</param>
        /// <param name="tiles">List of TileObjects to be filled with a match</param>
        public void FillMatchedContiguousTiles(TileInstance tile, List<TileInstance> tiles)
        {
            var foundMatches = false;

            using(ListPool<TileInstance>.Get(out List<TileInstance> foundTiles))
            {
                if(FillHorizontalMatch(tile, foundTiles))
                {
                    AddUniqueOnly(foundTiles, tiles);
                    foundMatches = true;
                }
                foundTiles.Clear();

                if(FillVerticalMatch(tile, foundTiles))
                {
                    AddUniqueOnly(foundTiles, tiles);
                    foundMatches = true;
                }

                if(foundMatches)
                {
                    if(!tiles.Contains(tile))
                    {
                        tiles.Add(tile);
                    }
                }
            }
        }

        void AddUniqueOnly(List<TileInstance> from, List<TileInstance> to)
        {
            foreach(TileInstance tileInstance in from)
            {
                if(!to.Contains(tileInstance))
                {
                    to.Add(tileInstance);
                }
            }
        }

        /// <summary>
        /// Looks left and right to get a Horizontal Match
        /// </summary>
        /// <param name="tile">The tile candidate of the match</param>
        /// <param name="tiles">TileInstances container</param>
        bool FillHorizontalMatch(TileInstance tile, List<TileInstance> tiles)
        {
            // Look right
            for (var i = tile.PosX + 1; i < _gridService.GridWidth; i++)
            {
                TileInstance curTile = _gridService[i, tile.PosY];
                if (curTile.TileType == tile.TileType)
                {
                    tiles.Add(curTile);
                }
                else
                {
                    break;
                }
            }

            // Look left
            for (var i = tile.PosX - 1; i >= 0; i--)
            {
                TileInstance curTile = _gridService[i, tile.PosY];
                if (curTile.TileType == tile.TileType)
                {
                    tiles.Add(curTile);
                }
                else
                {
                    break;
                }
            }

            if(tiles.Count >= 2)
            {
                return true;
            }

            tiles.Clear();

            return false;

        }

        /// <summary>
        /// Looks up and down to get a Vertical Match
        /// </summary>
        /// <param name="tile">The tile candidate of the match</param>
        /// <param name="tiles">TileInstances container</param>
        bool FillVerticalMatch(TileInstance tile, List<TileInstance> tiles)
        {
            // Look Up
            for (var i = tile.PosY + 1; i < _gridService.GridHeight; i++)
            {
                TileInstance curTile = _gridService[tile.PosX, i];
                if (curTile.TileType == tile.TileType)
                {
                    tiles.Add(curTile);
                }
                else
                {
                    break;
                }
            }

            // Look Down
            for (var i = tile.PosY - 1; i >= 0; i--)
            {
                TileInstance curTile = _gridService[tile.PosX, i];
                if (curTile.TileType == tile.TileType)
                {
                    tiles.Add(curTile);
                }
                else
                {
                    break;
                }
            }

            if(tiles.Count >= 2)
            {
                return true;
            }

            tiles.Clear();

            return false;
        }

        void FillHorizontalMatchesPair(TileInstance tile, List<TileInstance> tiles)
        {
            TileInstance curTile = _gridService[tile.PosX + 1, tile.PosY];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }
            curTile = _gridService[tile.PosX + 2, tile.PosY];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }

            curTile = _gridService[tile.PosX - 1, tile.PosY];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);
            }
            curTile = _gridService[tile.PosX - 2, tile.PosY];

            if(curTile == null || (curTile.TileType != tile.TileType))
            {
                return;
            }

            tiles.Add(tile);
            tiles.Add(curTile);
        }

        void FillVerticalMatchesPair(TileInstance tile, List<TileInstance> tiles)
        {
            TileInstance curTile = _gridService[tile.PosX, tile.PosY + 1];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }
            curTile = _gridService[tile.PosX, tile.PosY + 2];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }

            curTile = _gridService[tile.PosX, tile.PosY - 1];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);
            }
            curTile = _gridService[tile.PosX, tile.PosY - 2];

            if(curTile == null || (curTile.TileType != tile.TileType))
            {
                return;
            }

            tiles.Add(tile);
            tiles.Add(curTile);
        }

        public bool TryGetMatches(List<TileInstance> matches)
        {
            for (var x = 0; x < _gridService.GridWidth; x++)
            {
                for (var y = 0; y < _gridService.GridHeight; y++)
                {
                    TileInstance tileInstance = _gridService[x, y];

                    if(tileInstance.TileType == TileType.None)
                    {
                        Debug.LogError($"There should be no active tiles of type {TileType.None}: {tileInstance}");
                    }

                    if(!tileInstance.IsValid)
                    {
                        continue;
                    }

                    FillMatchedContiguousTiles(tileInstance, matches);
                }
            }

            return matches.Count > 0;
        }

    }
}

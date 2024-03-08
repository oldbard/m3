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

                        FillMatchesPair(tile, possibleMatches, true);
                        if(possibleMatches.Count > 1)
                        {
                            TileInstance changedTile = CreatePreMatch(possibleMatches);
                            possibleMatchTiles.Add(changedTile);

                            return;
                        }

                        possibleMatches.Clear();

                        FillMatchesPair(tile, possibleMatches, false);
                        if(possibleMatches.Count > 1)
                        {
                            TileInstance changedTile = CreatePreMatch(possibleMatches);
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
        
        (TileInstance, TileInstance) OrderTilesByCoordinate(TileInstance tile1, TileInstance tile2, int coord1, int coord2)
        {
            return coord1 > coord2 ? (tile2, tile1) : (tile1, tile2);
        }

        TileInstance CreatePreMatch(List<TileInstance> tiles)
        {
            TileInstance tile1 = tiles[0];
            TileInstance tile2 = tiles[1];

            bool horizontal = tile1.PosY == tile2.PosY;
            int length = horizontal ? _gridService.GridWidth : _gridService.GridHeight;

            int targetX;
            int targetY;;

            var coord1 = horizontal ? tile1.PosX : tile1.PosY;
            var coord2 = horizontal ? tile2.PosX : tile2.PosY;

            (tile1, tile2) = OrderTilesByCoordinate(tile1, tile2, coord1, coord2);
			
            coord1 = horizontal ? tile1.PosX : tile1.PosY;
            coord2 = horizontal ? tile2.PosX : tile2.PosY;

            var otherCoord = horizontal ? tile2.PosY : tile2.PosX;

            if(coord2 - coord1 > 1)
            {
                if(coord2 < length - 1)
                {
                    targetX = horizontal ? coord2 + 1 : tile1.PosX;
                    targetY = horizontal ? tile1.PosY : coord2 + 1;
                }
                else
                {
                    targetX = horizontal ? coord2 - 1 : tile1.PosX;
                    targetY = horizontal ? tile1.PosY : coord2 - 1;
                }
            }
            else
            {
                if(coord2 + 2 < length - 1)
                {
                    targetX = horizontal ? coord2 + 2 : tile1.PosX;
                    targetY = horizontal ? tile1.PosY : coord2 + 2;
                }
                else if(coord1 - 2 >= 0)
                {
                    targetX = horizontal ? coord1 - 2 : tile1.PosX;
                    targetY = horizontal ? tile1.PosY : coord1 - 2;
                }
                else
                {
                    if(horizontal)
                    {
                        targetX = coord2 + 1;
                        targetY = otherCoord > 0 ? otherCoord - 1 : otherCoord + 1;
                    }
                    else
                    {
                        targetX = otherCoord > 0 ? otherCoord - 1 : otherCoord + 1;
                        targetY = coord2 + 1;
                    }
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
                if(FillMatch(tile, foundTiles, true))
                {
                    AddUniqueOnly(foundTiles, tiles);
                    foundMatches = true;
                }
                foundTiles.Clear();

                if(FillMatch(tile, foundTiles, false))
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
        /// Looks for a Match
        /// </summary>
        /// <param name="tile">The tile candidate of the match</param>
        /// <param name="tiles">TileInstances container</param>
        /// <param name="horizontal">Whether it should check for horizontal matches or not</param>
        /// <returns>Returns whether it found any match or not</returns>
        bool FillMatch(TileInstance tile, List<TileInstance> tiles, bool horizontal)
        {
            int length = horizontal ? _gridService.GridWidth : _gridService.GridHeight;
            int coord1 = horizontal ? tile.PosX : tile.PosY;
            int coord2 = horizontal ? tile.PosY : tile.PosX;

            // Look right or up
            for (var i = coord1 + 1; i < length; i++)
            {
                if(TryGetMatchTile(tile.TileType, coord2, i, horizontal, out TileInstance curTile))
                {
                    tiles.Add(curTile);
                }
                else
                {
                    break;
                }
            }

            // Look left or down
            for (var i = coord1 - 1; i >= 0; i--)
            {
                if(TryGetMatchTile(tile.TileType, coord2, i, horizontal, out TileInstance curTile))
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

        bool TryGetMatchTile(TileType tileType, int coordinate, int index, bool horizontal, out TileInstance tileInstance)
        {
            int x = horizontal ? index : coordinate;
            int y = horizontal ? coordinate : index;

            tileInstance = _gridService[x, y];
            return tileInstance.TileType == tileType;
        }

        void FillMatchesPair(TileInstance tile, List<TileInstance> tiles, bool horizontal)
        {
            int x = horizontal ? tile.PosX + 1 : tile.PosX;
            int y = horizontal ? tile.PosY : tile.PosY + 1;

            TileInstance curTile = _gridService[x, y];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }
			
            x = horizontal ? tile.PosX + 2 : tile.PosX;
            y = horizontal ? tile.PosY : tile.PosY + 2;
            curTile = _gridService[x, y];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }

            x = horizontal ? tile.PosX - 1 : tile.PosX;
            y = horizontal ? tile.PosY : tile.PosY - 1;
            curTile = _gridService[x, y];
            if (curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);
            }

            x = horizontal ? tile.PosX - 2 : tile.PosX;
            y = horizontal ? tile.PosY : tile.PosY - 2;
            curTile = _gridService[x, y];
            if(curTile?.TileType == tile.TileType)
            {
                tiles.Add(tile);
                tiles.Add(curTile);
            }
        }

        /// <summary>
        /// Tries to get all matches in the grid
        /// </summary>
        /// <param name="matches"></param>
        /// <returns>Returns whether it found any match or not</returns>
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

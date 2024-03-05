using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace OldBard.Services.Match3.Grid
{
    class GridMatchesHelper
    {
        readonly IGridService _gridService;
        readonly List<TileObject> _tilesToUpdateView = new();
        List<TileObject> _listCache = new();
        List<TileObject> _contiguousCache = new();

        readonly int _tilesVariations;
        readonly Random _random;

        internal GridMatchesHelper(IGridService gridService, int variations, int randomSeed)
        {
            _gridService = gridService;

            _tilesVariations = variations;
            _random = new Random(randomSeed);
        }

        /// <summary>
        /// Fills up the grid with tiles
        /// </summary>
        public List<TileObject> ShuffleGrid()
        {
            // Fills up the grid
            List<TileObject> tiles = CreateTiles();

            if (!_gridService.HasPossibleMatch)
            {
                tiles.AddRange(CreatePossibleMatch());
            }

            ShuffleMatches();
            Debug.Log(_gridService.DebugGrid());
            
            return tiles;
        }

        /// <summary>
        /// Fills up the grid with tiles
        /// </summary>
        /// <returns>A List with the TileObjects</returns>
        List<TileObject> CreateTiles()
        {
            _tilesToUpdateView.Clear();

            for (var y = _gridService.GridHeight - 1; y >= 0; y--)
            {
                for (var x = 0; x < _gridService.GridWidth; x++)
                {
                    TileObject tile = _gridService[x, y];

                    // If the TileObject instance was not created yet, does it 
                    if (tile == null)
                    {
                        tile = new TileObject
                        {
                            PosX = x,
                            PosY = y,
                            Valid = true
                        };
                        _gridService.SetTile(tile, x, y);
                    }
                    tile.TileType = (TileType)_random.Next(0, _tilesVariations);

                    _tilesToUpdateView.Add(tile);
                }
            }
            // Makes sure the game does not start already with matches
            ShuffleMatches();
            return _tilesToUpdateView;
        }

        /// <summary>
        /// Goes through the grid making sure that there are no matches already
        /// </summary>
        void ShuffleMatches()
        {
            var needsShuffle = true;

            // TODO: Fix this!
            while (needsShuffle)
            {
                var restartCheck = false;
                needsShuffle = false;
                for (var x = 0; x < _gridService.GridWidth; x++)
                {
                    for (var y = 0; y < _gridService.GridHeight; y++)
                    {
                        TileObject tile = _gridService[x, y];

                        if(tile == null)
                        {
                            continue;
                        }

                        _contiguousCache.Clear();
                        FillMatchedContiguousTiles(tile, ref _contiguousCache);

                        // Found already a match in the grid. Let's randomly set it to another type
                        if(_contiguousCache.Count <= 0)
                        {
                            continue;
                        }

                        needsShuffle = true;
                        foreach(TileObject tileObject in _contiguousCache)
                        {
                            tileObject.TileType = (TileType)_random.Next(0, _tilesVariations);
                        }

                        restartCheck = true;
                        break;
                    }

                    if (restartCheck)
                    {
                        break;
                    }
                }
            }
        }

        public List<TileObject> CreatePossibleMatch()
        {
            _listCache.Clear();

            for (var x = _gridService.GridWidth - 1; x >= 0; x--)
            {
                for (var y = _gridService.GridHeight - 1; y >= 0; y--)
                {
                    TileObject tile = _gridService[x, y];

                    if(tile == null)
                    {
                        continue;
                    }

                    FillHorizontalMatchesPair(tile, ref _listCache);
                    if (_listCache.Count > 1)
                    {
                        var changedTile = CreateHorizontalPreMatch(_listCache);
                        _listCache.Clear();
                        _listCache.Add(changedTile);

                        return _listCache;
                    }

                    FillVerticalMatchesPair(tile, ref _listCache);
                    if (_listCache.Count > 1)
                    {
                        TileObject changedTile = CreateVerticalPreMatch(_listCache);
                        _listCache.Clear();
                        _listCache.Add(changedTile);

                        return _listCache;
                    }
                }
            }

            return CreateHorizontalPreMatch();
        }

        List<TileObject> CreateHorizontalPreMatch()
        {
            _listCache.Clear();

            var targetY = _random.Next(0, _gridService.GridHeight - 1);
            var tileType = (TileType)_random.Next(0, _tilesVariations);

            TileObject tile = _gridService[0, targetY];
            tile.TileType = tileType;
            _listCache.Add(tile);

            tile = _gridService[1, targetY];
            tile.TileType = tileType;
            _listCache.Add(tile);

            tile = _gridService[3, targetY];
            tile.TileType = tileType;
            _listCache.Add(tile);

            UnityEngine.Debug.Log($"Found a pre match! Set tile at {0}, {targetY} to {tile.TileType}");

            return _listCache;
        }

        TileObject CreateHorizontalPreMatch(List<TileObject> tiles)
        {
            TileObject tile1 = tiles[0];
            TileObject tile2 = tiles[1];

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

            TileObject tile = _gridService[targetX, targetY];
            UnityEngine.Debug.Log($"Found a pre match! Set tile at {targetX}, {targetY} to {tile1.TileType}");
            tile.TileType = tile1.TileType;

            return tile;
        }

        TileObject CreateVerticalPreMatch(List<TileObject> tiles)
        {
            TileObject tile1 = tiles[0];
            TileObject tile2 = tiles[1];

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

            TileObject tile = _gridService[targetX, targetY];
            UnityEngine.Debug.Log($"Found a pre match! Set tile at {targetX}, {targetY} to {tile1.TileType}");
            tile.TileType = tile1.TileType;

            return tile;
        }

        /// <summary>
        /// Searches for a match around the <paramref name="tile"/>
        /// </summary>
        /// <param name="tile">The tile candidate of the match</param>
        /// <param name="list">List of TileObjects to be filled with a match</param>
        public void FillMatchedContiguousTiles(TileObject tile, ref List<TileObject> list)
        {
            var foundMatches = false;
            
            List<TileObject> matchList = FillHorizontalMatch(tile);
            list.AddRange(matchList);
            foundMatches = matchList.Count > 0;

            matchList = FillVerticalMatch(tile);
            list.AddRange(matchList);
            foundMatches = foundMatches || matchList.Count > 0;

            if (!foundMatches)
            {
                return;
            }

            list.Add(tile);
        }

        /// <summary>
        /// Looks left and right to get a Horizontal Match
        /// </summary>
        /// <param name="tile">The tile candidate of the match</param>
        /// <returns>A List with the TileObjects</returns>
        List<TileObject> FillHorizontalMatch(TileObject tile)
        {
            _listCache.Clear();

            // Look right
            for (var i = tile.PosX + 1; i < _gridService.GridWidth; i++)
            {
                TileObject curTile = _gridService[i, tile.PosY];
                if (curTile.TileType == tile.TileType)
                {
                    _listCache.Add(curTile);
                }
                else
                {
                    break;
                }
            }
            // Look left
            for (var i = tile.PosX - 1; i >= 0; i--)
            {
                TileObject curTile = _gridService[i, tile.PosY];
                if (curTile.TileType == tile.TileType)
                {
                    _listCache.Add(curTile);
                }
                else
                {
                    break;
                }
            }
            if (_listCache.Count < 2)
            {
                _listCache.Clear();
            }

            return _listCache;
        }

        /// <summary>
        /// Looks up and down to get a Vertical Match
        /// </summary>
        /// <param name="tile">The tile candidate of the match</param>
        /// <returns>A List with the TileObjects</returns>
        List<TileObject> FillVerticalMatch(TileObject tile)
        {
            _listCache.Clear();

            // Look Down
            for (var i = tile.PosY + 1; i < _gridService.GridHeight; i++)
            {
                TileObject curTile = _gridService[tile.PosX, i];
                if (curTile.TileType == tile.TileType)
                {
                    _listCache.Add(curTile);
                }
                else
                {
                    break;
                }
            }
            // Look Up
            for (var i = tile.PosY - 1; i >= 0; i--)
            {
                TileObject curTile = _gridService[tile.PosX, i];
                if (curTile.TileType == tile.TileType)
                {
                    _listCache.Add(curTile);
                }
                else
                {
                    break;
                }
            }
            if (_listCache.Count < 2)
            {
                _listCache.Clear();
            }

            return _listCache;
        }

        void FillHorizontalMatchesPair(TileObject tile, ref List<TileObject> tiles)
        {
            TileObject curTile = _gridService[tile.PosX + 1, tile.PosY];
            if (curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }
            curTile = _gridService[tile.PosX + 2, tile.PosY];
            if (curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }

            curTile = _gridService[tile.PosX - 1, tile.PosY];
            if (curTile != null && (curTile.TileType == tile.TileType))
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

        void FillVerticalMatchesPair(TileObject tile, ref List<TileObject> tiles)
        {
            TileObject curTile = _gridService[tile.PosX, tile.PosY + 1];
            if (curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }
            curTile = _gridService[tile.PosX, tile.PosY + 2];
            if (curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }

            curTile = _gridService[tile.PosX, tile.PosY - 1];
            if (curTile != null && (curTile.TileType == tile.TileType))
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

        public List<TileObject> GetMatches()
        {
            _contiguousCache.Clear();

            for (var x = 0; x < _gridService.GridWidth; x++)
            {
                for (var y = 0; y < _gridService.GridHeight; y++)
                {
                    FillMatchedContiguousTiles(_gridService[x, y], ref _contiguousCache);
                }
            }

            return _contiguousCache;
        }

    }
}

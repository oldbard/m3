using System;
using System.Collections.Generic;
using OldBard.Services.Match3.Grid.Tiles.Data;
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

        readonly (int[,], int[,])[] _preMatchPatterns;

        internal GridMatchesHelper(IGridService gridService, int variations, int randomSeed)
        {
            _gridService = gridService;

            _tilesVariations = variations;
            _random = new Random(randomSeed);
            
            _preMatchPatterns = new (int[,], int[,])[6];
            _preMatchPatterns[0] = ( GridPreMatchConstants.MatchPatternHXXORequiredTiles, GridPreMatchConstants.MatchPatternHXXOMatchTiles );
            _preMatchPatterns[1] = ( GridPreMatchConstants.MatchPatternHXOXRequiredTiles, GridPreMatchConstants.MatchPatternHXOXMatchTiles );
            _preMatchPatterns[2] = ( GridPreMatchConstants.MatchPatternHOXXRequiredTiles, GridPreMatchConstants.MatchPatternHOXXMatchTiles );
            _preMatchPatterns[3] = ( GridPreMatchConstants.MatchPatternVXXORequiredTiles, GridPreMatchConstants.MatchPatternVXXOMatchTiles );
            _preMatchPatterns[4] = ( GridPreMatchConstants.MatchPatternVXOXRequiredTiles, GridPreMatchConstants.MatchPatternVXOXMatchTiles );
            _preMatchPatterns[5] = ( GridPreMatchConstants.MatchPatternVOXXRequiredTiles, GridPreMatchConstants.MatchPatternVOXXMatchTiles );
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

		TileInstance GetTileInstance(int x, int y, IReadOnlyList<TileInstance> tiles)
		{
			if (x < 0 || x >= _gridService.GridWidth || y < 0 || y >= _gridService.GridHeight)
			{
				return null;
			}

			return tiles[x + y * _gridService.GridWidth];
		}

		/// <summary>
		/// Gets the first possible match in the grid
		/// </summary>
		/// <returns>If whether there is a match</returns>
		public bool GetPreMatch(IReadOnlyList<TileInstance> tiles, List<TileInstance> foundMatch)
		{
			for (var x = 0; x < _gridService.GridWidth; x++)
			{
				for (var y = 0; y < _gridService.GridHeight; y++)
				{
					TileInstance tile = GetTileInstance(x, y, tiles);

					foreach((int[,], int[,]) preMatchPattern in _preMatchPatterns)
					{
						if(TryGetPreMatch(tile, tiles, preMatchPattern.Item1, preMatchPattern.Item2, foundMatch))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		bool TryGetPreMatch(TileInstance tile, IReadOnlyList<TileInstance> tiles, int[,] requiredTiles, int[,] matchTiles, List<TileInstance> foundMatch)
		{
			if(!TryGetValidRequiredTiles(tile, tiles, requiredTiles, foundMatch))
			{
				foundMatch.Clear();
				return false;
			}

			int expectedTiles = requiredTiles.GetLength(0);
			if(foundMatch.Count < expectedTiles)
			{
				throw new NullReferenceException($"Missing Required Tiles. Expected {expectedTiles}, got {foundMatch.Count}");
			}

			TileType requiredTileType = foundMatch[0].TileType;

			TileInstance matchTile = GetValidMatchTile(tile, tiles, matchTiles, requiredTileType);

			if(matchTile == null)
			{
				foundMatch.Clear();
				return false;
			}

			foundMatch.Add(matchTile);

			return true;
		}

		bool TryGetValidRequiredTiles(TileInstance tile, IReadOnlyList<TileInstance> tiles, int[,] requiredTiles, List<TileInstance> requiredTilesInstances)
		{
			TileType requiredTileType = TileType.None;

			for(int row = 0; row < requiredTiles.GetLength(0); row++)
			{
				int x = requiredTiles[row, 0];
				int y = requiredTiles[row, 1];

				TileInstance requiredTile = GetTileInstance(tile.PosX + x, tile.PosY + y, tiles);

				if(requiredTile == null)
				{
					return false;
				}

				if(requiredTileType == TileType.None)
				{
					requiredTileType = requiredTile.TileType;
				}

				if(requiredTile.TileType != requiredTileType)
				{
					return false;
				}
					
				requiredTilesInstances.Add(requiredTile);
			}

			return true;
		}

		TileInstance GetValidMatchTile(TileInstance tile, IReadOnlyList<TileInstance> tiles, int[,] matchTiles, TileType requiredTileType)
		{
			for(int row = 0; row < matchTiles.GetLength(0); row++)
			{
				int x = matchTiles[row, 0];
				int y = matchTiles[row, 1];

				TileInstance requiredTile = GetTileInstance(tile.PosX + x, tile.PosY + y, tiles);

				if(requiredTile?.TileType != requiredTileType)
				{
					continue;
				}
					
				return requiredTile;
			}

			return null;
		}
	}
}

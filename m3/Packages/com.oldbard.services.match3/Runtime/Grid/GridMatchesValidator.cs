using System;
using System.Collections.Generic;
using System.Text;

namespace OldBard.Services.Match3.Grid
{
	public class GridMatchesValidator
	{
		readonly int _gridWidth;
		readonly int _gridHeight;

		readonly (int[,], int[,])[] _preMatchPatterns;

		public GridMatchesValidator(int gridWidth, int gridHeight)
		{
			_gridWidth = gridWidth;
			_gridHeight = gridHeight;

			_preMatchPatterns = new (int[,], int[,])[6];
			_preMatchPatterns[0] = ( GridPreMatchConstants.MatchPatternHXXORequiredTiles, GridPreMatchConstants.MatchPatternHXXOMatchTiles );
			_preMatchPatterns[1] = ( GridPreMatchConstants.MatchPatternHXOXRequiredTiles, GridPreMatchConstants.MatchPatternHXOXMatchTiles );
			_preMatchPatterns[2] = ( GridPreMatchConstants.MatchPatternHOXXRequiredTiles, GridPreMatchConstants.MatchPatternHOXXMatchTiles );
			_preMatchPatterns[3] = ( GridPreMatchConstants.MatchPatternVXXORequiredTiles, GridPreMatchConstants.MatchPatternVXXOMatchTiles );
			_preMatchPatterns[4] = ( GridPreMatchConstants.MatchPatternVXOXRequiredTiles, GridPreMatchConstants.MatchPatternVXOXMatchTiles );
			_preMatchPatterns[5] = ( GridPreMatchConstants.MatchPatternVOXXRequiredTiles, GridPreMatchConstants.MatchPatternVOXXMatchTiles );
		}

		TileInstance GetTileInstance(int x, int y, IReadOnlyList<TileInstance> tiles)
		{
			if (x < 0 || x >= _gridWidth || y < 0 || y >= _gridHeight)
			{
				return null;
			}

			return tiles[x + y * _gridWidth];
		}

		/// <summary>
		/// Gets the first possible match in the grid
		/// </summary>
		/// <returns>If whether there is a match</returns>
		public bool GetPreMatch(IReadOnlyList<TileInstance> tiles, List<TileInstance> foundMatch)
		{
			for (var x = 0; x < _gridWidth; x++)
			{
				for (var y = 0; y < _gridHeight; y++)
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

		string DebugList(List<TileObject> foundMatch)
		{
			var sb = new StringBuilder();

			foreach(TileObject tileObject in foundMatch)
			{
				sb.AppendLine(tileObject.ToString());
			}

			return sb.ToString();
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

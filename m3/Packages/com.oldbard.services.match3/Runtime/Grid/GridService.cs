using System.Collections.Generic;
using UnityEngine.Pool;
using Random = System.Random;

namespace OldBard.Services.Match3.Grid
{
    /// <summary>
    /// GridService. Controls the grid logic.
    /// </summary>
    public class GridService : IGridService
    {
        /// Declarations

        /// <summary>
        /// Enumeration of the possible drag directions
        /// </summary>
        public enum DragDirection
        {
            Up,
            Right,
            Down,
            Left
        }

        readonly GridModel _gridModel;
        readonly GridMatchesHelper _gridMatchesHelper;
        readonly GridMatchesValidator _gridMatchesValidator;

        readonly int _tilesVariations;
        readonly Random _random;

        /// Accessors / Properties

        /// <summary>
        /// Settings for the Grid
        /// </summary>
        public GridSettings GridSettings { get; }

        public TileObject this[int x, int y] => _gridModel[x, y];

        public void SetTile(TileObject tile, int x, int y)
        {
            _gridModel[x, y] = tile;
        }

        public int GridWidth => _gridModel.GridWidth;
        public int GridHeight => _gridModel.GridHeight;

        /// <summary>
        /// Checks if there are any possible matches available
        /// </summary>
        public bool HasPossibleMatch
        {
            get
            {
                using(ListPool<TileObject>.Get(out List<TileObject> list))
                {
                    return GetFirstPossibleMatch(list);
                }
            }
        }

        /// Init
        /// <summary>
        /// Creates a GridManager instance
        /// </summary>
        /// <param name="gridSettings">Grid Configuration</param>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="variations">Amount of tiles variations</param>
        /// <param name="randomSeed">Seed for the random calls</param>
        public GridService(GridSettings gridSettings, int width, int height, int variations, int randomSeed)
        {
            GridSettings = gridSettings;

            _gridModel = new GridModel(width, height);
            _gridMatchesHelper = new GridMatchesHelper(this, variations, randomSeed);
            _gridMatchesValidator = new GridMatchesValidator(width, height);

            _tilesVariations = variations;
            _random = new Random(randomSeed);
        }

        public string DebugGrid()
        {
            return _gridModel.DebugGrid();
        }

        /// Populate

        /// <summary>
        /// Fills up the grid with tiles
        /// </summary>
        public List<TileObject> ShuffleGrid()
        {
            return _gridMatchesHelper.ShuffleGrid();
        }

        /// <summary>
        /// Moves the tiles down to occupy the positions left by destroyed tiles
        /// </summary>
        /// <returns>A List with the TileObjects</returns>
        public void MoveTilesDown(List<TileObject> tilesToUpdateView)
        {
            ListPool<TileObject>.Get(out List<TileObject> tiles);
            
            for(var x = 0; x < _gridModel.GridWidth; x++)
            {
                for(var y = 0; y < _gridModel.GridHeight; y++)
                {
                    TileObject tile = _gridModel[x, y];

                    if(tile.Valid)
                    {
                        continue;
                    }

                    // Gets an invalid tile

                    // Gets the first tile above the current one
                    TileObject upTile = _gridModel.GetTileUp(x, y + 1);

                    // if there are none, create it. Else swaps with the invalid one. Sending it up
                    // while bringing down the valid
                    if(upTile == null)
                    {
                        tile.TileType = (TileType) _random.Next(0, _tilesVariations);

                        tiles.Clear();
                        _gridMatchesHelper.FillMatchedContiguousTiles(tile, ref tiles);

                        tile.Valid = true;
                        tilesToUpdateView.Add(tile);
                    }
                    else
                    {
                        _gridModel.SwapTilesPos(tile, upTile);
                        tilesToUpdateView.Add(upTile);
                    }
                }
                
                ListPool<TileObject>.Release(tiles);
            }

            // Makes sure we have possible matches. If not, refresh the grid
            if(!HasPossibleMatch)
            {
                tilesToUpdateView.AddRange(_gridMatchesHelper.CreatePossibleMatch());
            }
        }

        /// GridAccess

        public bool GetFirstPossibleMatch(List<TileObject> tiles)
        {
            return _gridMatchesValidator.GetPreMatch(_gridModel.Tiles, tiles);
        }

        public TileObject GetNeighbourTile(TileObject tile, DragDirection dir)
        {
            return _gridModel.GetNeighbourTile(tile, dir);
        }

        /// Detection

        /// <summary>
        /// Tries to swap the given tiles and find a match
        /// </summary>
        /// <param name="tile1">Tile to be swapped</param>
        /// <param name="tile2">Tile to be swapped</param>
        /// <returns>A List with the TileObjects</returns>
        public bool TrySwapTiles(TileObject tile1, TileObject tile2, List<TileObject> tiles)
        {
            _gridModel.SwapTilesPos(tile1, tile2);

            _gridMatchesHelper.FillMatchedContiguousTiles(tile1, ref tiles);
            _gridMatchesHelper.FillMatchedContiguousTiles(tile2, ref tiles);

            if(tiles.Count > 1)
            {
                ProcessMatches(tiles);
            }
            else
            {
                _gridModel.SwapTilesPos(tile1, tile2);
            }
            
            return tiles.Count > 0;
        }

        /// <summary>
        /// Processes the matches in the given <paramref name="matchedTiles"/>
        /// </summary>
        /// <param name="matchedTiles">A List with the TileObjects</param>
        void ProcessMatches(List<TileObject> matchedTiles)
        {
            foreach (var tileObject in matchedTiles)
            {
                tileObject.Valid = false;
            }
        }
        
        /// <summary>
        /// Finds and processes matches in the grid
        /// </summary>
        /// <returns>A List with the TileObjects</returns>
        public List<TileObject> FindAndProcessMatches()
        {
            List<TileObject> matches = _gridMatchesHelper.GetMatches();

            if(matches.Count > 1)
            {
                ProcessMatches(matches);
            }

            return matches;
        }

        /// <summary>
        /// Cleans up the containers
        /// </summary>
        public void Dispose()
        {
            _gridModel.Dispose();
        }
    }
}
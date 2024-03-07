using System.Collections.Generic;
using OldBard.Services.Match3.Factories;
using UnityEngine;
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

        readonly GridModel _gridModel;
        readonly GridMatchesHelper _gridMatchesHelper;
        readonly GridMatchesValidator _gridMatchesValidator;

        readonly int _tilesVariations;
        readonly Random _random;
        
        readonly Vector3 _gridOffset;

        public Vector3 GridOffset => _gridOffset;

        /// Accessors / Properties

        /// <summary>
        /// Settings for the Grid
        /// </summary>
        public GridConfig GridConfig { get; }

        public TileInstance this[int x, int y] => _gridModel[x, y];
        
        public IReadOnlyList<TileInstance> Tiles => _gridModel.Tiles;

        public int GridWidth => _gridModel.GridWidth;
        public int GridHeight => _gridModel.GridHeight;

        /// <summary>
        /// Checks if there are any possible matches available
        /// </summary>
        bool HasPossibleMatch
        {
            get
            {
                using(ListPool<TileInstance>.Get(out List<TileInstance> list))
                {
                    return GetFirstPossibleMatch(list);
                }
            }
        }

        /// Init
        /// <summary>
        /// Creates a GridManager instance
        /// </summary>
        /// <param name="gridConfig">Grid Configuration</param>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="variations">Amount of tiles variations</param>
        /// <param name="randomSeed">Seed for the random calls</param>
        public GridService(GridConfig gridConfig, int width, int height, int variations, int randomSeed)
        {
            GridConfig = gridConfig;

            _gridModel = new GridModel(width, height);
            _gridMatchesHelper = new GridMatchesHelper(this, variations, randomSeed);
            _gridMatchesValidator = new GridMatchesValidator(width, height);

            _gridOffset = new Vector3(-((GridWidth - 1) * 0.5f), -((GridHeight - 1) * 0.5f), 0f);
            _tilesVariations = variations;
            _random = new Random(randomSeed);

            CreateTiles();
        }

        public string DebugGrid()
        {
            return _gridModel.DebugGrid();
        }

        /// Populate

        /// <summary>
        /// Fills up the grid with tiles
        /// </summary>
        void CreateTiles()
        {
            for (var y = GridHeight - 1; y >= 0; y--)
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    GetNewTile(x, y);
                }
            }
            
            // Makes sure the game does not start already with matches
            RemoveMatches();
        }

        TileInstance GetNewTile(int x, int y)
        {
            TileInstance tileInstance = TileInstanceFactory.Instance.GetNewTile(x, y, _gridOffset);

            SetNewTile(tileInstance, x, y);

            return tileInstance;
        }

        void RemoveMatches()
        {
            using(ListPool<TileInstance>.Get(out List<TileInstance> matches))
            {
                while(_gridMatchesHelper.TryGetMatches(matches))
                {
                    ShuffleMatches(matches);

                    matches.Clear();
                }
            }
        }

        void SetNewTile(TileInstance tile, int x, int y)
        {
            tile.SetTileType((TileType)_random.Next(0, _tilesVariations));
            SetTile(tile, x, y);
        }

        /// <summary>
        /// Sets the tile Grid Position
        /// </summary>
        /// <param name="tile">Tile Instance being set</param>
        /// <param name="x">X Position</param>
        /// <param name="y">Y Position</param>
        public void SetTile(TileInstance tile, int x, int y)
        {
            _gridModel[x, y] = tile;
        }

        /// <summary>
        /// Goes through the grid making sure that there are no matches already
        /// </summary>
        void ShuffleMatches(List<TileInstance> matches)
        {
            foreach(TileInstance tileInstance in matches)
            {
                tileInstance.SetTileType((TileType) _random.Next(0, _tilesVariations));
            }
        }

        /// <summary>
        /// Moves the tiles down to occupy the positions left by destroyed tiles
        /// </summary>
        public void MoveTilesDown(List<TileInstance> tilesToUpdateView)
        {
            for(var x = 0; x < _gridModel.GridWidth; x++)
            {
                for(var y = 0; y < _gridModel.GridHeight; y++)
                {
                    TileInstance tile = _gridModel[x, y];

                    // Tries to find an invalid Tile. Invalid tiles are tiles which are about to be removed because they were matched.
                    if(tile.IsValid)
                    {
                        continue;
                    }
                    
                    // Gets the first tile above the current one
                    TileInstance upTile = _gridModel.GetTileUp(x, y + 1);

                    // if there are none, create it. Else swaps with the invalid one. Sending it up
                    // while bringing down the valid
                    if(upTile == null)
                    {
                        upTile = GetNewTile(x, y);
                    }
                    else
                    {
                        // Inverts the view position so it can be properly dragged down
                        upTile.TileView.TargetPosition = tile.TileView.Position;
                        tile.TileView.Position = upTile.TileView.Position;
                        _gridModel.SwapTilesPos(tile, upTile);
                    }

                    tilesToUpdateView.Add(upTile);
                }
            }

            // Makes sure we have possible matches. If not, refresh the grid
            if(!HasPossibleMatch)
            {
                _gridMatchesHelper.CreatePossibleMatch(tilesToUpdateView);
            }
        }
        
        /// GridAccess
        
        /// <summary>
        /// Gets the first possible match in the Grid
        /// </summary>
        /// <param name="tiles">A List with the found TileObjects</param>
        /// <returns>True if there is a possible match</returns>
        public bool GetFirstPossibleMatch(List<TileInstance> tiles)
        {
            return _gridMatchesValidator.GetPreMatch(Tiles, tiles);
        }

        /// <summary>
        /// Gets the TileInstance at the dragged direction
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public TileInstance GetNeighbourTile(TileInstance tile, DragDirection dir)
        {
            return _gridModel.GetNeighbourTile(tile, dir);
        }

        /// Detection
        /// <summary>
        /// Tries to swap the given tiles and find a match
        /// </summary>
        /// <param name="tile1">Tile to be swapped</param>
        /// <param name="tile2">Tile to be swapped</param>
        /// <param name="tiles">A List with the found TileObjects</param>
        /// <returns>Return whether the swap resulted in a match</returns>
        public bool TrySwapTiles(TileInstance tile1, TileInstance tile2, List<TileInstance> tiles)
        {
            _gridModel.SwapTilesPos(tile1, tile2);

            _gridMatchesHelper.FillMatchedContiguousTiles(tile1, tiles);
            _gridMatchesHelper.FillMatchedContiguousTiles(tile2, tiles);

            if(tiles.Count > 1)
            {
                InvalidateTiles(tiles);
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
        /// <param name="matchedTiles">A List with the found TileObjects</param>
        public void ReleaseTiles(List<TileInstance> matchedTiles)
        {
            foreach (TileInstance tileInstance in matchedTiles)
            {
                tileInstance.IsValid = false;
                TileInstanceFactory.Instance.ReleaseTileInstance(tileInstance);
            }
        }

        /// <summary>
        /// Processes the matches in the given <paramref name="matchedTiles"/>
        /// </summary>
        /// <param name="matchedTiles">A List with the TileObjects</param>
        public void InvalidateTiles(List<TileInstance> matchedTiles)
        {
            foreach (TileInstance tileInstance in matchedTiles)
            {
                tileInstance.IsValid = false;
            }
        }
        
        /// <summary>
        /// Finds and processes matches in the grid
        /// </summary>
        /// <param name="matches">List of matched tiles</param>
        /// <returns>True if there are matches</returns>
        public bool TryFindAndProcessMatches(List<TileInstance> matches)
        {
            return _gridMatchesHelper.TryGetMatches(matches);
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
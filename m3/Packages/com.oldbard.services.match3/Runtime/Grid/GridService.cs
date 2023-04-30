using System;
using System.Collections.Generic;
using Random = System.Random;

namespace OldBard.Services.Match3.Grid
{
    /// <summary>
    /// GridService. Controls the grid logic.
    /// </summary>
    public class GridService : IDisposable
    {
        #region Declarations

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

        /// <summary>
        /// Enumeration of the possible match types
        /// </summary>
        enum PreMatchType
        {
            None,
            HXXO,
            HXOX,
            HOXX,
            VXXO,
            VXOX,
            VOXX,
        }

        /// <summary>
        /// Array with all the tiles in the battle
        /// </summary>
        TileObject[] _tiles;
        
        // List of TileObjects being use as caches for the iterations.
        List<TileObject> _tilesToUpdateView;
        List<TileObject> _contiguousCache;
        List<TileObject> _listCache;

        Random _random = null;

        public readonly int GridWidth;
        public readonly int GridHeight;
        readonly int _tilesVariations;

        #endregion

        #region Accessors / Properties

        /// <summary>
        /// Settings for the Grid
        /// </summary>
        public GridSettings GridSettings;

        /// <summary>
        /// Checks if there are any possible matches available
        /// </summary>
        bool HasPossibleMatch
        {
            get
            {
                var list = GetFirstPossibleMatch();

                var hasMatches = list != null && list.Count > 0;

                return hasMatches;
            }
        }

        /// <summary>
        /// Returns / Sets the Tile for the given coordinates
        /// </summary>
        /// <param name="x">Grid Column</param>
        /// <param name="y">Grid Row</param>
        TileObject this[int x, int y]
        {
            get
            {
                if(x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
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

        #endregion

        #region Init

        /// <summary>
        /// Creates a GridManager instance
        /// </summary>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="variations">Amount of tiles variations</param>
        public GridService(GridSettings gridSettings, int width, int height, int variations, int randomSeed)
        {
            GridSettings = gridSettings;

            GridWidth = width;
            GridHeight = height;
            _tilesVariations = variations;

            _random = new Random(randomSeed);

            // Initializes the containers
            var maxTiles = GridWidth * GridHeight;
            _tiles = new TileObject[maxTiles];
            _tilesToUpdateView = new List<TileObject>(maxTiles);
            _contiguousCache = new List<TileObject>(maxTiles);
            _listCache = new List<TileObject>(maxTiles);
        }

        #endregion
        
        #region Populate
        
        /// <summary>
        /// Fills up the grid with tiles
        /// </summary>
        public List<TileObject> ShuffleGrid()
        {
            // Fills up the grid
            var tiles = CreateTiles();

            if(!HasPossibleMatch)
            {
                tiles.AddRange(CreatePossibleMatch());
            }


            ShuffleMatches();

            return tiles;
        }

        public string DebugGrid()
        {
            var sb = new System.Text.StringBuilder();

            for (var y = 0; y < GridHeight; y++)
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    sb.Append(this[x, y].TileType.ToString() + ", ");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Fills up the grid with tiles
        /// </summary>
        /// <returns>A List with the TileObjects</returns>
        List<TileObject> CreateTiles()
        {
            _tilesToUpdateView.Clear();

            for(var y = GridHeight - 1; y >= 0; y--)
            {
                for(var x = 0; x < GridWidth; x++)
                {
                    var tile = this[x, y];

                    // If the TileObject instance was not created yet, does it 
                    if(tile == null)
                    {
                        tile = new TileObject
                        {
                            PosX = x,
                            PosY = y,
                            Valid = true
                        };
                        this[x, y] = tile;
                    }
                    tile.TileType = (TileType) _random.Next(0, _tilesVariations);

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

            while(needsShuffle)
            {
                var restartCheck = false;
                needsShuffle = false;
                for(var x = 0; x < GridWidth; x++)
                {
                    for(var y = 0; y < GridHeight; y++)
                    {
                        var tile = this[x, y];
                        if(tile != null)
                        {
                            _contiguousCache.Clear();
                            FillMatchedContiguousTiles(tile, ref _contiguousCache);
                            
                            // Found already a match in the grid. Let's randomly set it to another type
                            if(_contiguousCache.Count > 0)
                            {
                                needsShuffle = true;
                                for(var i = 0; i < _contiguousCache.Count; i++)
                                {
                                    _contiguousCache[i].TileType =
                                        (TileType)_random.Next(0, _tilesVariations);
                                }

                                restartCheck = true;
                                break;
                            }
                        }
                    }

                    if(restartCheck)
                    {
                        break;
                    }
                }
            }
        }

        List<TileObject> CreatePossibleMatch()
        {
            _listCache.Clear();

            for(var x = GridWidth - 1; x >= 0; x--)
            {
                for(var y = GridHeight - 1; y >= 0; y--)
                {
                    var tile = this[x, y];

                    if(tile != null)
                    {
                        FillHorizontalMatchesPair(tile, ref _listCache);
                        if(_listCache.Count > 1)
                        {
                            var changedTile = CreateHorizontalPreMatch(_listCache);
                            _listCache.Clear();
                            _listCache.Add(changedTile);

                            return _listCache;
                        }

                        FillVerticalMatchesPair(tile, ref _listCache);
                        if(_listCache.Count > 1)
                        {
                            var changedTile = CreateVerticalPreMatch(_listCache);
                            _listCache.Clear();
                            _listCache.Add(changedTile);

                            return _listCache;
                        }
                    }
                }
            }

            return CreateHorizontalPreMatch();
        }


        List<TileObject> CreateHorizontalPreMatch()
        {
            _listCache.Clear();

            var targetY = _random.Next(0, GridHeight - 1);
            var tileType = (TileType)_random.Next(0, _tilesVariations);

            var tile = this[0, targetY];
            tile.TileType = tileType;
            _listCache.Add(tile);

            tile = this[1, targetY];
            tile.TileType = tileType;
            _listCache.Add(tile);

            tile = this[3, targetY];
            tile.TileType = tileType;
            _listCache.Add(tile);

            UnityEngine.Debug.Log($"Found a pre match! Set tile at {0}, {targetY} to {tile.TileType}");

            return _listCache;
        }

        TileObject CreateHorizontalPreMatch(List<TileObject> tiles)
        {
            var tile1 = tiles[0];
            var tile2 = tiles[1];

            var targetX = 0;
            var targetY = tile1.PosY;

            if (tile1.PosX > tile2.PosX)
            {
                var tempTile = tile2;
                tile2 = tile1;
                tile1 = tempTile;
            }

            if (tile2.PosX - tile1.PosX > 1)
            {
                if (tile2.PosX < GridWidth - 1)
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
                if (tile2.PosX + 2 < GridWidth - 1)
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

            var tile = this[targetX, targetY];
            UnityEngine.Debug.Log($"Found a pre match! Set tile at {targetX}, {targetY} to {tile1.TileType}");
            tile.TileType = tile1.TileType;

            return tile;
        }

        TileObject CreateVerticalPreMatch(List<TileObject> tiles)
        {
            var tile1 = tiles[0];
            var tile2 = tiles[1];

            var targetX = tile1.PosX;
            var targetY = 0;

            if (tile1.PosY > tile2.PosY)
            {
                var tempTile = tile2;
                tile2 = tile1;
                tile1 = tempTile;
            }

            if(tile2.PosY - tile1.PosY > 1)
            {
                if (tile2.PosY < GridHeight - 1)
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
                if (tile2.PosY + 2 < GridHeight - 1)
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

            var tile = this[targetX, targetY];
            UnityEngine.Debug.Log($"Found a pre match! Set tile at {targetX}, {targetY} to {tile1.TileType}");
            tile.TileType = tile1.TileType;


            return tile;
        }

        /// <summary>
        /// Moves the tiles down to occupy the positions left by destroyed tiles
        /// </summary>
        /// <returns>A List with the TileObjects</returns>
        public List<TileObject> MoveTilesDown()
        {
            _tilesToUpdateView.Clear();
            
            for(var x = 0; x < GridWidth; x++)
            {
                for(var y = 0; y < GridHeight; y++)
                {
                    var tile = this[x, y];
                    if(tile.Valid) continue;
                    // Gets an invalid tile

                    // Gets the first tile above the current one
                    var upTile = GetTileUp(x, y + 1);

                    // if there are none, create it. Else swaps with the invalid one. Sending it up
                    // while bringing down the valid
                    if(upTile == null)
                    {
                        tile.TileType = (TileType)_random.Next(0, _tilesVariations);

                        _contiguousCache.Clear();
                        FillMatchedContiguousTiles(tile, ref _contiguousCache);

                        tile.Valid = true;
                        _tilesToUpdateView.Add(tile);
                    }
                    else
                    {
                        SwapTilesPos(tile, upTile);
                        _tilesToUpdateView.Add(upTile);
                    }
                }
            }

            // Makes sure we have possible matches. If not, refresh the grid
            if(!HasPossibleMatch)
            {
                _tilesToUpdateView.AddRange(CreatePossibleMatch());
            }

            return _tilesToUpdateView;
        }

        #endregion

        #region GridAccess

        /// <summary>
        /// Looks up for a tile in the given column <paramref name="x"/> starting by a
        /// given row <paramref name="fromY"/>
        /// </summary>
        /// <param name="x">Column</param>
        /// <param name="fromY">Initial row</param>
        /// <returns>The first tile above the given coordinates</returns>
        TileObject GetTileUp(int x, int fromY)
        {
            for(var y = fromY; y < GridHeight; y++)
            {
                var tile = this[x, y];
                if (tile == null) return null;

                if(tile.Valid)
                {
                    return tile;
                }
            }

            return null;
        }

        #endregion

        #region Detection

        /// <summary>
        /// Tries to swap the given tiles and find a match
        /// </summary>
        /// <param name="tile1">Tile to be swapped</param>
        /// <param name="tile2">Tile to be swapped</param>
        /// <returns>A List with the TileObjects</returns>
        public List<TileObject> TrySwapTiles(TileObject tile1, TileObject tile2)
        {
            SwapTilesPos(tile1, tile2);

            _contiguousCache.Clear();
            FillMatchedContiguousTiles(tile1, ref _contiguousCache);
            FillMatchedContiguousTiles(tile2, ref _contiguousCache);

            if(_contiguousCache.Count > 1)
            {
                ProcessMatches(_contiguousCache);
            }
            else
            {
                SwapTilesPos(tile1, tile2);
            }
            
            return _contiguousCache;
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
            _contiguousCache.Clear();

            for(var x = 0; x < GridWidth; x++)
            {
                for (var y = 0; y < GridHeight; y++)
                {
                    FillMatchedContiguousTiles(this[x, y], ref _contiguousCache);
                }
            }

            if(_contiguousCache.Count > 1)
            {
                ProcessMatches(_contiguousCache);
            }

            return _contiguousCache;
        }

        /// <summary>
        /// Searches for a match around the <paramref name="tile"/>
        /// </summary>
        /// <param name="tile">The tile candidate of the match</param>
        /// <param name="list">List of TileObjects to be filled with a match</param>
        void FillMatchedContiguousTiles(TileObject tile, ref List<TileObject> list)
        {
            var foundMatches = false;
            var matchList = FillHorizontalMatch(tile);
            list.AddRange(matchList);
            foundMatches = matchList.Count > 0;

            matchList = FillVerticalMatch(tile);
            list.AddRange(matchList);
            foundMatches = foundMatches || matchList.Count > 0;

            if(!foundMatches) return;
            
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
            for(var i = tile.PosX + 1; i < GridWidth; i++)
            {
                var curTile = this[i, tile.PosY];
                if(curTile.TileType == tile.TileType)
                {
                    _listCache.Add(curTile);
                }
                else
                {
                    break;
                }
            }
            // Look left
            for(var i = tile.PosX - 1; i >= 0; i--)
            {
                var curTile = this[i, tile.PosY];
                if(curTile.TileType == tile.TileType)
                {
                    _listCache.Add(curTile);
                }
                else
                {
                    break;
                }
            }
            if(_listCache.Count < 2)
            {
                _listCache.Clear();
            }

            return _listCache;
        }

        void FillHorizontalMatchesPair(TileObject tile, ref List<TileObject> tiles)
        {
            var curTile = this[tile.PosX + 1, tile.PosY];
            if(curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }
            curTile = this[tile.PosX + 2, tile.PosY];
            if(curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }

            curTile = this[tile.PosX - 1, tile.PosY];
            if(curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);
            }
            curTile = this[tile.PosX - 2, tile.PosY];
            if(curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }
        }

        void FillVerticalMatchesPair(TileObject tile, ref List<TileObject> tiles)
        {
            var curTile = this[tile.PosX, tile.PosY + 1];
            if(curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }
            curTile = this[tile.PosX, tile.PosY + 2];
            if(curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);

                return;
            }

            curTile = this[tile.PosX, tile.PosY - 1];
            if(curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);
            }
            curTile = this[tile.PosX, tile.PosY - 2];
            if(curTile != null && (curTile.TileType == tile.TileType))
            {
                tiles.Add(tile);
                tiles.Add(curTile);
            }
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
            for(var i = tile.PosY + 1; i < GridHeight; i++)
            {
                var curTile = this[tile.PosX, i];
                if(curTile.TileType == tile.TileType)
                {
                    _listCache.Add(curTile);
                }
                else
                {
                    break;
                }
            }
            // Look Up
            for(var i = tile.PosY - 1; i >= 0; i--)
            {
                var curTile = this[tile.PosX, i];
                if(curTile.TileType == tile.TileType)
                {
                    _listCache.Add(curTile);
                }
                else
                {
                    break;
                }
            }
            if(_listCache.Count < 2)
            {
                _listCache.Clear();
            }

            return _listCache;
        }

        /// <summary>
        /// Gets the pre match type out of 3 candidate tiles
        /// </summary>
        /// <param name="tile1">Tile of the pre match</param>
        /// <param name="tile2">Tile of the pre match</param>
        /// <param name="tile3">Tile of the pre match</param>
        /// <param name="tile">Initial tile of the possible pre match</param>
        /// <returns>The PreMatchType detected</returns>
        PreMatchType GetPreMatchType(TileObject tile1, TileObject tile2, TileObject tile3, ref TileObject tile)
        {
            if(tile1 == null || tile2 == null || tile3 == null) return PreMatchType.None;
            
            // Possible Horizontals
            if(tile1.PosY == tile2.PosY)
            {
                // Possible HXXO
                if(tile1.TileType == tile2.TileType)
                {
                    tile = this[tile2.PosX + 1, tile2.PosY - 1];
                    if (tile?.TileType == tile1.TileType)
                    {
                        return PreMatchType.HXXO;
                    }

                    tile = this[tile2.PosX + 1, tile2.PosY + 1];
                    if (tile?.TileType == tile1.TileType)
                    {
                        return PreMatchType.HXXO;
                    }
                }

                // Possible HXOX
                if(tile1.TileType == tile3.TileType)
                {
                    tile = this[tile1.PosX + 1, tile1.PosY - 1];
                    if(tile?.TileType == tile1.TileType)
                    {
                        return PreMatchType.HXOX;
                    }
                    
                    tile = this[tile1.PosX + 1, tile1.PosY + 1];
                    if(tile?.TileType == tile1.TileType)
                    {
                        return PreMatchType.HXOX;
                    }                    
                }


                // Possible HOXX
                if(tile2.TileType == tile3.TileType)
                {
                    tile = this[tile2.PosX - 1, tile2.PosY - 1];
                    if(tile?.TileType == tile2.TileType)
                    {
                        return PreMatchType.HOXX;
                    }
                    
                    tile = this[tile2.PosX - 1, tile2.PosY + 1];
                    if(tile?.TileType == tile2.TileType)
                    {
                        return PreMatchType.HOXX;
                    }
                }
            }

            // Possible Verticals
            if(tile1.PosX == tile2.PosX)
            {
                // Possible VXXO
                if(tile1.TileType == tile2.TileType)
                {
                    tile = this[tile2.PosX - 1, tile2.PosY + 1];
                    if(tile?.TileType == tile1.TileType)
                    {
                        return PreMatchType.VXXO;
                    }

                    tile = this[tile2.PosX + 1, tile2.PosY + 1];
                    if(tile?.TileType == tile1.TileType)
                    {
                        return PreMatchType.VXXO;
                    }
                }

                // Possible VXOX
                if(tile1.TileType == tile3.TileType)
                {
                    tile = this[tile1.PosX - 1, tile1.PosY + 1];
                    if(tile?.TileType == tile1.TileType)
                    {
                        return PreMatchType.VXOX;
                    }
                    
                    tile = this[tile1.PosX + 1, tile1.PosY + 1];
                    if(tile?.TileType == tile1.TileType)
                    {
                        return PreMatchType.VXOX;
                    }                    
                }

                // Possible VOXX
                if(tile2.TileType == tile3.TileType)
                {
                    tile = this[tile2.PosX - 1, tile2.PosY - 1];
                    if(tile?.TileType == tile2.TileType)
                    {
                        return PreMatchType.VOXX;
                    }
                    
                    tile = this[tile2.PosX + 1, tile2.PosY - 1];
                    if(tile?.TileType == tile2.TileType)
                    {
                        return PreMatchType.VOXX;
                    }
                }
            }

            tile = null;
            return PreMatchType.None;
        }

        /// <summary>
        /// Gets the first possible match in the grid
        /// </summary>
        /// <returns>A List with the TileObjects</returns>
        public List<TileObject> GetFirstPossibleMatch()
        {
            _contiguousCache.Clear();
            TileObject tile = null;
            TileObject tile1 = null, tile2 = null, tile3 = null;
            var preType = PreMatchType.None;

            for(var x = 0; x < GridWidth; x++)
            {
                for(var y = 0; y < GridHeight; y++)
                {
                    // Searches for Horizontal
                    tile1 = this[x, y];
                    tile2 = this[x + 1, y];
                    tile3 = this[x + 2, y];
                    
                    preType = GetPreMatchType(tile1, tile2, tile3, ref tile);

                    ParsePossibleMatch(preType, tile1, tile2, tile3, tile, ref _contiguousCache);
                    if(_contiguousCache.Count > 0) break;

                    // Searches for Vertical
                    tile1 = this[x, y];
                    tile2 = this[x, y + 1];
                    tile3 = this[x, y + 2];

                    preType = GetPreMatchType(tile1, tile2, tile3, ref tile);

                    ParsePossibleMatch(preType, tile1, tile2, tile3, tile, ref _contiguousCache);
                    if(_contiguousCache.Count > 0) break;
                }
                if(_contiguousCache.Count > 0) break;
            }

            return _contiguousCache;
        }

        /// <summary>
        /// Parses the possible match and fills the <paramref name="list"/> with the match data
        /// </summary>
        /// <param name="preMatchType">Type of pre match detected</param>
        /// <param name="tile1">Tile of the pre match</param>
        /// <param name="tile2">Tile of the pre match</param>
        /// <param name="tile3">Tile of the pre match</param>
        /// <param name="tile">The initial tile used to check for the pre match</param>
        /// <param name="list">List of TileObjects of the pre match</param>
        void ParsePossibleMatch(PreMatchType preMatchType, TileObject tile1, TileObject tile2, TileObject tile3,
            TileObject tile, ref List<TileObject> list)
        {
            // Not a pre match
            if(preMatchType == PreMatchType.None || tile == null) return;

            list.Add(tile);
            switch(preMatchType)
            {
                case PreMatchType.HXXO:
                case PreMatchType.VXXO:
                    list.Add(tile1);
                    list.Add(tile2);
                    break;
                case PreMatchType.HXOX:
                case PreMatchType.VXOX:
                    list.Add(tile1);
                    list.Add(tile3);
                    break;
                case PreMatchType.HOXX:
                case PreMatchType.VOXX:
                    list.Add(tile2);
                    list.Add(tile3);
                    break;
            }
        }

        /// <summary>
        /// Swaps the positions of the given tiles
        /// </summary>
        /// <param name="tile1">Tile to swap position</param>
        /// <param name="tile2">Tile to swap position</param>
        void SwapTilesPos(TileObject tile1, TileObject tile2)
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
        public TileObject GetNeighbourTile(TileObject tile, DragDirection dir)
        {
            switch (dir)
            {
                case DragDirection.Up:
                    return tile.PosY > 0 ?
                        this[tile.PosX, tile.PosY - 1] : null;
                case DragDirection.Right:
                    return tile.PosX < GridWidth - 1 ?
                        this[tile.PosX + 1, tile.PosY] : null;
                case DragDirection.Down:
                    return tile.PosY < GridHeight ?
                        this[tile.PosX, tile.PosY + 1] : null;
                case DragDirection.Left:
                    return tile.PosX > 0 ?
                        this[tile.PosX - 1, tile.PosY] : null;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Cleans up the containers
        /// </summary>
        public void Dispose()
        {
            _tilesToUpdateView.Clear();
            _tilesToUpdateView = null;
            
            _contiguousCache.Clear();
            _contiguousCache = null;
            
            _listCache.Clear();
            _listCache = null;

            _tiles = null;
        }
    }
}
﻿using System.Collections.Generic;
using System.Threading.Tasks;
using GameData;
using Gameplay.Animations;
using Shared;
using UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Gameplay.Views
{
    /// <summary>
    /// GridViewController
    /// </summary>
    
    // This is a partial class. It has the animation methods separated to
    // make it easier to support
    public class GridViewController : MonoBehaviour
    {
        #region Declarations

        const int TileBackgroundZPos = -1;
        const int TileZPos = -2;

        /// <summary>
        /// Class with information about the tile view
        /// </summary>
        public class TileObjectView
        {
            public TileView TileView;
            public TileObject TileObject;
            public bool Spawned;
        }

        [SerializeField] Transform _tilesParent;
        [SerializeField] Transform _boardFrame;
        [SerializeField] SpriteRenderer _tilesBackground;
        [SerializeField] BoxCollider2D _tilesBackgroundCollider;

        Config _config;
        GridManager _gridManager;
        AnimationsController _animationsController;
        Camera _camera;

        /// <summary>
        /// A HashSet with all the tiles in the view
        /// </summary>
        HashSet<TileObjectView> _tiles;
        
        // A list used as cache for the required iterations 
        List<TileObjectView> _tilesBeingAnimated;

        int _variation;

        int _yCascadePositionOffset;
        
        #endregion

        #region Accessors

        /// <summary>
        /// Gets a tile given it's coordinates
        /// </summary>
        /// <param name="x">The Tile Column</param>
        /// <param name="y">The Tile Row</param>
        private TileObjectView this[int x, int y] => GetTileAt(x, y);

        #endregion

        #region Init

        /// <summary>
        /// Initializes the Grid View
        /// </summary>
        /// <param name="config">The Game Config</param>
        /// <param name="gridManager">The Grid Manager</param>
        public void Initialize(Config config, GridManager gridManager, AnimationsController animationsController)
        {
            _config = config;
            _gridManager = gridManager;
            _animationsController = animationsController;
            _camera = Camera.main;
            
            // Moves the camera and the board to the proper positions depending on the grid size
            _camera.transform.position = new Vector3((_gridManager.GridWidth * 0.5f) - 4f,
                _gridManager.GridHeight * 0.5f - 0.5f, 0f);

            _boardFrame.localPosition = new Vector3(_gridManager.GridWidth * 0.5f - 0.5f,
                _gridManager.GridHeight * 0.5f - 0.5f, 0f);

            _tilesBackground.transform.localPosition = new Vector3(_gridManager.GridWidth * 0.5f - 0.5f,
                _gridManager.GridHeight * 0.5f - 0.5f, 0f);
            _tilesBackground.size = new Vector2(_gridManager.GridWidth, _gridManager.GridHeight);
            _tilesBackgroundCollider.size = _tilesBackground.size;

            // Inits the containers
            var maxTiles = _gridManager.GridWidth * _gridManager.GridHeight;
            _tiles = new HashSet<TileObjectView>();
            _tilesBeingAnimated = new List<TileObjectView>(maxTiles);

            // Inits the specific variation for this play through and the position from which
            // the tiles should come
            _variation = Random.Range(0, _config.TilesVariations);

            _yCascadePositionOffset = _gridManager.GridHeight + 10;

            // Creates the View Tiles
            InitializeGridView(_gridManager.GridWidth, _gridManager.GridHeight);
        }

        /// <summary>
        /// Initializes the tiles background and view
        /// </summary>
        /// <param name="width">Grid Width</param>
        /// <param name="height">Grid Height</param>
        void InitializeGridView(int width, int height)
        {
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    // Creates the Tiles Views
                    var tile = Instantiate(_config.TilePrefab, _tilesParent);

                    var tileView = new TileObjectView
                    {
                        TileView = tile,
                        TileObject = null,
                        Spawned = false
                    };
                    _tiles.Add(tileView);
                }
            }
        }

        /// <summary>
        /// Unity's On Destroy event. We use it to clean up the containers and objects we created.
        /// </summary>
        void OnDestroy()
        {
            _tilesBeingAnimated.Clear();
            _tilesBeingAnimated = null;

            foreach(var tile in _tiles)
            {
                if (tile != null)
                {
                    Destroy(tile.TileView.gameObject);
                }
            }

            _tiles.Clear();
            _tiles = null;

            _gridManager = null;
        }

        #endregion

        #region Populate
        
        /// <summary>
        /// Creates a TileObjectView based on it's object counterpart
        /// </summary>
        /// <param name="tileObject">TileObject reference data</param>
        /// <returns>The TileObjectView</returns>
        TileObjectView CreateTile(TileObject tileObject)
        {
            // Virtually spawns a view tile
            var tileView = SpawnTile(tileObject);

            // We place the tile higher in the screen to move it down and have a cascade effect.
            SetTilePos(tileObject.PosX,
                tileObject.PosY + _yCascadePositionOffset,
                TileZPos, tileView.TileView.gameObject);
            return tileView;
        }

        /// <summary>
        /// Virtually Spawns a TileObjectView
        /// </summary>
        /// <param name="tile">TileObject reference data</param>
        /// <returns>The TileObjectView</returns>
        TileObjectView SpawnTile(TileObject tile)
        {
            // Go through the list to find the specific TileObject reference
            foreach(var curTile in _tiles)
            {
                if (curTile.Spawned == false)
                {
                    curTile.TileObject = tile;

                    // Gets and sets the tile based on the view data in the config
                    var viewData = _config.GetViewData(tile.TileType, _variation);
                    curTile.TileView.Init(tile, viewData);

                    // Enables the tile and tags it as spawned
                    curTile.TileView.gameObject.SetActive(true);
                    curTile.Spawned = true;

                    return curTile;
                }
            }

            return null;
        }

        /// <summary>
        /// Disables a tile and sets it as not spawned
        /// </summary>
        /// <param name="tile">Tile do despawn</param>
        void DeSpawnTile(TileObjectView tile)
        {
            tile.Spawned = false;
            tile.TileView.gameObject.SetActive(false);
        }

        #endregion
        
        #region Access

        /// <summary>
        /// Returns the TileObjectView correspondent to the TileObject
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>The TileObjectView</returns>
        TileObjectView GetTileAt(TileObject tile)
        {
            foreach(var curTile in _tiles)
            {
                if(curTile.Spawned && curTile.TileObject == tile)
                {
                    return curTile;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the TileObjectView at the specific coordinates.
        /// </summary>
        /// <param name="x">Tile Column</param>
        /// <param name="y">Tile Row</param>
        /// <returns>The TileObjectView</returns>
        TileObjectView GetTileAt(int x, int y)
        {
            foreach(var curTile in _tiles)
            {
                if(curTile.TileObject.PosX == x && curTile.TileObject.PosY == y)
                {
                    return curTile;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the TileObject at the specific screen position
        /// </summary>
        /// <param name="pos">The screen position</param>
        /// <returns>The TileObject</returns>
        public TileObject GetTileAt(Vector3 pos)
        {
            // Converts the screen position into a world position
            var worldPos = _camera.ScreenToWorldPoint(pos);
            
            // Calculates the column and row based on the given position and tiles sizes
            var x = Mathf.RoundToInt(worldPos.x / _config.TileViewWidth);
            var y = Mathf.RoundToInt(worldPos.y / _config.TileViewHeight);
            return this[x, y]?.TileObject;
        }

        /// <summary>
        /// Gets the world position of the given TileObject
        /// </summary>
        /// <param name="tileObject">TileObject to get the world position</param>
        /// <returns>The World Position of the TileObject</returns>
        Vector3 GetTilePos(TileObject tileObject)
        {
            return new Vector3(
                tileObject.PosX * _config.TileViewWidth,
                tileObject.PosY * _config.TileViewHeight,
                TileZPos);
        }

        /// <summary>
        /// Sets the GameObject in the world position based on the grid coordinates 
        /// </summary>
        /// <param name="x">The column coordinate</param>
        /// <param name="y">The row coordinate</param>
        /// <param name="z">The order in the view</param>
        /// <param name="tile">The GameObject to be placed</param>
        void SetTilePos(int x, int y, int z, GameObject tile)
        {
            tile.transform.localPosition = new Vector3(
                x * _config.TileViewWidth,
                y * _config.TileViewHeight,
                z);
        }

        /// <summary>
        /// Fills up the given TileObjectViews list based on the given TileObjects list
        /// </summary>
        /// <param name="tiles">List of TileObjects</param>
        /// <param name="viewsList">List of TileObjectViews to be filled</param>
        List<TileObjectView> GetViewTiles(List<TileObject> tiles, bool setPositions = false)
        {
            _tilesBeingAnimated.Clear();

            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                var tileView = GetTileAt(tile);

                // If we don't have a view set for the specific tile. We create it. 
                if(tileView == null)
                {
                    tileView = CreateTile(tile);
                }

                if (tileView.TileView.NeedsRefresh)
                {
                    var viewData = _config.GetViewData(tile.TileType, _variation);
                    tileView.TileView.Init(tile, viewData);
                }

                _tilesBeingAnimated.Add(tileView);

                if(setPositions)
                {
                    tileView.TileView.Position = tileView.Spawned
                        ? tileView.TileView.Position
                        : CascadeInitialPosition(tileView.TileObject);

                    tileView.TileView.TargetPosition = GetTilePos(tileView.TileObject);
                }
            }

            return _tilesBeingAnimated;
        }


        public string DebugGrid()
        {
            var sb = new System.Text.StringBuilder();

            for (var y = 0; y < _gridManager.GridHeight; y++)
            {
                for (var x = 0; x < _gridManager.GridWidth; x++)
                {
                    sb.Append(this[x, y].TileView.AppliedTileType + ", ");
                }
            }

            return sb.ToString();
        }


        #endregion

        #region Animations

        /// <summary>
        /// Animation of tiles dropping into the board
        /// </summary>
        /// <param name="tilesToUpdate">List of Tiles to animate</param>
        public async Task PlayTilesDropAnim(List<TileObject> tilesToUpdate)
        {
            var viewTiles = GetViewTiles(tilesToUpdate, true);
            await _animationsController.PlayTilesPositionAnim(viewTiles,
                _config.DropAnimationTime);

            // Tags the view tile as spawned
            for (var i = 0; i < viewTiles.Count; i++)
            {
                viewTiles[i].Spawned = true;
            }
        }

        /// <summary>
        /// Animates two tiles to swap positions
        /// </summary>
        /// <param name="tile1"></param>
        /// <param name="tile2"></param>
        /// <returns></returns>
        public async Task PlaySwapAnim(TileObject tile1, TileObject tile2)
        {
            // Gets the view information about the tiles
            _tilesBeingAnimated.Clear();

            // Gets the tile views and positions information
            var tileView1 = GetTileAt(tile1);
            var tileView2 = GetTileAt(tile2);

            tileView1.TileView.TargetPosition = tileView2.TileView.Position;
            tileView2.TileView.TargetPosition = tileView1.TileView.Position;

            // Plays a blink animation on the tiles being swapped
            tileView1.TileView.DoBlink();
            tileView2.TileView.DoBlink();

            _tilesBeingAnimated.Add(tileView1);
            _tilesBeingAnimated.Add(tileView2);

            await _animationsController.PlayTilesPositionAnim(_tilesBeingAnimated, _config.SwapAnimationTime);
        }

        /// <summary>
        /// Animates the tiles to simulate their destruction
        /// </summary>
        /// <param name="tilesMatched">List of Tiles to animate</param>
        public async Task PlayHideTilesAnim(List<TileObject> tilesMatched)
        {
            var tiles = GetViewTiles(tilesMatched);
            await _animationsController.PlayTilesScaleAnim(tiles, _config.SwapAnimationTime);

            // Despawn the specific tile
            for (var i = 0; i < tiles.Count; i++)
            {
                DeSpawnTile(tiles[i]);
            }
        }

        /// <summary>
        /// Plays a hint animation with a possible match
        /// </summary>
        /// <param name="tilesMatched">List of Tiles to animate</param>
        public async Task PlayHintAnim(List<TileObject> tilesMatched)
        {
            // Gets the view information about the tiles
            var tiles = GetViewTiles(tilesMatched);

            // We animate it as many times as it is configured
            for (int j = 0; j < _config.HintCycles; j++)
            {
                // Enables the highlight sprite and sets it's alpha to 0
                for (var i = 0; i < tiles.Count; i++)
                {
                    var tile = tiles[i].TileView;
                    tile.SetSelectedBackgroundAlpha(0f);
                    tile.HighlightTile();
                }

                // Shows background
                await _animationsController.PlayTilesBackgroundAlphaAnim(tiles,
                    _config.HintAnimationTime * 0.5f, true);

                // Hides background
                await _animationsController.PlayTilesBackgroundAlphaAnim(tiles,
                    _config.HintAnimationTime * 0.5f, false);
            }

            // Disables the high lighting
            for (var i = 0; i < tiles.Count; i++)
            {
                tiles[i].TileView.DehighlightTile();
            }
        }

        #endregion

        #region Utils

        /// <summary>
        /// Calculates the position of the tile when being dropped in the board
        /// </summary>
        /// <param name="tileObject">TileObject to get the position info</param>
        /// <returns>The World Position for the TileObject</returns>
        Vector3 CascadeInitialPosition(TileObject tileObject)
        {
            return new Vector3(
                tileObject.PosX * _config.TileViewWidth,
                (tileObject.PosY + _yCascadePositionOffset) * _config.TileViewHeight,
                TileZPos);
        }

        #endregion
    }
}

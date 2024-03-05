using System.Collections.Generic;
using System.Threading.Tasks;
using OldBard.Match3.Config;
using OldBard.Match3.Gameplay.Views.Animations;
using OldBard.Services.Match3.Grid;
using OldBard.Services.Match3.Grid.Data;
using OldBard.Services.Match3.Grid.Views;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace OldBard.Match3.Gameplay.Views
{
    /// <summary>
    /// GridViewController
    /// </summary>
    public class GridViewController : MonoBehaviour
    {
        /// Declarations

        const int TILE_Z_POS = -2;

        [SerializeField] Transform _tilesParent;
        [SerializeField] SpriteRenderer _tilesBackground;
        [SerializeField] BoxCollider2D _tilesBackgroundCollider;

        GameConfig _gameConfig;
        GridService _gridService;
        AnimationsController _animationsController;
        Camera _camera;

        Vector3 _gridOffset;

        /// <summary>
        /// A HashSet with all the tiles in the view
        /// </summary>
        HashSet<TileInstance> _tiles = new();

        int _variation;

        int _yCascadePositionOffset;
        
        /// Accessors

        /// <summary>
        /// Gets a tile given it's coordinates
        /// </summary>
        /// <param name="x">The Tile Column</param>
        /// <param name="y">The Tile Row</param>
        TileInstance this[int x, int y] => GetTileAt(x, y);

        /// Initialization

        /// <summary>
        /// Initializes the Grid View
        /// </summary>
        /// <param name="gameConfig">The Game Config</param>
        /// <param name="gridService">The Grid Manager</param>
        /// <param name="animationsController"></param>
        public void Initialize(GameConfig gameConfig, GridService gridService, AnimationsController animationsController)
        {
            _gameConfig = gameConfig;
            _gridService = gridService;
            _animationsController = animationsController;
            _camera = Camera.main;

            _gridOffset = new Vector3(-((_gridService.GridWidth - 1) * 0.5f), -((_gridService.GridHeight - 1) * 0.5f), 0f);
            
            _tilesBackground.size = new Vector2(_gridService.GridWidth, _gridService.GridHeight);
            _tilesBackgroundCollider.size = _tilesBackground.size;

            // Inits the specific variation for this play through and the position from which
            // the tiles should come
            _variation = Random.Range(0, _gameConfig.TilesVariations);

            _yCascadePositionOffset = _gridService.GridHeight + 10;

            // Creates the View Tiles
            InitializeGridView(_gridService.GridWidth, _gridService.GridHeight);
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
                    TileView tile = Instantiate(_gameConfig.TilePrefab, _tilesParent);

                    var tileView = new TileInstance
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
            foreach(TileInstance tile in _tiles)
            {
                if (tile != null)
                {
                    Destroy(tile.TileView.gameObject);
                }
            }

            _tiles.Clear();
            _tiles = null;

            _gridService = null;
        }

        /// Populate
        
        /// <summary>
        /// Creates a TileObjectView based on it's object counterpart
        /// </summary>
        /// <param name="tileObject">TileObject reference data</param>
        /// <returns>The TileInstance</returns>
        TileInstance CreateTile(TileObject tileObject)
        {
            // Virtually spawns a view tile
            TileInstance tileView = SpawnTile(tileObject);

            // We place the tile higher in the screen to move it down and have a cascade effect.
            SetTilePos(tileObject.PosX,
                tileObject.PosY + _yCascadePositionOffset,
                TILE_Z_POS, tileView.TileView.gameObject);
            return tileView;
        }

        /// <summary>
        /// Virtually Spawns a TileObjectView
        /// </summary>
        /// <param name="tile">TileObject reference data</param>
        /// <returns>The TileInstance</returns>
        TileInstance SpawnTile(TileObject tile)
        {
            // Go through the list to find the specific TileObject reference
            foreach(TileInstance curTile in _tiles)
            {
                if(curTile.Spawned)
                {
                    continue;
                }

                curTile.TileObject = tile;

                // Gets and sets the tile based on the view data in the config
                TileViewData viewData = _gameConfig.GetViewData(tile.TileType, _variation);
                curTile.TileView.Init(tile, viewData);

                // Enables the tile and tags it as spawned
                curTile.TileView.gameObject.SetActive(true);
                curTile.Spawned = true;

                return curTile;
            }

            return null;
        }

        /// <summary>
        /// Disables a tile and sets it as not spawned
        /// </summary>
        /// <param name="tile">Tile do despawn</param>
        void DeSpawnTile(TileInstance tile)
        {
            tile.Spawned = false;
            tile.TileView.gameObject.SetActive(false);
        }

        /// Access

        /// <summary>
        /// Returns the TileObjectView correspondent to the TileObject
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>The TileInstance</returns>
        TileInstance GetTileAt(TileObject tile)
        {
            foreach(TileInstance curTile in _tiles)
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
        /// <returns>The TileInstance</returns>
        TileInstance GetTileAt(int x, int y)
        {
            foreach(TileInstance curTile in _tiles)
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
            Vector3 worldPos = _camera.ScreenToWorldPoint(pos);
            
            // Calculates the column and row based on the given position and tiles sizes
            var x = Mathf.RoundToInt((worldPos.x - _gridOffset.x) / _gridService.GridSettings.TileViewWidth);
            var y = Mathf.RoundToInt((worldPos.y - _gridOffset.y) / _gridService.GridSettings.TileViewHeight);
            
            return this[x, y]?.TileObject;
        }

        /// <summary>
        /// Gets the world position of the given TileObject
        /// </summary>
        /// <param name="tileObject">TileObject to get the world position</param>
        /// <returns>The World Position of the TileObject</returns>
        Vector3 GetTilePos(TileObject tileObject)
        {
            var pos = new Vector3(
                tileObject.PosX * _gridService.GridSettings.TileViewWidth,
                tileObject.PosY * _gridService.GridSettings.TileViewHeight,
                TILE_Z_POS);

            pos += _gridOffset;

            return pos;
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
             var localPosition = new Vector3(
                x * _gridService.GridSettings.TileViewWidth,
                y * _gridService.GridSettings.TileViewHeight,
                z);
            
            localPosition += _gridOffset;
            tile.transform.localPosition = localPosition;
        }

        /// <summary>
        /// Fills up the given TileObjectViews list based on the given TileObjects list
        /// </summary>
        /// <param name="tiles">List of TileObjects</param>
        /// <param name="tilesBeingAnimated">List of TileInstances to be filled</param>
        /// <param name="setPositions"></param>
        void GetViewTiles(List<TileObject> tiles, List<TileInstance> tilesBeingAnimated, bool setPositions = false)
        {
            foreach(TileObject tile in tiles)
            {
                // If we don't have a view set for the specific tile. We create it. 
                TileInstance tileInstance = GetTileAt(tile) ?? CreateTile(tile);

                if(tileInstance.TileView.NeedsRefresh)
                {
                    TileViewData viewData = _gameConfig.GetViewData(tile.TileType, _variation);
                    tileInstance.TileView.Init(tile, viewData);
                }

                tilesBeingAnimated.Add(tileInstance);

                if(!setPositions)
                {
                    continue;
                }

                tileInstance.TileView.Position = tileInstance.Spawned ? tileInstance.TileView.Position : CascadeInitialPosition(tileInstance.TileObject);

                tileInstance.TileView.TargetPosition = GetTilePos(tileInstance.TileObject);
            }
        }

        public string DebugGrid()
        {
            var sb = new System.Text.StringBuilder();

            for (var y = 0; y < _gridService.GridHeight; y++)
            {
                for (var x = 0; x < _gridService.GridWidth; x++)
                {
                    sb.Append(this[x, y].TileView.AppliedTileType + ", ");
                }
            }

            return sb.ToString();
        }

        /// Animations

        /// <summary>
        /// Animation of tiles dropping into the board
        /// </summary>
        /// <param name="tilesToUpdate">List of Tiles to animate</param>
        public async Task PlayTilesDropAnim(List<TileObject> tilesToUpdate)
        {
            using(ListPool<TileInstance>.Get(out List<TileInstance> viewTiles))
            {
                GetViewTiles(tilesToUpdate, viewTiles, true);
                await _animationsController.PlayTilesPositionAnim(viewTiles, _gameConfig.DropAnimationTime);

                // Tags the view tile as spawned
                foreach(TileInstance tile in viewTiles)
                {
                    tile.Spawned = true;
                }
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
            // Gets the tile views and positions information
            TileInstance tileView1 = GetTileAt(tile1);
            TileInstance tileView2 = GetTileAt(tile2);

            tileView1.TileView.TargetPosition = tileView2.TileView.Position;
            tileView2.TileView.TargetPosition = tileView1.TileView.Position;

            // Plays a blink animation on the tiles being swapped
            tileView1.TileView.DoBlink();
            tileView2.TileView.DoBlink();

            using(ListPool<TileInstance>.Get(out List<TileInstance> tiles))
            {
                tiles.Add(tileView1);
                tiles.Add(tileView2);
            
                await _animationsController.PlayTilesPositionAnim(tiles, _gameConfig.SwapAnimationTime);
            }
        }

        /// <summary>
        /// Animates the tiles to simulate their destruction
        /// </summary>
        /// <param name="tilesMatched">List of Tiles to animate</param>
        public async Task PlayHideTilesAnim(List<TileObject> tilesMatched)
        {
            using(ListPool<TileInstance>.Get(out List<TileInstance> tiles))
            {
                GetViewTiles(tilesMatched, tiles);
                await _animationsController.PlayTilesScaleAnim(tiles, _gameConfig.SwapAnimationTime);

                // Despawn the specific tile
                foreach(TileInstance tile in tiles)
                {
                    DeSpawnTile(tile);
                }
            }
        }

        /// <summary>
        /// Plays a hint animation with a possible match
        /// </summary>
        public async Task PlayHintAnim()
        {
            ListPool<TileObject>.Get(out List<TileObject> tilesMatched);

            if(!_gridService.GetFirstPossibleMatch(tilesMatched))
            {
                ListPool<TileObject>.Release(tilesMatched);
                return;
            }

            using(ListPool<TileInstance>.Get(out List<TileInstance> tiles))
            {
                // Gets the view information about the tiles
                GetViewTiles(tilesMatched, tiles);
                
                ListPool<TileObject>.Release(tilesMatched);

                // We animate it as many times as it is configured
                for(int j = 0; j < _gameConfig.HintCycles; j++)
                {
                    // Enables the highlight sprite and sets it's alpha to 0
                    foreach(TileInstance tile in tiles)
                    {
                        TileView tileView = tile.TileView;
                        tileView.SetSelectedBackgroundAlpha(0f);
                        tileView.HighlightTile();
                    }

                    // Shows background
                    await _animationsController.PlayTilesBackgroundAlphaAnim(tiles, _gameConfig.HintAnimationTime * 0.5f, true);

                    // Hides background
                    await _animationsController.PlayTilesBackgroundAlphaAnim(tiles, _gameConfig.HintAnimationTime * 0.5f, false);
                }

                // Disables the high lighting
                foreach(TileInstance tile in tiles)
                {
                    tile.TileView.DisableTileHighlight();
                }
            }
        }

        /// Utils

        /// <summary>
        /// Calculates the position of the tile when being dropped in the board
        /// </summary>
        /// <param name="tileObject">TileObject to get the position info</param>
        /// <returns>The World Position for the TileObject</returns>
        Vector3 CascadeInitialPosition(TileObject tileObject)
        {
            return new Vector3(
                tileObject.PosX * _gridService.GridSettings.TileViewWidth,
                (tileObject.PosY + _yCascadePositionOffset) * _gridService.GridSettings.TileViewHeight,
                TILE_Z_POS);
        }
    }
}

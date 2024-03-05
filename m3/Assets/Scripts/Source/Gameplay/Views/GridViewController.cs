using System.Collections.Generic;
using System.Threading.Tasks;
using OldBard.Match3.Config;
using OldBard.Match3.Gameplay.Views.Animations;
using OldBard.Services.Match3.Grid;
using OldBard.Services.Match3.Grid.Views;
using UnityEngine;
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

        public class TileInstance
        {
            public TileView TileView;
            public TileObject TileObject;
            public bool Spawned;
        }

        [SerializeField] Transform _tilesParent;
        [SerializeField] SpriteRenderer _tilesBackground;
        [SerializeField] BoxCollider2D _tilesBackgroundCollider;

        GameConfig _gameConfig;
        GridService _gridService;
        AnimationsController _animationsController;
        Camera _camera;

        /// <summary>
        /// A HashSet with all the tiles in the view
        /// </summary>
        
        // A list used as cache for the required iterations 
        List<TileObjectView> _tilesBeingAnimated;
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
            
            // Moves the camera and the board to the proper positions depending on the grid size
            _camera.transform.position = new Vector3((_gridService.GridWidth * 0.5f) - 4f,
                _gridService.GridHeight * 0.5f - 0.5f, 0f);

            _boardFrame.localPosition = new Vector3(_gridService.GridWidth * 0.5f - 0.5f,
                _gridService.GridHeight * 0.5f - 0.5f, 0f);

            _tilesBackground.transform.localPosition = new Vector3(_gridService.GridWidth * 0.5f - 0.5f,
                _gridService.GridHeight * 0.5f - 0.5f, 0f);
            _tilesBackground.size = new Vector2(_gridService.GridWidth, _gridService.GridHeight);
            _tilesBackgroundCollider.size = _tilesBackground.size;

            // Inits the containers
            var maxTiles = _gridService.GridWidth * _gridService.GridHeight;
            _tiles = new HashSet<TileObjectView>();
            _tilesBeingAnimated = new List<TileObjectView>(maxTiles);

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
                if (curTile.Spawned == false)
                {
                    curTile.TileObject = tile;

                    // Gets and sets the tile based on the view data in the config
                    var viewData = _gameConfig.GetViewData(tile.TileType, _variation);
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
            return new Vector3(
                tileObject.PosX * _gridService.GridSettings.TileViewWidth,
                tileObject.PosY * _gridService.GridSettings.TileViewHeight,
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
                x * _gridService.GridSettings.TileViewWidth,
                y * _gridService.GridSettings.TileViewHeight,
                z);
        }

        /// <summary>
        /// Fills up the given TileObjectViews list based on the given TileObjects list
        /// </summary>
        /// <param name="tiles">List of TileObjects</param>
        /// <param name="tilesBeingAnimated">List of TileInstances to be filled</param>
        /// <param name="setPositions"></param>
        void GetViewTiles(List<TileObject> tiles, List<TileInstance> tilesBeingAnimated, bool setPositions = false)
        {
            _tilesBeingAnimated.Clear();

            foreach(TileObject tile in tiles)
            {
                var tile = tiles[i];
                var tileView = GetTileAt(tile);

                // If we don't have a view set for the specific tile. We create it. 
                if(tileView == null)
                {
                    tileView = CreateTile(tile);
                }

                if(tileInstance.TileView.NeedsRefresh)
                {
                    TileViewData viewData = _gameConfig.GetViewData(tile.TileType, _variation);
                    tileInstance.TileView.Init(tile, viewData);
                }

                tilesBeingAnimated.Add(tileInstance);

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
            var viewTiles = GetViewTiles(tilesToUpdate, true);
            await _animationsController.PlayTilesPositionAnim(viewTiles,
                _gameConfig.DropAnimationTime);

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
            TileInstance tileView1 = GetTileAt(tile1);
            TileInstance tileView2 = GetTileAt(tile2);

            tileView1.TileView.TargetPosition = tileView2.TileView.Position;
            tileView2.TileView.TargetPosition = tileView1.TileView.Position;

            // Plays a blink animation on the tiles being swapped
            tileView1.TileView.DoBlink();
            tileView2.TileView.DoBlink();

            _tilesBeingAnimated.Add(tileView1);
            _tilesBeingAnimated.Add(tileView2);

            await _animationsController.PlayTilesPositionAnim(_tilesBeingAnimated, _gameConfig.SwapAnimationTime);
        }

        /// <summary>
        /// Animates the tiles to simulate their destruction
        /// </summary>
        /// <param name="tilesMatched">List of Tiles to animate</param>
        public async Task PlayHideTilesAnim(List<TileObject> tilesMatched)
        {
            var tiles = GetViewTiles(tilesMatched);
            await _animationsController.PlayTilesScaleAnim(tiles, _gameConfig.SwapAnimationTime);

            // Despawn the specific tile
            for (var i = 0; i < tiles.Count; i++)
            {
                DeSpawnTile(tiles[i]);
            }
        }

        /// <summary>
        /// Plays a hint animation with a possible match
        /// </summary>
        public async Task PlayHintAnim()
        {
            // Gets the view information about the tiles
            var tiles = GetViewTiles(tilesMatched);

            // We animate it as many times as it is configured
            for (int j = 0; j < _gameConfig.HintCycles; j++)
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
                    _gameConfig.HintAnimationTime * 0.5f, true);

                // Hides background
                await _animationsController.PlayTilesBackgroundAlphaAnim(tiles,
                    _gameConfig.HintAnimationTime * 0.5f, false);
            }

            // Disables the high lighting
            for (var i = 0; i < tiles.Count; i++)
            {
                tiles[i].TileView.DehighlightTile();
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

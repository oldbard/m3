using System.Collections.Generic;
using System.Threading.Tasks;
using OldBard.Match3.Gameplay.Views.Animations;
using OldBard.Services.Match3.Grid;
using OldBard.Services.Match3.Grid.Data;
using OldBard.Services.Match3.Grid.Views;
using UnityEngine;
using UnityEngine.Pool;

namespace OldBard.Match3.Gameplay.Views
{
    /// <summary>
    /// GridViewController
    /// </summary>
    public class GridViewController : MonoBehaviour
    {
        /// Declarations

        [SerializeField] SpriteRenderer _tilesBackground;
        [SerializeField] BoxCollider2D _tilesBackgroundCollider;

        GridConfig _config;
        GridService _gridService;
        AnimationsController _animationsController;
        Camera _camera;

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
        /// <param name="gridConfig">The Game Config</param>
        /// <param name="gridService">The Grid Manager</param>
        /// <param name="animationsController"></param>
        public void Initialize(GridConfig gridConfig, GridService gridService, AnimationsController animationsController)
        {
            _config = gridConfig;
            _gridService = gridService;
            _animationsController = animationsController;
            _camera = Camera.main;

            _tilesBackground.size = new Vector2(_gridService.GridWidth, _gridService.GridHeight);
            _tilesBackgroundCollider.size = _tilesBackground.size;
        }

        /// <summary>
        /// Unity's On Destroy event. We use it to clean up the containers and objects we created.
        /// </summary>
        void OnDestroy()
        {
            _gridService = null;
        }

        /// Access

        /// <summary>
        /// Returns the TileObjectView at the specific coordinates.
        /// </summary>
        /// <param name="x">Tile Column</param>
        /// <param name="y">Tile Row</param>
        /// <returns>The TileInstance</returns>
        TileInstance GetTileAt(int x, int y)
        {
            return _gridService[x, y];
        }

        /// <summary>
        /// Returns the TileObject at the specific screen position
        /// </summary>
        /// <param name="pos">The screen position</param>
        /// <returns>The TileObject</returns>
        public TileInstance GetTileAt(Vector3 pos)
        {
            // Converts the screen position into a world position
            Vector3 worldPos = _camera.ScreenToWorldPoint(pos);
            
            // Calculates the column and row based on the given position and tiles sizes
            var x = Mathf.RoundToInt((worldPos.x - _gridService.GridOffset.x) / _gridService.GridSettings.TileViewWidth);
            var y = Mathf.RoundToInt((worldPos.y - _gridService.GridOffset.y) / _gridService.GridSettings.TileViewHeight);
            
            return this[x, y];
        }

        public string DebugGrid()
        {
            var sb = new System.Text.StringBuilder();

            for (var y = 0; y < _gridService.GridHeight; y++)
            {
                for (var x = 0; x < _gridService.GridWidth; x++)
                {
                    sb.Append($"{this[x, y].TileView.AppliedTileType}: ({x}, {y}), ");
                }

                sb.AppendLine("");
            }

            return sb.ToString();
        }

        /// Animations

        /// <summary>
        /// Animation of tiles dropping into the board
        /// </summary>
        /// <param name="tilesToUpdate">List of Tiles to animate</param>
        public async Task PlayTilesDropAnim(IReadOnlyList<TileInstance> tilesToUpdate)
        {
            await _animationsController.PlayTilesPositionAnim(tilesToUpdate, _config.DropAnimationTime);
        }

        /// <summary>
        /// Animates two tiles to swap positions
        /// </summary>
        /// <param name="tile1"></param>
        /// <param name="tile2"></param>
        /// <returns></returns>
        public async Task PlaySwapAnim(TileInstance tile1, TileInstance tile2)
        {
            tile1.TileView.TargetPosition = tile2.TileView.Position;
            tile2.TileView.TargetPosition = tile1.TileView.Position;

            // Plays a blink animation on the tiles being swapped
            tile1.TileView.DoBlink();
            tile2.TileView.DoBlink();

            using(ListPool<TileInstance>.Get(out List<TileInstance> tiles))
            {
                tiles.Add(tile1);
                tiles.Add(tile2);
            
                await _animationsController.PlayTilesPositionAnim(tiles, _config.SwapAnimationTime);
            }
        }

        /// <summary>
        /// Animates the tiles to simulate their destruction
        /// </summary>
        /// <param name="tilesMatched">List of Tiles to animate</param>
        public async Task PlayHideTilesAnim(List<TileInstance> tilesMatched)
        {
            await _animationsController.PlayTilesScaleAnim(tilesMatched, _config.SwapAnimationTime);
        }

        /// <summary>
        /// Plays a hint animation with a possible match
        /// </summary>
        public async Task PlayHintAnim()
        {
            ListPool<TileInstance>.Get(out List<TileInstance> tilesMatched);

            if(!_gridService.GetFirstPossibleMatch(tilesMatched))
            {
                ListPool<TileInstance>.Release(tilesMatched);
                return;
            }

            // We animate it as many times as it is configured
            for(int j = 0; j < _config.HintCycles; j++)
            {
                // Enables the highlight sprite and sets it's alpha to 0
                foreach(TileInstance tile in tilesMatched)
                {
                    TileView tileView = tile.TileView;
                    tileView.SetSelectedBackgroundAlpha(0f);
                    tileView.HighlightTile();
                }

                // Shows background
                await _animationsController.PlayTilesBackgroundAlphaAnim(tilesMatched, _config.HintAnimationTime * 0.5f, true);

                // Hides background
                await _animationsController.PlayTilesBackgroundAlphaAnim(tilesMatched, _config.HintAnimationTime * 0.5f, false);
            }

            // Disables the high lighting
            foreach(TileInstance tile in tilesMatched)
            {
                tile.TileView.DisableTileHighlight();
            }

            ListPool<TileInstance>.Release(tilesMatched);
        }
    }
}

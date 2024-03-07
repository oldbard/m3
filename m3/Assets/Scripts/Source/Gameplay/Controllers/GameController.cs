using System.Collections.Generic;
using System.Threading.Tasks;
using OldBard.Match3.Gameplay.Views;
using OldBard.Match3.Gameplay.Views.Animations;
using OldBard.Match3.Gameplay.Views.UI;
using OldBard.Services.Match3.Audio;
using OldBard.Services.Match3.Grid;
using OldBard.Services.Match3.Grid.Data;
using UnityEngine;
using UnityEngine.Pool;

namespace OldBard.Match3.Gameplay.Controllers
{
    /// <summary>
    /// GameController. Controls the initialization, flow, and conditions of the match
    /// </summary>
    public class GameController : MonoBehaviour
    {
        /// Declarations

        const string HIGH_SCORE_KEY = "HighScore";
        const string GRID_WIDTH_KEY = "GridWidth";
        const string GRID_HEIGHT_KEY = "GridHeight";
        const string TILES_VARIATIONS_KEY = "TilesVariations";
        const string MATCH_DURATION_KEY = "MatchDuration";

        [SerializeField] GridConfig _config;
        [SerializeField] GridSettings _gridSettings;
        [SerializeField] GridViewController _gridView;
        [SerializeField] GameUIController _gameUIController;
        [SerializeField] InputManager _inputManager;
        [SerializeField] Services.Match3.Audio.AudioSettings _audioSettings;
        [SerializeField] AudioService _audioService;
        [SerializeField] Transform _tilesParent;

        GridService _gridService;
        AnimationsController _animationsController;

        bool _gameIsRunning;

        int _score;
        int _initialDuration;
        float _timeLeft;

        /// Accessors

        /// <summary>
        /// Game Score
        /// </summary>
        int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    _gameUIController.UpdateScore(_score);
                }
            }
        }

        /// Initialization

        /// <summary>
        /// Unity Awake Event.
        /// </summary>
        void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Unity OnDestroy Event. We use it to unregister to actions
        /// </summary>
        void OnDestroy()
        {
            TileInstanceFactory.Instance.Terminate();

            _gameIsRunning = false;

            UnregisterEvents();

            _gridService.Dispose();
            _gridService = null;
        }

        /// <summary>
        /// Initializes the match
        /// </summary>
        async void Initialize()
        {
            _inputManager.DisableInput();

            _initialDuration = PlayerPrefs.GetInt(MATCH_DURATION_KEY, _config.GameDuration);

            _animationsController = new AnimationsController(_config.LerpAnimationCurve);

            _timeLeft = _initialDuration;
            
            // Gets the configuration. Loads the data from the PlayerPrefs. Uses the
            // information in the config as default.
            var width = PlayerPrefs.GetInt(GRID_WIDTH_KEY, _gridSettings.DefaultGridWidth);
            var height = PlayerPrefs.GetInt(GRID_HEIGHT_KEY, _gridSettings.DefaultGridHeight);
            var variations = PlayerPrefs.GetInt(TILES_VARIATIONS_KEY, _config.NumberOfTileTypes);

            var variation = Random.Range(0, _config.TilesVariations);

            TileInstanceFactory.Instance.Initialize(_config, _gridSettings, _tilesParent, variation, width * height);

            InitHUD();

            await InitGrid(width, height, variations);

            RegisterEvents();

            _gameIsRunning = true;

            _inputManager.EnableInput();
        }

        /// <summary>
        /// Initializes the HUD
        /// </summary>
        void InitHUD()
        {
            _gameUIController.UpdateScore(0);

            _gameUIController.Init(_config, _animationsController);
            _audioService.InitSound(_audioSettings);

            var highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            _gameUIController.UpdateHighScore(highScore);

            UpdateTimer();
        }

        /// <summary>
        /// Initializes the Grid. Both Logic and View
        /// </summary>
        async Task InitGrid(int width, int height, int variations)
        {
            _gridService = new GridService(_gridSettings, width, height, variations, System.DateTime.UtcNow.Millisecond);

            _gridView.Initialize(_config, _gridService, _animationsController);

            await _gridView.PlayTilesDropAnim(_gridService.Tiles);
        }

        /// <summary>
        /// Registers external events / callbacks
        /// </summary>
        void RegisterEvents()
        {
            _gameUIController.ShowHint += OnShowHint;
            _inputManager.Dragged += OnDragged;
            _gameUIController.TimeOut += _audioService.PlayTimeoutClip;
        }

        /// <summary>
        /// Unregisters external events / callbacks
        /// </summary>
        void UnregisterEvents()
        {
            _gameUIController.ShowHint -= OnShowHint;
            _inputManager.Dragged -= OnDragged;
            _gameUIController.TimeOut -= _audioService.PlayTimeoutClip;
        }

        /// Loop

        void Update()
        {
            if(!_gameIsRunning)
            {
                return;
            }
            
            _timeLeft -= Time.deltaTime;

            int flooredTimeLeft = UpdateTimer();

            // If time is over. Game Over
            if (flooredTimeLeft <= 0)
            {
                GameOver();

                return;
            }

            // Checks if the game is running, if the player can interact and if a
            // given amount of time has passed. If so, shows a hint to the player
            if(!_inputManager.CanDrag || (_inputManager.TimeSinceLastInteraction < _config.TimeToShowHint - 1))
            {
                return;
            }

            OnShowHint();
            _inputManager.ResetLastInteraction();
        }

        int UpdateTimer()
        {
            int flooredTimeLeft = Mathf.FloorToInt(_timeLeft);
            
            _gameUIController.UpdateTimer(flooredTimeLeft);

            return flooredTimeLeft;
        }

        /// <summary>
        /// Game over. Wraps the match data and show it to the player.
        /// </summary>
        void GameOver()
        {
            // Stops the game and disables the input.
            _gameIsRunning = false;
            _inputManager.DisableInput();
            
            // Calculates the score and checks if it is the new High Score
            var highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

            var isHighScore = Score > highScore;
            
            // Updates the High Score if needed
            if(isHighScore)
            {
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, Score);
            }

            UnregisterEvents();

            // Shows the Game Over popup
            _gameUIController.ShowGameOver(Score, isHighScore);
            _audioService.PlayGameOver(isHighScore);
        }

        /// Matching

        /// <summary>
        /// Unity's input callback with the data about dragging
        /// </summary>
        /// <param name="curPos">The cursor current position</param>
        /// <param name="initialDragPos">The drag initial position</param>
        void OnDragged(Vector3 curPos, Vector3 initialDragPos)
        {
            // Gets the mouse pointer position and compares with the previous position
            Vector3 diff = curPos - initialDragPos;

            // If the player dragged enough, process the input
            if(!(diff.sqrMagnitude >= _config.DragDetectionThreshold * _config.DragDetectionThreshold))
            {
                return;
            }

            // Gets the tile that is being dragged
            TileInstance pickedTile = _gridView.GetTileAt(initialDragPos);

            // Dragged outside the board
            if(pickedTile == null)
            {
                return;
            }

            // Gets the neighbour tile based on the direction of the drag
            GridService.DragDirection dir = GetDirection(diff.normalized);
            TileInstance neighbour = _gridService.GetNeighbourTile(pickedTile, dir);

            // Dragged in a bad direction
            if(neighbour == null)
            {
                return;
            }

            // Data is fine. Blocks the input
            _inputManager.DisableInput();
                
            // Swaps the involved tiles positions
            DoSwapAnims(pickedTile, neighbour);
        }

        /// <summary>
        /// Swaps the tiles positions in the logic and view.
        /// </summary>
        /// <param name="tile1">Tile to swap position</param>
        /// <param name="tile2">Tile to swap position</param>
        async void DoSwapAnims(TileInstance tile1, TileInstance tile2)
        {
            // Plays an audio specific to the swapping
            _audioService.PlaySwapClip();
            
            // Plays the swap animation and waits for it to end
            await _gridView.PlaySwapAnim(tile1, tile2);

            using(ListPool<TileInstance>.Get(out List<TileInstance> tiles))
            {
                // Tries to swap the tiles in the logic and get the affected tiles (matches)
                if(_gridService.TrySwapTiles(tile1, tile2, tiles))
                {
                    // If we managed to make a match, process it
                    await ProcessMatches(tiles);
                }
                else
                {
                    // If we failed to make a match, undo the swap
                    await _gridView.PlaySwapAnim(tile1, tile2);
                }
            }

            // Re-enables the input
            _inputManager.EnableInput();
        }

        /// <summary>
        /// Processes the list of matched tiles
        /// </summary>
        /// <param name="tiles">List of matched tiles</param>
        async Task ProcessMatches(List<TileInstance> tiles)
        {
            ListPool<TileInstance>.Get(out List<TileInstance> tilesToUpdateView);

            // As we remove the matched tiles and have new ones coming, we may
            // find new matches which needs to be processed. So we keep on
            // Cascading / matching until all is done.
            while(tiles.Count > 0)
            {
                // Calculates the player score
                Score += tiles.Count * _config.PointsPerTile;

                // Play a sound for the match and destruction
                _audioService.PlayMatchClip();
                
                _gridService.InvalidateTiles(tiles);

                // Waits for the matched tiles destruction animation to end.
                await _gridView.PlayHideTilesAnim(tiles);

                // Moves the tiles down. Gets a list of all the involved tiles from the manager
                tilesToUpdateView.Clear();
                _gridService.MoveTilesDown(tilesToUpdateView);
                
                _gridService.ReleaseTiles(tiles);

                // Shows in the view the movement of the tiles and placement of new ones
                await _gridView.PlayTilesDropAnim(tilesToUpdateView);

                // Checks if we have more tiles to process because of the cascading
                tiles.Clear();
                _gridService.TryFindAndProcessMatches(tiles);
            }
            
            ListPool<TileInstance>.Release(tilesToUpdateView);
            
            var gmDebug = _gridService.DebugGrid();
            var gvDebug = _gridView.DebugGrid();

            if (!gmDebug.Equals(gvDebug))
            {
                Debug.Log("GridM: " + gmDebug);
                Debug.Log("GridV: " + gvDebug);
            }
        }

        /// <summary>
        /// Callback from the Show Hint action from the HUD.
        /// </summary>
        void OnShowHint()
        {
            // Gets a list of the first possible match and shows it to the player
            _ = _gridView.PlayHintAnim();
        }

        /// Utils

        /// <summary>
        /// Gets the direction information based on the drag vector
        /// </summary>
        /// <param name="dir">The direction of the drag vector</param>
        /// <returns>The DragDirection from the Vector</returns>
        GridService.DragDirection GetDirection(Vector3 dir)
        {
            // Moved left or right
            if(Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                return dir.x > 0 ? GridService.DragDirection.Right : GridService.DragDirection.Left;
            }

            return dir.y > 0 ? GridService.DragDirection.Down : GridService.DragDirection.Up;
        }
    }
}

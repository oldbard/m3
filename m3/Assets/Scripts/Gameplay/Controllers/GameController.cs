﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameData;
using Gameplay.Animations;
using Gameplay.Views;
using Shared;
using Sounds;
using UI;
using UnityEngine;

namespace Gameplay.Controllers
{
    /// <summary>
    /// GameController. Controls the initialization, flow, and conditions of the match
    /// </summary>
    public class GameController : MonoBehaviour
    {
        #region Declarations

        const string HighScoreKey = "HighScore";
        const string GridWidthKey = "GridWidth";
        const string GridHeightKey = "GridHeight";
        const string TilesVariationsKey = "TilesVariations";
        const string MatchDurationKey = "MatchDuration";

        [SerializeField] Config _config;
        [SerializeField] GridViewController _gridView;
        [SerializeField] GameUIController _gameUIController;
        [SerializeField] InputManager _inputManager;
        [SerializeField] SoundsManager _soundsManager;

        GridManager _gridManager;
        AnimationsController _animationsController;

        CancellationTokenSource _timerLoopTokenSource;

        bool _gameIsRunning;

        int _score;
        int _timeLeft;

        #endregion

        #region Accessors

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

        /// <summary>
        /// Time Left to end the match
        /// </summary>
        private int TimeLeft
        {
            get => _timeLeft;
            set
            {
                if(_timeLeft != value)
                {
                    _timeLeft = value;
                    _gameUIController.UpdateTimer(_timeLeft);
                }
            }
        }

        #endregion
        
        #region Initialization

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
            _gameIsRunning = false;

            _timerLoopTokenSource.Cancel();

            UnregisterEvents();

            _gridManager.Dispose();
            _gridManager = null;
        }

        /// <summary>
        /// Initializes the match
        /// </summary>
        async void Initialize()
        {
            _inputManager.DisableInput();

            var duration = PlayerPrefs.GetInt(MatchDurationKey, _config.GameDuration);

            _animationsController = new AnimationsController(_config.LerpAnimationCurve);

            InitHUD();

            TimeLeft = duration;

            await InitGrid();

            RegisterEvents();

            _gameIsRunning = true;

            _inputManager.EnableInput();

            _timerLoopTokenSource = new CancellationTokenSource();

            TimerLoop(_timerLoopTokenSource.Token);
        }

        /// <summary>
        /// Initializes the HUD
        /// </summary>
        void InitHUD()
        {
            _gameUIController.UpdateScore(0);

            _gameUIController.Init(_config, _animationsController);
            _soundsManager.InitSound(_config);

            var highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
            _gameUIController.UpdateHighScore(highScore);
        }

        /// <summary>
        /// Initializes the Grid. Both Logic and View
        /// </summary>
        async Task InitGrid()
        {
            // Gets the configuration. Loads the data from the PlayerPrefs. Uses the
            // information in the config as default.
            var width = PlayerPrefs.GetInt(GridWidthKey, _config.DefaultGridWidth);
            var height = PlayerPrefs.GetInt(GridHeightKey, _config.DefaultGridHeight);
            var variations = PlayerPrefs.GetInt(TilesVariationsKey, _config.NumberOfTileTypes);
            
            _gridManager = new GridManager(width, height, variations, System.DateTime.UtcNow.Millisecond);

            _gridView.Initialize(_config, _gridManager, _animationsController);
            
            // Gets the tiles data created by the GridManager and sends it to be shown in the view
            var tiles = _gridManager.ShuffleGrid();
            await _gridView.PlayTilesDropAnim(tiles);
        }

        /// <summary>
        /// Registers external events / callbacks
        /// </summary>
        void RegisterEvents()
        {
            _gameUIController.ShowHint += OnShowHint;
            _inputManager.Dragged += OnDragged;
            _gameUIController.TimeOut += _soundsManager.PlayTimeoutClip;
        }

        /// <summary>
        /// Unregisters external events / callbacks
        /// </summary>
        void UnregisterEvents()
        {
            _gameUIController.ShowHint -= OnShowHint;
            _inputManager.Dragged -= OnDragged;
            _gameUIController.TimeOut -= _soundsManager.PlayTimeoutClip;
        }

        #endregion

        #region Loop

        /// <summary>
        /// Timer loop
        /// </summary>
        async void TimerLoop(CancellationToken token)
        {
            while(_gameIsRunning)
            {
                // We only update once per second.
                await Task.Delay(1000);

                if(token.IsCancellationRequested)
                {
                    return;
                }

                TimeLeft -= 1;

                // If time is over. Game Over
                if (TimeLeft <= 0f)
                {
                    GameOver();
                }

                // Checks if the game is running, if the player can interact and if a
                // given amount of time has passed. If so, shows a hint to the player
                if (_inputManager.CanDrag && _inputManager.TimeSinceLastInteraction > _config.TimeToShowHint)
                {
                    OnShowHint();
                    _inputManager.ResetLastInteraction();
                }
            }

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
            var highScore = PlayerPrefs.GetInt(HighScoreKey, 0);

            var isHighScore = Score > highScore;
            
            // Updates the High Score if needed
            if(isHighScore)
            {
                PlayerPrefs.SetInt(HighScoreKey, Score);
            }

            UnregisterEvents();

            // Shows the Game Over popup
            _gameUIController.ShowGameOver(Score, isHighScore);
            _soundsManager.PlayGameOver(isHighScore);
        }

        #endregion


        #region Matching

        /// <summary>
        /// Unity's input callback with the data about dragging
        /// </summary>
        /// <param name="curPos">The cursor current position</param>
        /// <param name="initialDragPos">The drag initial position</param>
        public void OnDragged(Vector3 curPos, Vector3 initialDragPos)
        {
            // Gets the mouse pointer position and compares with the previous position
            var diff = curPos - initialDragPos;

            // If the player dragged enough, process the input
            if(diff.sqrMagnitude >= _config.DragDetectionThreshold * _config.DragDetectionThreshold)
            {
                // Gets the tile that is being dragged
                var pickedTile = _gridView.GetTileAt(initialDragPos);

                // Dragged outside the board
                if(pickedTile == null) return;

                // Gets the neighbour tile based on the direction of the drag
                var dir = GetDirection(diff.normalized);
                var neighbour = _gridManager.GetNeighbourTile(pickedTile, dir);

                // Dragged in a bad direction
                if(neighbour == null) return;

                // Data is fine. Blocks the input
                _inputManager.DisableInput();
                
                // Swaps the involved tiles positions
                DoSwapAnims(pickedTile, neighbour);
            }
        }

        /// <summary>
        /// Swaps the tiles positions in the logic and view.
        /// </summary>
        /// <param name="tile1">Tile to swap position</param>
        /// <param name="tile2">Tile to swap position</param>
        async void DoSwapAnims(TileObject tile1, TileObject tile2)
        {
            // Plays an audio specific to the swapping
            _soundsManager.PlaySwapClip();
            
            // Plays the swap animation and waits for it to end
            await _gridView.PlaySwapAnim(tile1, tile2);

            // Tries to swap the tiles in the logic and get the affected tiles (matches)
            var tiles = _gridManager.TrySwapTiles(tile1, tile2);

            var success = tiles.Count > 0;
            
            if(success)
            {
                // If we managed to make a match, process it
                await ProcessMatches(tiles);
            }
            else
            {
                // If we failed to make a match, undo the swap
                await _gridView.PlaySwapAnim(tile1, tile2);
            }

            // Re-enables the input
            _inputManager.EnableInput();
        }

        /// <summary>
        /// Processes the list of matched tiles
        /// </summary>
        /// <param name="tiles">List of matched tiles</param>
        async Task ProcessMatches(List<TileObject> tiles)
        {
            // As we remove the matched tiles and have new ones coming, we may
            // find new matches which needs to be processed. So we keep on
            // Cascading / matching until all is done.
            while(tiles.Count > 0)
            {
                // Calculates the player score
                Score += tiles.Count * _config.PointsPerTile;

                // Play a sound for the match and destruction
                _soundsManager.PlayMatchClip();
                
                // Waits for the matched tiles destruction animation to end.
                await _gridView.PlayHideTilesAnim(tiles);

                // Moves the tiles down. Gets a list of all the involved tiles from the manager
                var animatedTiles = _gridManager.MoveTilesDown();

                // Shows in the view the movement of the tiles and placement of new ones
                await _gridView.PlayTilesDropAnim(animatedTiles);

                // Checks if we have more tiles to process because of the cascading
                tiles = _gridManager.FindAndProcessMatches();
            }
            var gmDebug = _gridManager.DebugGrid();
            var gvDebug = _gridView.DebugGrid();

            if (!gmDebug.Equals(gvDebug))
            {
                UnityEngine.Debug.Log("GridM: " + gmDebug);
                UnityEngine.Debug.Log("GridV: " + gvDebug);
            }
        }

        /// <summary>
        /// Callback from the Show Hint action from the HUD.
        /// </summary>
        void OnShowHint()
        {
            // Gets a list of the first possible match and shows it to the player
            var list = _gridManager.GetFirstPossibleMatch();
            _gridView.PlayHintAnim(list);
        }

        #endregion

        #region Utils
        
        /// <summary>
        /// Gets the direction information based on the drag vector
        /// </summary>
        /// <param name="dir">The direction of the drag vector</param>
        /// <returns>The DragDirection from the Vector</returns>
        GridManager.DragDirection GetDirection(Vector3 dir)
        {
            // Moved left or right
            if(Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                return dir.x > 0 ? GridManager.DragDirection.Right : GridManager.DragDirection.Left;
            }

            return dir.y > 0 ? GridManager.DragDirection.Down : GridManager.DragDirection.Up;
        }

        #endregion
    }
}

using GameData;
using Gameplay.Animations;
using GameServices;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// GameUIController. Used to handle / update the in game ui
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] private Text _timer;
        [SerializeField] private Text _score;
        [SerializeField] private Text _highScore;
        [SerializeField] GameOverScreenController _gameOverController;

        Config _config;
        AnimationsController _animationsController;

        public Action ShowHint;
        public Action TimeOut;

        bool _blinkingTimer;
        GamePersistentData _gamePersistentData;

        ConfigData ConfigData
        {
            get
            {
                if (_gamePersistentData == null)
                {
                    _gamePersistentData = Services.Resolve<GamePersistentData>();
                }

                return _gamePersistentData.ConfigData;
            }
        }

        public void Init(Config config, AnimationsController animationsController)
        {
            _config = config;
            _animationsController = animationsController;
        }

        public void UpdateTimer(int timeLeft)
        {
            var timeSpan = TimeSpan.FromSeconds(timeLeft);
            _timer.text = $"Time Left: {timeSpan:mm':'ss}";

            if(!_blinkingTimer && timeLeft <= ConfigData.TimeToShowWarning + 1)
            {
                _blinkingTimer = true;
                ShowBlinkingTimer();
            }
        }

        async void ShowBlinkingTimer()
        {
            var defColor = _timer.color;
            
            // Wait half a second to start showing the timer in red
            await Task.Delay(ConfigData.StartBlinkDelay);

            float lerpInterval = 0.3f;

            for(var i = 0; i < ConfigData.TimeToShowWarning; i++)
            {
                await Task.Delay(ConfigData.FullColorBlinkDelay);

                await _animationsController.PlayTextColorAnim(_timer, defColor, Color.red, lerpInterval);

                TimeOut?.Invoke();

                await Task.Delay(ConfigData.FullColorBlinkDelay);

                await _animationsController.PlayTextColorAnim(_timer, Color.red, defColor, lerpInterval);
            }
        }

        public void UpdateScore(int score)
        {
            _score.text = $"Score: {score.ToString()}";
        }

        public void UpdateHighScore(int score)
        {
            _highScore.text = $"High Score: {score.ToString()}";
        }

        public void ShowGameOver(int score, bool highScore)
        {
            _gameOverController.Show(score, highScore);
        }

        public void OnShowHint()
        {
            ShowHint?.Invoke();
        }

        public void OnExit()
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}
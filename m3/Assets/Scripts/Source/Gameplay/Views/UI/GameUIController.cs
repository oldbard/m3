using OldBard.Match3.Gameplay.Views.Animations;
using System;
using System.Threading.Tasks;
using OldBard.Services.Match3.Grid.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OldBard.Match3.Gameplay.Views.UI
{
    /// <summary>
    /// GameUIController. Used to handle / update the in game ui
    /// </summary>
    public class GameUIController : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _timer;
        [SerializeField] TextMeshProUGUI _score;
        [SerializeField] TextMeshProUGUI _highScore;
        [SerializeField] GameOverScreenController _gameOverController;

        GridConfig _config;
        AnimationsController _animationsController;

        public Action ShowHint;
        public Action TimeOut;

        bool _timerIsBlinking;

        public void Init(GridConfig config, AnimationsController animationsController)
        {
            _config = config;
            _animationsController = animationsController;
        }

        public void UpdateTimer(int seconds)
        {
            int minutes = (seconds % 3600) / 60;
            seconds %= 60;

            _timer.SetText("Time Left: {0:00}:{1:00}", minutes, seconds);

            if(_timerIsBlinking || seconds > _config.TimeToShowWarning + 1)
            {
                return;
            }

            _timerIsBlinking = true;
            ShowBlinkingTimer();
        }

        async void ShowBlinkingTimer()
        {
            Color defColor = _timer.color;
            
            // Wait half a second to start showing the timer in red
            await Task.Delay(_config.StartBlinkDelay);

            const float LERP_INTERVAL = 0.3f;

            for(var i = 0; i < _config.TimeToShowWarning; i++)
            {
                await Task.Delay(_config.FullColorBlinkDelay);

                await _animationsController.PlayTextColorAnim(_timer, defColor, Color.red, LERP_INTERVAL);

                TimeOut?.Invoke();

                await Task.Delay(_config.FullColorBlinkDelay);

                await _animationsController.PlayTextColorAnim(_timer, Color.red, defColor, LERP_INTERVAL);
            }
        }

        public void UpdateScore(int score)
        {
            _score.SetText("Score: {0}", score);
        }

        public void UpdateHighScore(int score)
        {
            _highScore.SetText("High Score: {0}", score);
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
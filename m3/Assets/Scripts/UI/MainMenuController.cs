using System;
using GameData;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// MainMenuController
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        const string GridWidthKey = "GridWidth";
        const string GridHeightKey = "GridHeight";
        const string TilesVariationsKey = "TilesVariations";
        const string MatchDurationKey = "MatchDuration";
        const string HighScoreKey = "HighScore";

        [SerializeField] Config _config;
        
        [SerializeField] Slider _widthSlider;
        [SerializeField] Slider _heightSlider;
        [SerializeField] Slider _variationsSlider;
        [SerializeField] Slider _durationSlider;
        [SerializeField] Text _widthText;
        [SerializeField] Text _heightText;
        [SerializeField] Text _variationsText;
        [SerializeField] Text _durationText;
        [SerializeField] Text _highScoreText;

        [SerializeField] AudioSource _backgroundMusic;
        
        void Awake()
        {
            _widthSlider.value = PlayerPrefs.GetInt(GridWidthKey, _config.DefaultGridWidth);
            OnChangeWidth(_widthSlider.value);
            _heightSlider.value = PlayerPrefs.GetInt(GridHeightKey, _config.DefaultGridHeight);
            OnChangeHeight(_heightSlider.value);
            _variationsSlider.value = PlayerPrefs.GetInt(TilesVariationsKey, _config.NumberOfTileTypes);
            OnChangeVariations(_variationsSlider.value);
            _durationSlider.value = PlayerPrefs.GetInt(MatchDurationKey, _config.GameDuration);
            OnChangeDuration(_durationSlider.value);
            
            _highScoreText.text = $"High Score: {PlayerPrefs.GetInt(HighScoreKey, 0)}";
        }

        public void OnChangeWidth(Single width)
        {
            PlayerPrefs.SetInt(GridWidthKey, (int)width);
            _widthText.text = $"Grid Width: {(int)width}";
        }

        public void OnChangeHeight(Single height)
        {
            PlayerPrefs.SetInt(GridHeightKey, (int)height);
            _heightText.text = $"Grid Height: {(int)height}";
        }

        public void OnChangeVariations(Single variations)
        {
            PlayerPrefs.SetInt(TilesVariationsKey, (int)variations);
            _variationsText.text = $"Tiles Variations: {(int)variations}";
        }

        public void OnChangeDuration(Single duration)
        {
            PlayerPrefs.SetInt(MatchDurationKey, (int)duration);
            var timeSpan = TimeSpan.FromSeconds(duration);
            _durationText.text = $"Match Duration: {timeSpan:mm':'ss}";
        }

        public void OnPlay()
        {
            _backgroundMusic.Stop();
            SceneManager.LoadScene("Gameplay");
        }
    }
}

using OldBard.Services.Match3.Grid;
using System;
using OldBard.Services.Match3.Grid.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OldBard.Match3.Meta
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

        [SerializeField] GridConfig _config;
        [SerializeField] GridSettings _gridSettings;
        
        [SerializeField] Slider _widthSlider;
        [SerializeField] Slider _heightSlider;
        [SerializeField] Slider _variationsSlider;
        [SerializeField] Slider _durationSlider;
        [SerializeField] TextMeshProUGUI _widthText;
        [SerializeField] TextMeshProUGUI _heightText;
        [SerializeField] TextMeshProUGUI _variationsText;
        [SerializeField] TextMeshProUGUI _durationText;
        [SerializeField] TextMeshProUGUI _highScoreText;

        [SerializeField] AudioSource _backgroundMusic;

        void Awake()
        {
            _widthSlider.value = PlayerPrefs.GetInt(GridWidthKey, _gridSettings.DefaultGridWidth);
            OnChangeWidth(_widthSlider.value);
            _heightSlider.value = PlayerPrefs.GetInt(GridHeightKey, _gridSettings.DefaultGridHeight);
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
            TimeSpan timeSpan = TimeSpan.FromSeconds(duration);
            _durationText.text = $"Match Duration: {timeSpan:mm':'ss}";
        }

        public void OnPlay()
        {
            _backgroundMusic.Stop();
            SceneManager.LoadScene("Gameplay");
        }
    }
}

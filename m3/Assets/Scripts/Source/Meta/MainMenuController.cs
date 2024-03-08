using OldBard.Services.Match3.Config.Data;
using OldBard.Services.Match3.Grid;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace OldBard.Match3.Meta
{
    /// <summary>
    /// MainMenuController
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        const string GRID_WIDTH_KEY = "GridWidth";
        const string GRID_HEIGHT_KEY = "GridHeight";
        const string TILES_VARIATIONS_KEY = "TilesVariations";
        const string MATCH_DURATION_KEY = "MatchDuration";
        const string HIGH_SCORE_KEY = "HighScore";

        [SerializeField] GameConfig _config;
        [FormerlySerializedAs("_gridSettings"),SerializeField] GridConfig _gridConfig;
        
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
            _widthSlider.value = PlayerPrefs.GetInt(GRID_WIDTH_KEY, _gridConfig.DefaultGridWidth);
            OnChangeWidth(_widthSlider.value);
            _heightSlider.value = PlayerPrefs.GetInt(GRID_HEIGHT_KEY, _gridConfig.DefaultGridHeight);
            OnChangeHeight(_heightSlider.value);
            _variationsSlider.value = PlayerPrefs.GetInt(TILES_VARIATIONS_KEY, _config.NumberOfTileTypes);
            OnChangeVariations(_variationsSlider.value);
            _durationSlider.value = PlayerPrefs.GetInt(MATCH_DURATION_KEY, _config.GameDuration);
            OnChangeDuration(_durationSlider.value);
            
            _highScoreText.text = $"High Score: {PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0)}";
        }

        public void OnChangeWidth(Single width)
        {
            PlayerPrefs.SetInt(GRID_WIDTH_KEY, (int)width);
            _widthText.text = $"Grid Width: {(int)width}";
        }

        public void OnChangeHeight(Single height)
        {
            PlayerPrefs.SetInt(GRID_HEIGHT_KEY, (int)height);
            _heightText.text = $"Grid Height: {(int)height}";
        }

        public void OnChangeVariations(Single variations)
        {
            PlayerPrefs.SetInt(TILES_VARIATIONS_KEY, (int)variations);
            _variationsText.text = $"Tiles Variations: {(int)variations}";
        }

        public void OnChangeDuration(Single duration)
        {
            PlayerPrefs.SetInt(MATCH_DURATION_KEY, (int)duration);
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

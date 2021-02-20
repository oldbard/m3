﻿using System;
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
        const string HighScoreKey = "HighScore";

        [SerializeField] Config _config;
        
        [SerializeField] Slider _widthSlider;
        [SerializeField] Slider _heightSlider;
        [SerializeField] Slider _variationsSlider;
        [SerializeField] Text _widthText;
        [SerializeField] Text _heightText;
        [SerializeField] Text _variationsText;
        [SerializeField] Text _highScoreText;
        [SerializeField] Text _goldText;
        [SerializeField] Text _gemsText;
        [SerializeField] UpgradeButtonController _timerUpdateController;

        [SerializeField] AudioSource _backgroundMusic;
        
        void Start()
        {
            _widthSlider.value = PlayerPrefs.GetInt(GridWidthKey, _config.DefaultGridWidth);
            OnChangeWidth(_widthSlider.value);
            _heightSlider.value = PlayerPrefs.GetInt(GridHeightKey, _config.DefaultGridHeight);
            OnChangeHeight(_heightSlider.value);
            _variationsSlider.value = PlayerPrefs.GetInt(TilesVariationsKey, _config.NumberOfTileTypes);
            OnChangeVariations(_variationsSlider.value);

            _highScoreText.text = $"High Score: {PlayerPrefs.GetInt(HighScoreKey, 0)}";

            RegisterEvents();

            _goldText.text = "0";
            _gemsText.text = "0";
        }

        void OnDestroy()
        {
            UnregisterEvents();
        }

        void RegisterEvents()
        {
            GamePersistentData.Instance.UserData.GoldChanged += OnChangeGold;
            GamePersistentData.Instance.UserData.GemsChanged += OnChangeGems;
            GamePersistentData.Instance.UserData.DataLoaded += OnDataLoaded;
        }

        void UnregisterEvents()
        {
            GamePersistentData.Instance.UserData.GoldChanged -= OnChangeGold;
            GamePersistentData.Instance.UserData.GemsChanged -= OnChangeGems;
            GamePersistentData.Instance.UserData.DataLoaded -= OnDataLoaded;
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

        void OnChangeGold(int amount)
        {
            _goldText.text = amount.ToString();
        }

        void OnChangeGems(int amount)
        {
            _gemsText.text = amount.ToString();
        }

        void OnDataLoaded()
        {
            var catalog = GamePersistentData.Instance.ConfigData.Catalog;

            _timerUpdateController.Init(_config, catalog, OnUpgradeItemButtonClicked);
        }

        void OnUpgradeItemButtonClicked()
        {
            if(GamePersistentData.Instance.UserData.IsUpgradingTimer)
            {
                OnSkipUpgradeItemCalled();
            }
            else
            {
                OnUpgradeItemCalled();
            }
        }

        void OnUpgradeItemCalled()
        {
            GamePersistentData.Instance.UserData.SetTimer(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            _timerUpdateController.ShowUpgrade();
        }

        void OnSkipUpgradeItemCalled()
        {
            GamePersistentData.Instance.UserData.SetTimer(0);
            GamePersistentData.Instance.UserData.SetTimerLevel(GamePersistentData.Instance.UserData.DurationLevel + 1);

            _timerUpdateController.CompleteUpgrade();
        }

        public void OnPlay()
        {
            _backgroundMusic.Stop();
            SceneManager.LoadScene("Gameplay");
        }
    }
}

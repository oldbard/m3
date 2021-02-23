using System;
using GameData;
using GameServices;
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

        [Header("Config")]
        [SerializeField] Config _config;
        
        [Header("UI")]
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
        [SerializeField] Image _loginPanel;
        [SerializeField] Button _loginButton;
        [SerializeField] GameObject _loginControls;
        [SerializeField] Image _leaderboardPanel;
        [SerializeField] Transform _leaderboardControls;

        [Header("Audio")]
        [SerializeField] AudioSource _backgroundMusic;

        ClientManager _clientManager;

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

        UserData UserData
        {
            get
            {
                if (_gamePersistentData == null)
                {
                    _gamePersistentData = Services.Resolve<GamePersistentData>();
                }

                return _gamePersistentData.UserData;
            }
        }

        string _userName;

        void Start()
        {
            _loginPanel.gameObject.SetActive(true);

            _widthSlider.value = PlayerPrefs.GetInt(GridWidthKey, _config.DefaultGridWidth);
            OnChangeWidth(_widthSlider.value);
            _heightSlider.value = PlayerPrefs.GetInt(GridHeightKey, _config.DefaultGridHeight);
            OnChangeHeight(_heightSlider.value);
            _variationsSlider.value = PlayerPrefs.GetInt(TilesVariationsKey, _config.NumberOfTileTypes);
            OnChangeVariations(_variationsSlider.value);

            _highScoreText.text = $"High Score: {PlayerPrefs.GetInt(HighScoreKey, 0)}";

            _clientManager = Services.Resolve<ClientManager>();

            RegisterEvents();

            if(_clientManager.IsLoggedIn)
            {
                _clientManager.LoadData();
                ToggleLoginControler(false);
            }

            _goldText.text = "0";
            _gemsText.text = "0";
        }

        void OnDestroy()
        {
            UnregisterEvents();
        }

        void RegisterEvents()
        {
            _timerUpdateController.TimerShouldHaveEnded += OnTimerShouldHaveEnded;

            UserData.GoldChanged += OnChangeGold;
            UserData.GemsChanged += OnChangeGems;
            UserData.DataLoaded += OnDataLoaded;

            _clientManager.LoggedIn += ToggleLoginControler;
            _clientManager.StartedDurationUpgrade += OnUpgradeTimerStarted;
            _clientManager.UpgradeCompleted += OnUpgradeCompleted;
        }

        void UnregisterEvents()
        {
            _timerUpdateController.TimerShouldHaveEnded -= OnTimerShouldHaveEnded;

            UserData.GoldChanged -= OnChangeGold;
            UserData.GemsChanged -= OnChangeGems;
            UserData.DataLoaded -= OnDataLoaded;

            _clientManager.LoggedIn -= ToggleLoginControler;
            _clientManager.StartedDurationUpgrade -= OnUpgradeTimerStarted;
            _clientManager.UpgradeCompleted -= OnUpgradeCompleted;
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

        void OnChangeGold(uint amount)
        {
            _goldText.text = amount.ToString();
        }

        void OnChangeGems(uint amount)
        {
            _gemsText.text = amount.ToString();
        }

        void OnDataLoaded()
        {
            var catalog = ConfigData.Catalog;

            _timerUpdateController.Init(_config, catalog, OnUpgradeItemButtonClicked);
        }

        void OnTimerShouldHaveEnded()
        {
            _clientManager.VerifyTimerDurationUpgradeFinished();
        }

        void OnUpgradeItemButtonClicked()
        {
            if(UserData.IsUpgradingTimer)
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
            if (UserData.Gold >= ConfigData.Catalog.UpgradeCost)
            {
                _clientManager.StartDurationUpgrade();
            }
            else
            {
                Debug.LogError($"Not enough gold to upgrade duration. Required {ConfigData.Catalog.UpgradeCost}, player has {UserData.Gold}.");
            }
        }

        void OnUpgradeTimerStarted(bool success)
        {
            if (success)
            {
                _timerUpdateController.ShowUpgrade();
            }
        }

        void OnUpgradeCompleted(bool completed)
        {
            if (completed)
                _timerUpdateController.CompleteUpgrade();
        }

        void OnSkipUpgradeItemCalled()
        {
            if (UserData.Gems >= ConfigData.Catalog.SkipUpgradeCost)
            {
                _clientManager.SkipDurationUpgrade();
            }
            else
            {
                Debug.LogError($"Not enough gems to complete upgrade. Required {ConfigData.Catalog.SkipUpgradeCost}, player has {UserData.Gems}.");
            }
        }

        public void OnPlay()
        {
            _backgroundMusic.Stop();
            SceneManager.LoadScene("Gameplay");
        }

        public void OnShowLeaderboard()
        {
            for (int i = 0; i < _leaderboardControls.childCount; i++)
            {
                Destroy(_leaderboardControls.GetChild(i));
            }

            int pos = 1;
            foreach (var player in _gamePersistentData.Leaderboard)
            {
                var controller = GameObject.Instantiate(_config.LeaderboardItem,
                    _leaderboardControls).GetComponent<LeaderboardController>();

                controller.Init(pos, player.Key, player.Value);

                pos++;
            }

            _leaderboardPanel.gameObject.SetActive(true);
        }

        public void OnHideLeaderboard()
        {
            _leaderboardPanel.gameObject.SetActive(false);
        }

        #region Login

        public void ToggleLoginControler(bool enabled)
        {
            _loginPanel.gameObject.SetActive(enabled);
            _loginControls.SetActive(enabled);
            _loginButton.enabled = enabled;
        }

        public void SetUserName(string userName)
        {
            _userName = userName;
        }

        public void OnSetUserNameClicked()
        {
            if(!IsUserNameValid())
            {
                Debug.LogWarning("Player Name is not set!");
            }
            else
            {
                _loginButton.enabled = false;

                _clientManager.SetUserName(_userName);
            }
        }

        bool IsUserNameValid()
        {
            if(string.IsNullOrEmpty(_userName))
                return false;

            if(_userName.Length < 3 || _userName.Length > 20)
                return false;

            return true;
        }

        #endregion
    }
}

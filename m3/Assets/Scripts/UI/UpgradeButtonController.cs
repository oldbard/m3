using GameData;
using GameServices;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UpgradeButtonController : MonoBehaviour
    {
        [SerializeField] Button _button;
        [SerializeField] Text _cost;
        [SerializeField] Text _currentValue;
        [SerializeField] Text _timeAndLevel;
        [SerializeField] Image _currency;
        [SerializeField] GameObject _processingPanel;

        long _initialTime;
        int _duration;

        Config _config;
        CatalogConfigData _catalogItemConfig;

        WaitForSeconds _waitForSecond = new WaitForSeconds(1f);
        Coroutine _timerCoroutine;

        Action _onButtonClickedCallback;
        public Action TimerShouldHaveEnded;

        GamePersistentData _gamePersistentData;

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

        public void Init(Config config, CatalogConfigData catalogConfig, Action onButtonClickedCallback)
        {
            _config = config;
            _catalogItemConfig = catalogConfig;
            _onButtonClickedCallback = onButtonClickedCallback;

            ShowData();
        }

        public void CompleteUpgrade()
        {
            if(_timerCoroutine != null)
                StopCoroutine(_timerCoroutine);

            ShowData();
        }

        void ShowData()
        {
            _cost.text = $"{_catalogItemConfig.UpgradeCost}$";
            _timeAndLevel.text = $"Level: {UserData.DurationLevel}";
            _currentValue.text = $"Duration: {_catalogItemConfig.CurrentValue}";
            _currency.sprite = _config.GoldSprite;

            if (UserData.IsUpgradingTimer)
            {
                ShowUpgrade();
            }
        }

        public void ShowUpgrade()
        {
            _cost.text = $"{_catalogItemConfig.SkipUpgradeCost} Skip";
            _initialTime = UserData.UpgradeStartedTimeStamp;
            _duration = _catalogItemConfig.UpgradeDuration;
            _currency.sprite = _config.GemsSprite;

            _timerCoroutine = StartCoroutine(UpdateTimer());
        }

        IEnumerator UpdateTimer()
        {
            while (true)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var timeDiff = now - _initialTime;

                if (timeDiff > _duration)
                {
                    break;
                }

                var timeSpan = TimeSpan.FromSeconds(_duration - timeDiff);
                _timeAndLevel.text = $"Time Left: {timeSpan:mm':'ss}";

                yield return _waitForSecond;
            }

            Debug.Log("Timer is Over! Yay");
            TimerShouldHaveEnded?.Invoke();
        }

        public void OnClick()
        {
            _onButtonClickedCallback?.Invoke();
        }
    }
}
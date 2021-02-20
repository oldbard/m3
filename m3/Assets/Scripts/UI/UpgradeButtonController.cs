using GameData;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonController : MonoBehaviour
{
    [SerializeField] Button _button;
    [SerializeField] Text _cost;
    [SerializeField] Text _currentValue;
    [SerializeField] Text _timeAndLevel;
    [SerializeField] Image _currency;
    [SerializeField] GameObject _confirmPanel;

    long _initialTime;
    int _duration;

    Config _config;
    CatalogConfigData _catalogItemConfig;

    WaitForSeconds _waitForSecond = new WaitForSeconds(1f);
    Coroutine _timerCoroutine;

    Action _onButtonClickedCallback;

    public void Init(Config config, CatalogConfigData catalogConfig, Action onButtonClickedCallback)
    {
        _config = config;
        _catalogItemConfig = catalogConfig;
        _onButtonClickedCallback = onButtonClickedCallback;

        ShowData();
    }

    public void CompleteUpgrade()
    {
        StopCoroutine(_timerCoroutine);
        ShowData();
    }

    void ShowData()
    {
        var userData = GamePersistentData.Instance.UserData;

        _cost.text = $"{_catalogItemConfig.UpgradeCost}$";
        _timeAndLevel.text = $"Level: {userData.DurationLevel}";
        _currentValue.text = $"Duration: {_catalogItemConfig.CurrentValue}";
        _currency.sprite = _config.GoldSprite;

        if (userData.IsUpgradingTimer)
        {
            ShowUpgrade();
        }
    }

    public void ShowUpgrade()
    {
        var userData = GamePersistentData.Instance.UserData;

        _cost.text = $"{_catalogItemConfig.SkipUpgradeCost} Skip";
        _initialTime = userData.UpgradeStartedTimeStamp;
        _duration = _catalogItemConfig.UpgradeDuration;
        _currency.sprite = _config.GemsSprite;

        _timerCoroutine = StartCoroutine(UpdateTimer());
    }

    IEnumerator UpdateTimer()
    {
        while(true)
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
        CompleteUpgrade();
    }

    public void OnClick()
    {
        _onButtonClickedCallback?.Invoke();
    }
}

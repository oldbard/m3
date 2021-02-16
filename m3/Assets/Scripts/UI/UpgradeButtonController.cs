using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonController : MonoBehaviour
{
    [SerializeField] Button _button;
    [SerializeField] Text _cost;
    [SerializeField] Text _timeAndLevel;
    [SerializeField] GameObject _confirmPanel;

    bool _upgrading;
    long _initialTime;
    int _duration;

    WaitForSeconds _waitForSecond = new WaitForSeconds(1f);

    public void Init(int cost, int level)
    {
        _cost.text = $"{cost}$";
        _timeAndLevel.text = $"Level {level}";
    }

    public void SetupUpgrade(long initialTimestamp, int duration)
    {
        _initialTime = initialTimestamp;
        _duration = duration;
    }

    void Start()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 50;

        SetupUpgrade(now, 100);
        StartCoroutine(UpdateTimer());
    }

    IEnumerator UpdateTimer()
    {
        while (true)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var timeDiff = now - _initialTime;

            if (timeDiff > _duration)
            {
                Debug.Log("Timer is Over! Yay");
                yield break;
            }

            _cost.text = $"{1}";
            _timeAndLevel.text = $" {_duration - timeDiff}";

            yield return _waitForSecond;
        }
    }
}

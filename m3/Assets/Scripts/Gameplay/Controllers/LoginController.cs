using Client;
using Data;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField] Image _loginPanel;
    [SerializeField] Button _loginButton;

    PlayFabLogin _playFabLogin;

    GamePersistentData _gameData;

    string _userName;

    void Awake()
    {
        _loginButton.enabled = false;

        _gameData = new GamePersistentData();

        _playFabLogin = new PlayFabLogin();

        RegisterEvents();

        _playFabLogin.LoginWithDeviceId();
    }

    void RegisterEvents()
    {
        _playFabLogin.GotGameData += OnGotGameData;
        _playFabLogin.PendingUserName += OnPendingUserName;
    }

    void UnregisterEvents()
    {
        _playFabLogin.GotGameData -= OnGotGameData;
        _playFabLogin.PendingUserName -= OnPendingUserName;
    }

    void OnGotGameData(Dictionary<string, string> configData, Dictionary<string, int> currenciesData)
    {
        UnregisterEvents();

        _gameData.ParseData(configData, currenciesData);

        _loginPanel.gameObject.SetActive(false);
    }

    private void OnPendingUserName()
    {
        _loginButton.enabled = true;
    }
    public void SetUserName(string userName)
    {
        _userName = userName;
    }

    public void OnLogin()
    {
        if(!IsUserNameValid())
        {
            Debug.LogWarning("Player Name is not set!");
        }
        else
        {
            _playFabLogin.UpdatePlayerName(_userName);
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
}
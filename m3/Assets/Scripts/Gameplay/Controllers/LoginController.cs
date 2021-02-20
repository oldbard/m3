using Client;
using GameData;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField] Image _loginPanel;
    [SerializeField] Button _loginButton;
    [SerializeField] GameObject _loginControls;

    PlayFabClient _playFabClient;

    GamePersistentData _gameData;

    string _userName;

    void Awake()
    {
        _gameData = new GamePersistentData();

        _playFabClient = new PlayFabClient();

        _loginPanel.gameObject.SetActive(true);

        _ = DoLogin();
    }

    async Task DoLogin()
    {
        try
        {
            var (currenciesData, pendingName) = await _playFabClient.Login();

            var configData = await _playFabClient.GetGameData();

            var catalog = await _playFabClient.GetCatalogItems();

            _gameData.ParseData(configData, currenciesData, catalog);

            _loginPanel.gameObject.SetActive(pendingName);
            _loginControls.SetActive(pendingName);
            _loginButton.enabled = pendingName;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
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
            _ = SetUserName();
        }
    }

    async Task SetUserName()
    {
        _loginButton.enabled = false;

        // TODO: Compare if it is the same with the result?
        await _playFabClient.SetUserName(_userName);

        _loginPanel.gameObject.SetActive(false);
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
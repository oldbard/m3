using GameData;
using Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameServices
{
    // TODO: Add checks for unity editor
    public class ClientManager : IService
    {
        #region Declarations

        readonly List<string> PlayerDataKey = new List<string> { "UpgradeDurationLevel",
            "UpgradeDurationTimestamp" };

        readonly List<string> ContentTitleDataKeys = new List<string>
        {
            "DestroyAnimationTime",
            "DropAnimationTime",
            "FullColorBlinkDelay",
            "GameDuration",
            "HintAnimationTime",
            "HintCycles",
            "PointsPerTile",
            "StartToBlinkDelay",
            "SwapAnimationTime",
            "TimeToShowHint",
            "TimeToShowWarning"
        };

        GamePersistentData _gameData;

        public Action<bool> LoggedIn;
        public Action<bool> StartedDurationUpgrade;
        public Action<bool> UpgradeCompleted;

        #endregion

        #region Init

        public ClientManager()
        {
            Services.RegisterService<ClientManager>(this);
            try
            {
                _gameData = new GamePersistentData();

                _ = Login();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        #endregion

        #region API Async Calls

        #region Login

        async Task Login()
        {
            try
            {
                var loginResult = await new RequestLoginAsync().Process()
                    as LoginResultAsync;

                var gameTitleDataResult = await new RequestTitleDataAsync(ContentTitleDataKeys).Process()
                    as GameTitleDataResultAsync;

                var gamePlayerDataResult = await new RequestPlayerDataAsync(PlayerDataKey).Process()
                    as GamePlayerDataResultAsync;

                var durationUpgradeDataResult = await new RequestGetDurationUpgradeCatalogAsync().Process()
                    as DurationUpgradeCatalogResultAsync;

                _gameData.ParseData(gameTitleDataResult.GameData, gamePlayerDataResult.PlayerData,
                    loginResult.CurrenciesData, durationUpgradeDataResult.DurationUpgradeItem);

                LoggedIn?.Invoke(loginResult.PendingName);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        #endregion

        #region Set Display Name

        public void SetUserName(string userName)
        {
            try
            {
                _ = DoSetUserName(userName);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        async Task DoSetUserName(string userName)
        {
            try
            {
                var setDisplayNameResult = await new RequestSetDisplayNameAsync(userName).Process()
                    as SetDisplayNameResultAsync;

                // TODO: Compare if it is the same with the result?

                LoggedIn?.Invoke(false);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        #endregion

        #endregion

        #region Functions

        public void StartDurationUpgrade()
        {
            _ = DoStartDurationUpgrade();
        }

        async Task DoStartDurationUpgrade()
        {
            try
            {
                var durationUpgradeResult = await new RequestDurationUpgradeAsync().Process()
                    as DurationUpgradeResultAsync;

                if (durationUpgradeResult.Success)
                {
                    var gamePlayerDataResult = await new RequestPlayerDataAsync(PlayerDataKey).Process()
                        as GamePlayerDataResultAsync;

                    _gameData.ParseUpgradablesData(gamePlayerDataResult.PlayerData);

                    var gameCurrenciesDataResult = await new RequestCurrenciesDataAsync().Process()
                        as GameCurrenciesDataResultAsync;

                    _gameData.ParseCurrenciesData(gameCurrenciesDataResult.SC, gameCurrenciesDataResult.HC);
                }
                StartedDurationUpgrade?.Invoke(durationUpgradeResult.Success);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not start timer duration upgrade: {e.Message}");
            }
        }

        public void SkipDurationUpgrade()
        {
            _ = DoSkipDurationUpgrade();
        }

        async Task DoSkipDurationUpgrade()
        {
            try
            {
                var skipDurationUpgradeResult = await new RequestSkipUpgradeAsync().Process()
                    as DurationUpgradeSkipResultAsync;

                if (skipDurationUpgradeResult.Success)
                {
                    var gamePlayerDataResult = await new RequestPlayerDataAsync(PlayerDataKey).Process()
                        as GamePlayerDataResultAsync;

                    _gameData.ParseUpgradablesData(gamePlayerDataResult.PlayerData);

                    var gameCurrenciesDataResult = await new RequestCurrenciesDataAsync().Process()
                        as GameCurrenciesDataResultAsync;

                    _gameData.ParseCurrenciesData(gameCurrenciesDataResult.SC, gameCurrenciesDataResult.HC);
                }
                UpgradeCompleted?.Invoke(skipDurationUpgradeResult.Success);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not start timer duration upgrade: {e.Message}");
            }
        }

        public void VerifyTimerDurationUpgradeFinished()
        {
            _ = DoVerifyTimerDurationUpgradeEnded();
        }

        async Task DoVerifyTimerDurationUpgradeEnded()
        {
            try
            {
                var durationTryFinishUpgradeResult = await new RequestTryFinishUpgradeAsync().Process()
                    as DurationUpgradeTryFinishResultAsync;

                if (durationTryFinishUpgradeResult.Success)
                {
                    var gamePlayerDataResult = await new RequestPlayerDataAsync(PlayerDataKey).Process()
                        as GamePlayerDataResultAsync;

                    _gameData.ParseUpgradablesData(gamePlayerDataResult.PlayerData);
                }
                UpgradeCompleted?.Invoke(durationTryFinishUpgradeResult.Success);
            }
            catch (Exception e)
            {
                Debug.LogError($"Could not start timer duration upgrade: {e.Message}");
            }
        }

        #endregion
    }
}
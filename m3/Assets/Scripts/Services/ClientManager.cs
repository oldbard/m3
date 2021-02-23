using GameData;
using Requests;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace GameServices
{
    /// <summary>
    /// Service which manages the calls to requests to PlayFab
    /// </summary>
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

        readonly string LeaderboardName = "HighScore";

        GamePersistentData _gameData;

        public Action<bool> LoggedIn;
        public Action<bool> StartedDurationUpgrade;
        public Action<bool> UpgradeCompleted;

        public bool IsLoggedIn;

        #endregion

        #region Init

        public ClientManager()
        {
            Services.RegisterService<ClientManager>(this);
            try
            {
                _gameData = new GamePersistentData();

                _ = Login();

                IsLoggedIn = true;
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
            }
        }

        #endregion

        #region API Async Calls

        #region Login

        /// <summary>
        /// Logs in PlayFab
        /// </summary>
        /// <returns></returns>
        async Task Login()
        {
            try
            {
                var loginResult = await new RequestLoginAsync().Process()
                    as LoginResultAsync;

                var getDisplayNameResult = await new RequestGetDisplayNameAsync().Process()
                    as GetDisplayNameResultAsync;

                var pendingName = string.IsNullOrEmpty(getDisplayNameResult.DisplayName);

                await DoLoadData();

                LoggedIn?.Invoke(pendingName);
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        #endregion

        #region LoadData()

        public void LoadData()
        {
            _ = DoLoadData();
        }

        /// <summary>
        /// Loads data from the title and user
        /// </summary>
        /// <returns></returns>
        async Task DoLoadData()
        {
            try
            {
                var gameTitleDataResult = await new RequestTitleDataAsync(ContentTitleDataKeys).Process()
                    as GameTitleDataResultAsync;

                var gamePlayerDataResult = await new RequestPlayerDataAsync(PlayerDataKey).Process()
                    as GamePlayerDataResultAsync;

                var gameCurrenciesDataResult = await new RequestCurrenciesDataAsync().Process()
                    as GameCurrenciesDataResultAsync;

                var durationUpgradeDataResult = await new RequestGetDurationUpgradeCatalogAsync().Process()
                    as DurationUpgradeCatalogResultAsync;

                var leaderboardResult = await new RequestLeaderboardAsync(LeaderboardName).Process()
                    as GameLeaderboardResultAsync;

                _gameData.ParseData(gameTitleDataResult.GameData, gamePlayerDataResult.PlayerData,
                    gameCurrenciesDataResult.SC, gameCurrenciesDataResult.HC,
                    durationUpgradeDataResult.DurationUpgradeItem);

                _gameData.SetLeaderboard(leaderboardResult.Leaderboard);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region Set Display Name

        /// <summary>
        /// Sets the user Display Name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public void SetUserName(string userName)
        {
            try
            {
                _ = DoSetUserName(userName);
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
            }
        }

        /// <summary>
        /// Sets the user Display Name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
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
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
            }
        }

        #endregion

        #region Leaderboard

        /// <summary>
        /// Requests the Leaderboard data
        /// </summary>
        public void GetLeaderboard()
        {
            try
            {
                _ = DoGetLeaderboard(LeaderboardName);
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
            }
        }

        /// <summary>
        /// Requests the Leaderboard data
        /// </summary>
        async Task DoGetLeaderboard(string leaderboardName)
        {
            try
            {
                var leaderboardResult = await new RequestLeaderboardAsync(leaderboardName).Process()
                    as GameLeaderboardResultAsync;

                _gameData.SetLeaderboard(leaderboardResult.Leaderboard);
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError(e.Message);
#endif
            }
        }

        #endregion

        #endregion

        #region Functions

        /// <summary>
        /// Requests the timer upgrade to be started
        /// </summary>
        public void StartDurationUpgrade()
        {
            _ = DoStartDurationUpgrade();
        }

        /// <summary>
        /// Requests the timer upgrade to be started
        /// </summary>
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
#if UNITY_EDITOR
                Debug.LogError($"Could not start timer duration upgrade: {e.Message}");
#endif
            }
        }

        /// <summary>
        /// Requests the timer upgrade to be skipped
        /// </summary>
        public void SkipDurationUpgrade()
        {
            _ = DoSkipDurationUpgrade();
        }

        /// <summary>
        /// Requests the timer upgrade to be skipped
        /// </summary>
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
#if UNITY_EDITOR
                Debug.LogError($"Could not start timer duration upgrade: {e.Message}");
#endif
            }
        }

        /// <summary>
        /// Requests the server to try and finish the timer upgrade
        /// </summary>
        public void VerifyTimerDurationUpgradeFinished()
        {
            _ = DoVerifyTimerDurationUpgradeFinished();
        }

        /// <summary>
        /// Requests the server to try and finish the timer upgrade
        /// </summary>
        async Task DoVerifyTimerDurationUpgradeFinished()
        {
            try
            {
                var tryFinishUpgradeResult = await new RequestTryFinishUpgradeAsync().Process()
                    as DurationUpgradeTryFinishResultAsync;

                if (tryFinishUpgradeResult.Success)
                {
                    var gamePlayerDataResult = await new RequestPlayerDataAsync(PlayerDataKey).Process()
                        as GamePlayerDataResultAsync;

                    _gameData.ParseUpgradablesData(gamePlayerDataResult.PlayerData);
                }
                UpgradeCompleted?.Invoke(tryFinishUpgradeResult.Success);
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError($"Could not start timer duration upgrade: {e.Message}");
#endif
            }
        }

        /// <summary>
        /// Requests the player High Score to be updated
        /// </summary>
        /// <param name="highScore"></param>
        public void UpdateHighScore(uint highScore)
        {
            _ = DoUpdateHighScore(highScore);
        }

        /// <summary>
        /// Requests the player High Score to be updated
        /// </summary>
        /// <param name="highScore"></param>
        async Task DoUpdateHighScore(uint highScore)
        {
            try
            {
                var updateHighScoreResult = await new RequestUpdateHighScoreAsync(highScore).Process()
                    as GameUpdateHighScoreResultAsync;

                if (updateHighScoreResult.Success)
                {
                    var leaderboardResult = await new RequestLeaderboardAsync(LeaderboardName).Process()
                        as GameLeaderboardResultAsync;

                    _gameData.SetLeaderboard(leaderboardResult.Leaderboard);
                }
            }
            catch (Exception e)
            {
#if UNITY_EDITOR
                Debug.LogError($"Could not update high score: {e.Message}");
#endif
            }
        }

        #endregion
    }
}
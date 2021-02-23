using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Requests
{
    public class RequestLeaderboardAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        Dictionary<string, int> _leaderboard;
        string _statistic;

        public RequestLeaderboardAsync(string statistic)
        {
            _statistic = statistic;
        }

        public async Task<IResultAsync> Process()
        {
            RequestLeaderboard();

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new GameLeaderboardResultAsync(_leaderboard);
        }

        void RequestLeaderboard()
        {
            _isProcessing = true;

            var request = new GetLeaderboardRequest
            {
                StartPosition = 0,
                StatisticName = _statistic,
                MaxResultsCount = 10
            };
            PlayFabClientAPI.GetLeaderboard(request, OnGetLeaderboardSuccessful, OnGetLeaderboardFailed);
        }

        void OnGetLeaderboardSuccessful(GetLeaderboardResult result)
        {
#if UNITY_EDITOR
            Debug.Log("Managed to get the leaderboard data");
#endif
            _leaderboard = new Dictionary<string, int>();

            foreach (var player in result.Leaderboard)
            {
                _leaderboard.Add(player.DisplayName, player.StatValue);
            }

            _isProcessing = false;
        }

        void OnGetLeaderboardFailed(PlayFabError error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to get the player data. {error.ErrorMessage}");
        }
    }

    public class GameLeaderboardResultAsync : IResultAsync
    {
        public Dictionary<string, int> Leaderboard;

        public GameLeaderboardResultAsync(Dictionary<string, int> leaderboard)
        {
            Leaderboard = leaderboard;
        }
    }
}
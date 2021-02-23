using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Requests
{
    public class RequestUpdateHighScoreAsync : IRequestAsync
    {
        bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        bool _suceess;

        uint _highScore;

        public RequestUpdateHighScoreAsync(uint highScore)
        {
            _highScore = highScore;
        }

        public async Task<IResultAsync> Process()
        {
            CallUpdateHighScore(_highScore);

            while (_isProcessing)
            {
                await Task.Yield();
            }

            return new GameUpdateHighScoreResultAsync(_suceess);
        }

        void CallUpdateHighScore(uint newHighScore)
        {
            _isProcessing = true;

            var request = new ExecuteCloudScriptRequest
            {
                FunctionName = "UpdateHighScore",
                FunctionParameter = new { highScore = newHighScore }
            };
            
            PlayFabClientAPI.ExecuteCloudScript(request, OnUpdateHighScoreSuccessful, OnUpdateHighScoreFailed);
        }

        void OnUpdateHighScoreSuccessful(ExecuteCloudScriptResult result)
        {
            if (result.Error != null)
            {
                ThrowError(result.Error.Message);
            }

            if (bool.TryParse(result.FunctionResult.ToString(), out var success))
            {
                if (success)
                {
                    UnityEngine.Debug.Log("Updated High Score!");
                    _suceess = true;
                }
            }

            _isProcessing = false;
        }

        void OnUpdateHighScoreFailed(PlayFabError error)
        {
            ThrowError(error.ErrorMessage);
        }

        void ThrowError(string error)
        {
            _isProcessing = false;

            throw new Exception($"Failed to update the High Score. {error}");
        }
    }

    public class GameUpdateHighScoreResultAsync : IResultAsync
    {
        public bool Success;

        public GameUpdateHighScoreResultAsync(bool success)
        {
            Success = success;
        }
    }
}
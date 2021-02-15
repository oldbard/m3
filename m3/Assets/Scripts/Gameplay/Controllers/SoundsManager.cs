using GameData;
using UnityEngine;

namespace Sounds
{
    public class SoundsManager : MonoBehaviour
    {
        [SerializeField] AudioSource _audioSource;

        AudioClip _gameOverClip;
        AudioClip _gameOverHighScoreClip;
        AudioClip _swapClip;
        AudioClip _matchClip;
        AudioClip _timeoutClip;

        public void InitSound(Config config)
        {
            _audioSource.clip = config.GetBGM(Random.Range(0, config.TotalBGMs));
            _audioSource.Play();

            _gameOverClip = config.GameOverSound;
            _gameOverHighScoreClip = config.GameOverHighScoreSound;
            _swapClip = config.SwapSound;
            _matchClip = config.MatchSound;
            _timeoutClip = config.TimeoutSound;
        }

        public void PlaySwapClip()
        {
            _audioSource.PlayOneShot(_swapClip);
        }

        public void PlayMatchClip()
        {
            _audioSource.PlayOneShot(_matchClip);
        }

        public void PlayTimeoutClip()
        {
            _audioSource.PlayOneShot(_timeoutClip);
        }

        public void PlayGameOver(bool highScore)
        {
            _audioSource.Stop();
            _audioSource.PlayOneShot(highScore ? _gameOverHighScoreClip : _gameOverClip);

        }
    }
}
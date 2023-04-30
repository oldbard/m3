using UnityEngine;

namespace OldBard.Services.Match3.Audio
{
    public class AudioService : MonoBehaviour
    {
        [SerializeField] AudioSource _audioSource;

        AudioClip _gameOverClip;
        AudioClip _gameOverHighScoreClip;
        AudioClip _swapClip;
        AudioClip _matchClip;
        AudioClip _timeoutClip;

        public void InitSound(AudioSettings settings)
        {
            _audioSource.clip = settings.GetBGM(Random.Range(0, settings.TotalBGMs));
            _audioSource.Play();

            _gameOverClip = settings.GameOverSound;
            _gameOverHighScoreClip = settings.GameOverHighScoreSound;
            _swapClip = settings.SwapSound;
            _matchClip = settings.MatchSound;
            _timeoutClip = settings.TimeoutSound;
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
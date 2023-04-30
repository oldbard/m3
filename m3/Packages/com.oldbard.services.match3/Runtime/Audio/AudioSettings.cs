using UnityEngine;

namespace OldBard.Services.Match3.Audio
{
    [CreateAssetMenu(fileName = "AudioSettings", menuName = "M3/AudioSettings", order = 1)]
    public class AudioSettings : ScriptableObject
    {
        [Header("BGM")]
        [SerializeField] AudioClip[] _bgms;

        [Header("Clips")]
        public AudioClip GameOverSound;
        public AudioClip GameOverHighScoreSound;
        public AudioClip SwapSound;
        public AudioClip MatchSound;
        public AudioClip TimeoutSound;


        /// <summary>
        /// The total of BGMs available
        /// </summary>
        public int TotalBGMs => _bgms.Length;

        /// <summary>
        /// Gets a BGM based on the given variation
        /// </summary>
        /// <param name="variation">Variation index</param>
        /// <returns>The requested AudioClip</returns>
        public AudioClip GetBGM(int variation)
        {
            return _bgms[variation];
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OldBard.Match3.Gameplay.Views.UI
{
    /// <summary>
    /// GameOverScreenController
    /// </summary>
    public class GameOverScreenController : MonoBehaviour
    {
        [SerializeField] Text _score;

        public void Show(int score, bool highScore)
        {
            _score.text = highScore ? $"New High Score: {score}!" : $"Score: {score}";
            gameObject.SetActive(true);
        }

        public void OnExit()
        {
            SceneManager.LoadScene("MainScene");
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] Text _position;
    [SerializeField] Text _name;
    [SerializeField] Text _score;

    public void Init(int position, string name, int score)
    {
        _position.text = position.ToString();
        _name.text = name;
        _score.text = score.ToString();
    }
}

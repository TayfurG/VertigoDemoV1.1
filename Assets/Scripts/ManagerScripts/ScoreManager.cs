using UnityEngine;
using TMPro;
public class ScoreManager : MonoBehaviour
{
    static public ScoreManager SM { get; set; }
    // Start is called before the first frame update

    [SerializeField]
    private TextMeshProUGUI CurrentScoreText = null;

    [SerializeField]
    private TextMeshProUGUI HighScoreText = null;

    [System.NonSerialized]
    public int CurrentScore = 0;

    [System.NonSerialized]
    public int Highscore = 0;

    void Awake()
    {
        SM = this;
    }

    private void Start()
    {
        Highscore = PlayerPrefs.GetInt("HighScore");
        HighScoreText.text = "Highscore: " + Highscore;
    }

    public void AddScore(int Score)
    {
        CurrentScore += Score;
        CurrentScoreText.text = CurrentScore.ToString();

        if (CurrentScore > Highscore)
        {
            PlayerPrefs.SetInt("HighScore", CurrentScore);
            Highscore = CurrentScore;
            HighScoreText.text = "Highscore: " + CurrentScore;
        }

    }
}

  


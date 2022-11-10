using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {

	
	public Text scoreText;
	public Text highScoreText;

	int score = 0;
	int highScore = 0;

	void Start()
	{
		scoreText.text = score.ToString() +" POINTS";
		highScoreText.text = "HIGHSCORE:" + highScore.ToString();

    }
}

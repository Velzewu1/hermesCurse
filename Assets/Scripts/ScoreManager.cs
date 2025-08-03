using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    [Header("Score Settings")]
    public float scoreMultiplier = 10f;

    private float score;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        score += RoadMover.GlobalSpeed * scoreMultiplier * Time.deltaTime;

        scoreText.text = "Ñ÷¸ò: " + Mathf.FloorToInt(score).ToString();
    }

    public int GetScore() => Mathf.FloorToInt(score);
}

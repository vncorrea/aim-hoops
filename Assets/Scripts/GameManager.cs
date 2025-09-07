// Assets/Scripts/GameManager.cs
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private TMP_Text scoreText;

    private int score;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddPoint(int amount = 1)
    {
        score += amount;
        if (scoreText) scoreText.text = $"Pontos: {score}";
    }

    public void ResetPoints()
    {
        score = 0;
        if (scoreText) scoreText.text = $"Pontos: {score}";
    }
}
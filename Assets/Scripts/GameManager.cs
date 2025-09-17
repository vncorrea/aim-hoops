// Assets/Scripts/GameManager.cs
using System;                // <-- ADICIONE
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Dispara toda vez que pontua
    public static event Action Scored;   // <-- NOVO

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
        Scored?.Invoke();
    }

    public void ResetPoints()
    {
        score = 0;
        if (scoreText) scoreText.text = $"Pontos: {score}";
    }
}

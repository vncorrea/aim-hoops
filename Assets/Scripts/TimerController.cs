using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TimerController : MonoBehaviour
{
    [Header("Configurações do Timer")]
    [SerializeField] private float initialTime = 60f; // 1 minuto inicial
    [SerializeField] private float bonusTimePerScore = 10f; // 10 segundos por cesta
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text gameOverText;
    
    [Header("Configurações de UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float gameOverDisplayTime = 3f; // tempo para mostrar "ACABOU O TEMPO"
    [SerializeField] private string menuSceneName = "Menu"; // nome da cena do menu
    [SerializeField] private int menuSceneIndex = 0; // índice da cena do menu (alternativa)
    
    [Header("Cores")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.yellow; // quando restam 30 segundos
    [SerializeField] private Color dangerColor = Color.red; // quando restam 10 segundos
    
    private float currentTime;
    private bool isGameActive = true;
    private bool gameOverShown = false;
    
    void Start()
    {
        // Inicializa o timer
        currentTime = initialTime;
        
        // Esconde o painel de game over
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Configura o texto inicial
        UpdateTimerDisplay();
        
        // Se inscreve no evento de pontuação
        GameManager.Scored += OnScore;
    }
    
    void OnDestroy()
    {
        // Se desinscreve do evento para evitar vazamentos de memória
        GameManager.Scored -= OnScore;
    }
    
    void Update()
    {
        if (isGameActive && currentTime > 0f)
        {
            // Subtrai o tempo
            currentTime -= Time.deltaTime;
            
            // Atualiza o display
            UpdateTimerDisplay();
            
            // Verifica se o tempo acabou
            if (currentTime <= 0f)
            {
                currentTime = 0f;
                GameOver();
            }
        }
    }
    
    void UpdateTimerDisplay()
    {
        if (timerText == null) return;
        
        // Calcula minutos e segundos
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);
        
        // Formata o texto (MM:SS)
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);
        timerText.text = timeString;
        
        // Muda a cor baseada no tempo restante
        if (currentTime <= 10f)
        {
            timerText.color = dangerColor;
        }
        else if (currentTime <= 30f)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }
    
    void OnScore()
    {
        if (isGameActive)
        {
            // Adiciona tempo bônus
            currentTime += bonusTimePerScore;
            
            // Garante que não passe do tempo máximo (opcional)
            // currentTime = Mathf.Min(currentTime, initialTime * 2f); // máximo 2x o tempo inicial
            
            Debug.Log($"Tempo adicionado! Novo tempo: {currentTime:F1}s");
        }
    }
    
    void GameOver()
    {
        if (gameOverShown) return;
        
        gameOverShown = true;
        isGameActive = false;
        
        Debug.Log("ACABOU O TEMPO!");
        
        // Mostra a mensagem de game over
        ShowGameOverMessage();
        
        // Volta para o menu após um tempo
        Invoke(nameof(ReturnToMenu), gameOverDisplayTime);
    }
    
    void ShowGameOverMessage()
    {
        if (gameOverText != null)
        {
            gameOverText.text = "ACABOU O TEMPO!";
            gameOverText.gameObject.SetActive(true);
        }
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
    
    void ReturnToMenu()
    {
        Debug.Log("Voltando para o menu...");
        
        // Tenta carregar por nome primeiro
        if (!string.IsNullOrEmpty(menuSceneName))
        {
            try
            {
                SceneManager.LoadScene(menuSceneName);
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Não foi possível carregar a cena '{menuSceneName}': {e.Message}");
            }
        }
        
        // Se falhar, tenta carregar por índice
        try
        {
            SceneManager.LoadScene(menuSceneIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Não foi possível carregar a cena com índice {menuSceneIndex}: {e.Message}");
            Debug.LogError("Verifique se as cenas estão adicionadas ao Build Settings!");
        }
    }
    
    // Métodos públicos para controle
    public void AddTime(float timeToAdd)
    {
        if (isGameActive)
        {
            currentTime += timeToAdd;
            Debug.Log($"Tempo adicionado manualmente: {timeToAdd}s. Novo tempo: {currentTime:F1}s");
        }
    }
    
    public void SetTime(float newTime)
    {
        if (isGameActive)
        {
            currentTime = newTime;
            UpdateTimerDisplay();
        }
    }
    
    public float GetCurrentTime()
    {
        return currentTime;
    }
    
    public bool IsGameActive()
    {
        return isGameActive;
    }
    
    public void PauseGame()
    {
        isGameActive = false;
    }
    
    public void ResumeGame()
    {
        if (!gameOverShown)
        {
            isGameActive = true;
        }
    }
    
    public void RestartTimer()
    {
        currentTime = initialTime;
        isGameActive = true;
        gameOverShown = false;
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        
        UpdateTimerDisplay();
    }
}

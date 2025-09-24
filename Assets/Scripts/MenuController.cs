// Assets/Scripts/MenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Configurações de Cena")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private int gameSceneIndex = 1; // índice da cena do jogo
    
    public void OnStartGame()
    {
        
        // Tenta carregar por nome primeiro
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            try
            {
                SceneManager.LoadScene(gameSceneName);
                return;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Não foi possível carregar a cena '{gameSceneName}': {e.Message}");
            }
        }
        
        // Se falhar, tenta carregar por índice
        try
        {
            SceneManager.LoadScene(gameSceneIndex);
        }
        catch (System.Exception e)
        {
        }
    }
    
    public void OnQuitGame()
    {
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public void OnSettings()
    {
        // Implementar menu de configurações se necessário
    }
}
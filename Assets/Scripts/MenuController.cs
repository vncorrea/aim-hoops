// Assets/Scripts/MenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnStartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
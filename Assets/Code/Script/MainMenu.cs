using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Loadgame scene
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        // Exit the application
        Application.Quit();
        Debug.Log("Game Quit");
    }
}
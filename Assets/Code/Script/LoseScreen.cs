using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreen : MonoBehaviour
{
    private static bool isSceneUnloading = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isSceneUnloading = false;
        Debug.Log($"[LoseScreen] Scene '{scene.name}' loaded.");
    }

    public void Retry()
    {
        isSceneUnloading = true;
        Time.timeScale = 1f;

        if (AudioManager.instance != null)
            AudioManager.instance.ResetMusic();

        PersistentCleanup();
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        PersistentCleanup();
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    // New quit button functionality
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        
        // For standalone builds
        Application.Quit();
        
        // For editor testing
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    /// <summary>
    /// Cleans or resets all persistent managers so replay works without broken state.
    /// </summary>
    private void PersistentCleanup()
    {
        Scene dontDestroyScene = SceneManager.GetSceneByName("DontDestroyOnLoad");
        if (!dontDestroyScene.IsValid()) return;

        foreach (GameObject go in dontDestroyScene.GetRootGameObjects())
        {
            if (go == null) continue;

            if (go == AudioManager.instance?.gameObject) continue;

            if (go.TryGetComponent(out LevelManager levelManager))
            {
                levelManager.ResetState();
                continue;
            }

            if (go.CompareTag("Enemy") || go.name.Contains("EnemyPool"))
            {
                Destroy(go);
                continue;
            }

            if (go.name.Contains("Popup") || go.name.Contains("UI"))
            {
                Destroy(go);
                continue;
            }
        }
    }

    public static bool IsSceneUnloading()
    {
        return isSceneUnloading;
    }
}
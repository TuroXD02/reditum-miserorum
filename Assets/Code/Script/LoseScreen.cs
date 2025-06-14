using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreen : MonoBehaviour
{
    // Static flag to indicate scene unloading or resetting
    private static bool isSceneUnloading = false;

    private void OnEnable()
    {
        // SceneManager does NOT have sceneUnloading event, so remove this or replace with sceneUnloaded if needed
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    // Called after a scene unloads
    private void OnSceneUnloaded(Scene scene)
    {
        isSceneUnloading = true;
    }

    public void Retry()
    {
        // Mark unloading flag manually BEFORE loading new scene
        isSceneUnloading = true;

        // Reset time scale
        Time.timeScale = 1f;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.ResetMusic();
        }

        CleanupPersistentObjects();

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void CleanupPersistentObjects()
    {
        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.scene.name != "DontDestroyOnLoad") continue;

            if (go.CompareTag("Enemy") || go.name == "EnemyPool")
            {
                Destroy(go);
            }
        }
    }

    // Static method other scripts can use to check unload status
    public static bool IsSceneUnloading()
    {
        return isSceneUnloading;
    }
}
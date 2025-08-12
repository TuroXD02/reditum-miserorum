using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        PersistentCleanup();
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }

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

            Destroy(go);
        }
    }
}
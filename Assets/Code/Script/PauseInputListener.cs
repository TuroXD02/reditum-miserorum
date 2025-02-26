using UnityEngine;

public class PauseInputListener : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu; // Reference to the PauseMenu GameObject.

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);  // Activate the menu.
        }
    }
}
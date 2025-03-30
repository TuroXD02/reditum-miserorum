using UnityEngine;
using UnityEngine.UI;

public class PausePopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button openButton;    // Button to open the pause popup.
    [SerializeField] private Button closeButton;   // Button inside the popup to close it.
    [SerializeField] private GameObject popupPanel; // The popup panel that appears when pausing.

    private void Start()
    {
        // Ensure the popup panel is initially inactive.
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("PausePopup: popupPanel is not assigned!");
        }

        // Add onClick listeners to the buttons.
        if (openButton != null)
        {
            openButton.onClick.AddListener(OpenPopup);
        }
        else
        {
            Debug.LogWarning("PausePopup: openButton is not assigned!");
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
        else
        {
            Debug.LogWarning("PausePopup: closeButton is not assigned!");
        }
    }

    /// <summary>
    /// Activates the popup and pauses the game.
    /// </summary>
    public void OpenPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("Pause popup opened; game paused.");
        }
    }

    /// <summary>
    /// Deactivates the popup. 
    /// According to your requirement, the game remains paused.
    /// </summary>
    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            // Keep the game paused. (If you want to resume, set Time.timeScale = 1f here.)
            Time.timeScale = 0f;
            Debug.Log("Pause popup closed; game remains paused.");
        }
    }
}

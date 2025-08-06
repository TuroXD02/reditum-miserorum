using UnityEngine;
using UnityEngine.UI;

public class PausePopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject popupPanel;

    private float storedSpeed = 1f;

    private void Start()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
        else
            Debug.LogWarning("PausePopup: popupPanel is not assigned!");

        if (openButton != null)
            openButton.onClick.AddListener(OpenPopup);
        else
            Debug.LogWarning("PausePopup: openButton is not assigned!");

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
        else
            Debug.LogWarning("PausePopup: closeButton is not assigned!");
    }

    private void Update()
    {
        // Pressing Spacebar toggles pause/resume without showing the panel
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.timeScale > 0f)
            {
                storedSpeed = Time.timeScale;
                Time.timeScale = 0f;
                Debug.Log("Game paused via Spacebar.");
            }
            else
            {
                Time.timeScale = storedSpeed;
                Debug.Log($"Game resumed via Spacebar at {storedSpeed}x.");
            }
        }
    }

    public void OpenPopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            storedSpeed = Time.timeScale > 0f ? Time.timeScale : storedSpeed;
            Time.timeScale = 0f;
            Debug.Log("Pause popup opened; game paused.");
        }
    }

    public void ClosePopup()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            Time.timeScale = storedSpeed;
            Debug.Log($"Pause popup closed; game resumed at {storedSpeed}x.");
        }
    }
}
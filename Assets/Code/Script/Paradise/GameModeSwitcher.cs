using UnityEngine;
using UnityEngine.UI;

public class GameModeSwitcher : MonoBehaviour
{
    [Header("UI Toggle Button")]
    public Button toggleModeButton;

    [Header("Paradise Layer GameObject")]
    public GameObject paradiseLayer;

    private bool isInParadise = false;

    void Start()
    {
        if (toggleModeButton != null)
            toggleModeButton.onClick.AddListener(SwitchMode);

        if (paradiseLayer != null)
            paradiseLayer.SetActive(false); // Hide paradise by default
    }

    void SwitchMode()
    {
        isInParadise = !isInParadise;

        // Show or hide the paradise layer
        if (paradiseLayer != null)
            paradiseLayer.SetActive(isInParadise);

        // Pause or resume the game
        Time.timeScale = isInParadise ? 0f : 1f;
    }
}
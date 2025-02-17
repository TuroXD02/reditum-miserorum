using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicSettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider volumeSlider;         // Assign via the Inspector.
    [SerializeField] private TextMeshProUGUI volumeText;    // Optional: displays volume percentage.
    [SerializeField] private Button closeButton;            // Button to close the settings menu.

    private void Start()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("MusicSettingsMenu: AudioManager instance not found!");
            return;
        }
        
        // Initialize the slider with the current volume.
        volumeSlider.value = AudioManager.instance.targetVolume;
        UpdateVolumeText(volumeSlider.value);
        
        // Add a listener to update music volume in real time.
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        
        // Add a listener to close the menu.
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMenu);
    }

    private void OnVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetVolume(value);
            UpdateVolumeText(value);
        }
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
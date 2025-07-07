using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicSettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError("MusicSettingsMenu: AudioManager not found!");
            return;
        }

        float savedVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        volumeSlider.value = savedVolume;
        UpdateVolumeText(savedVolume);

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMenu);
    }

    private void OnVolumeChanged(float value)
    {
        AudioManager.instance.SetVolumeLinear(value);
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float value)
    {
        if (volumeText != null)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            volumeText.text = percent + "%";
        }
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
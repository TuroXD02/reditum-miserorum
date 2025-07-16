using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DisplaySettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;

    private Resolution[] availableResolutions;

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettingsPanel);

        PopulateResolutionDropdown();
        PopulateScreenModeDropdown();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
    }

    private void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void PopulateResolutionDropdown()
    {
        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string option = $"{availableResolutions[i].width} x {availableResolutions[i].height}";
            options.Add(option);

            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void PopulateScreenModeDropdown()
    {
        screenModeDropdown.ClearOptions();
        List<string> options = new List<string> { "Fullscreen", "Windowed" };

        screenModeDropdown.AddOptions(options);
        screenModeDropdown.value = Screen.fullScreen ? 0 : 1;
        screenModeDropdown.RefreshShownValue();
    }

    private void SetResolution(int index)
    {
        Resolution res = availableResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    private void SetScreenMode(int index)
    {
        bool fullScreen = index == 0;
        Screen.fullScreen = fullScreen;

        // Also reapply resolution to ensure mode change takes effect
        int resIndex = resolutionDropdown.value;
        Resolution res = availableResolutions[resIndex];
        Screen.SetResolution(res.width, res.height, fullScreen);
    }
}

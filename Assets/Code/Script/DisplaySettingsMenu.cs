using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DisplaySettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button settingsButton;
    [SerializeField] private GameObject settingsPanel;

    [Header("Escape / Back")]
    [SerializeField] private Button backButton; // <-- ADD THIS IN THE INSPECTOR

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown displayModeDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Header("Dropdown Resize Settings")]
    [SerializeField] private int maxVisibleItems = 5;
    [SerializeField] private float itemHeight = 60f;

    private Resolution[] availableResolutions;

    private void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettingsPanel);

        if (backButton != null)
            backButton.onClick.AddListener(CloseSettingsPanel);

        SetupResolutionDropdown();
        SetupDisplayModeDropdown();

        if (displayModeDropdown != null)
        {
            displayModeDropdown.onValueChanged.AddListener(OnDisplayModeChanged);
            displayModeDropdown.onValueChanged.AddListener(_ => StartCoroutine(ResizeDropdownNextFrame(displayModeDropdown)));
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            resolutionDropdown.onValueChanged.AddListener(_ => StartCoroutine(ResizeDropdownNextFrame(resolutionDropdown)));
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && settingsPanel.activeSelf)
        {
            CloseSettingsPanel();
        }
    }

    private void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    private void CloseSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        availableResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> resolutionOptions = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            string resLabel = $"{availableResolutions[i].width} x {availableResolutions[i].height}";
            resolutionOptions.Add(resLabel);

            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void SetupDisplayModeDropdown()
    {
        if (displayModeDropdown == null) return;

        displayModeDropdown.ClearOptions();

        List<string> modes = new List<string>
        {
            "Windowed",
            "Borderless Windowed",
            "Fullscreen"
        };

        displayModeDropdown.AddOptions(modes);

        FullScreenMode currentMode = Screen.fullScreenMode;
        int selectedIndex = 0;

        if (currentMode == FullScreenMode.Windowed)
            selectedIndex = 0;
        else if (currentMode == FullScreenMode.MaximizedWindow)
            selectedIndex = 1;
        else if (currentMode == FullScreenMode.FullScreenWindow)
            selectedIndex = 2;

        displayModeDropdown.value = selectedIndex;
        displayModeDropdown.RefreshShownValue();
    }

    private void OnDisplayModeChanged(int index)
    {
        FullScreenMode selectedMode = FullScreenMode.Windowed;

        switch (index)
        {
            case 0: selectedMode = FullScreenMode.Windowed; break;
            case 1: selectedMode = FullScreenMode.MaximizedWindow; break;
            case 2: selectedMode = FullScreenMode.FullScreenWindow; break;
        }

        ApplyDisplaySettings(resolutionDropdown.value, selectedMode);
    }

    private void OnResolutionChanged(int index)
    {
        FullScreenMode currentMode = Screen.fullScreenMode;
        ApplyDisplaySettings(index, currentMode);
    }

    private void ApplyDisplaySettings(int resolutionIndex, FullScreenMode mode)
    {
        if (resolutionIndex < 0 || resolutionIndex >= availableResolutions.Length)
            return;

        Resolution res = availableResolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, mode);
    }

    private IEnumerator ResizeDropdownNextFrame(TMP_Dropdown dropdown)
    {
        yield return new WaitForEndOfFrame();

        GameObject dropdownList = GameObject.Find("Dropdown List");
        if (dropdownList == null) yield break;

        ScrollRect scrollRect = dropdownList.GetComponentInChildren<ScrollRect>();
        if (scrollRect == null) yield break;

        RectTransform listRect = dropdownList.GetComponent<RectTransform>();
        RectTransform viewportRect = scrollRect.viewport;
        RectTransform contentRect = scrollRect.content;

        int optionCount = dropdown.options.Count;
        int visibleCount = Mathf.Min(optionCount, maxVisibleItems);

        float fullItemHeight = itemHeight;
        float newViewportHeight = visibleCount * fullItemHeight;
        float totalContentHeight = optionCount * fullItemHeight;
        float outerHeight = newViewportHeight + 30f; // Fix for cut item

        if (viewportRect != null)
            viewportRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newViewportHeight);

        if (contentRect != null)
            contentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalContentHeight);

        if (listRect != null)
            listRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, outerHeight);
    }
}

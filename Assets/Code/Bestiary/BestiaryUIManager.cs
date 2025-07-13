using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestiaryUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject bestiaryMenuPanel;        // The main Bestiary UI panel
    [SerializeField] private Transform gridContainer;             // Grid container for entry icons
    [SerializeField] private Transform detailPanelParent;         // Parent for detail panel

    [Header("Prefabs")]
    [SerializeField] private GameObject entryIconPrefab;          // Prefab for enemy entry icon
    [SerializeField] private GameObject detailPanelPrefab;        // Prefab for detail display

    [Header("Buttons")]
    [SerializeField] private Button closeButton;                  // Close button (optional)

    private List<BestiaryEntry> discoveredEnemies = new();        // List of discovered enemies
    private int currentIndex = -1;                                // Current index in discoveredEnemies
    private GameObject currentDetailPanel;                        // Current detail panel instance

    private void Start()
    {
        // Hide Bestiary at start
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(false);

        // Hook up close button if assigned
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseBestiary);
    }

    /// <summary>
    /// Opens the Bestiary UI and pauses the game.
    /// </summary>
    public void OpenBestiary()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(true);

        RefreshBestiary();
        Time.timeScale = 0f; // Pause the game
    }

    /// <summary>
    /// Closes the Bestiary UI but keeps the game paused.
    /// </summary>
    public void CloseBestiary()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(false);

        ClearGrid();
        CloseDetailPanel();

        // Do NOT unpause the game
        // Time.timeScale = 1f;
    }

    /// <summary>
    /// Refreshes the Bestiary grid with discovered entries.
    /// </summary>
    private void RefreshBestiary()
    {
        ClearGrid();
        discoveredEnemies.Clear();

        foreach (var entry in BestiaryManager.Instance.GetAllEntries())
        {
            if (!BestiaryManager.Instance.IsDiscovered(entry.enemyID))
                continue;

            GameObject iconObj = Instantiate(entryIconPrefab, gridContainer);
            var icon = iconObj.GetComponent<BestiaryUIEntry>();

            if (icon != null)
            {
                icon.Setup(entry, true, () => OnEntryClicked(entry));
                discoveredEnemies.Add(entry);
            }
        }
    }

    /// <summary>
    /// Clears all icons from the grid.
    /// </summary>
    private void ClearGrid()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Called when a Bestiary entry is clicked.
    /// </summary>
    private void OnEntryClicked(BestiaryEntry entry)
    {
        currentIndex = discoveredEnemies.IndexOf(entry);
        ShowDetailPanel(entry);
    }

    /// <summary>
    /// Displays the detail panel for a selected enemy.
    /// </summary>
    private void ShowDetailPanel(BestiaryEntry entry)
    {
        CloseDetailPanel();

        currentDetailPanel = Instantiate(detailPanelPrefab, detailPanelParent);
        var detail = currentDetailPanel.GetComponent<BestiaryDetailPanel>();

        if (detail != null)
        {
            detail.Setup(entry, GoToPrevious, GoToNext);
            detail.SetCloseAction(CloseDetailPanel);
        }
    }

    /// <summary>
    /// Destroys the current detail panel if it exists.
    /// </summary>
    private void CloseDetailPanel()
    {
        if (currentDetailPanel != null)
        {
            Destroy(currentDetailPanel);
            currentDetailPanel = null;
        }
    }

    /// <summary>
    /// Navigates to the previous entry in the discovered list.
    /// </summary>
    private void GoToPrevious()
    {
        if (discoveredEnemies.Count == 0) return;

        currentIndex = (currentIndex - 1 + discoveredEnemies.Count) % discoveredEnemies.Count;
        ShowDetailPanel(discoveredEnemies[currentIndex]);
    }

    /// <summary>
    /// Navigates to the next entry in the discovered list.
    /// </summary>
    private void GoToNext()
    {
        if (discoveredEnemies.Count == 0) return;

        currentIndex = (currentIndex + 1) % discoveredEnemies.Count;
        ShowDetailPanel(discoveredEnemies[currentIndex]);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestiaryUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject bestiaryMenuPanel;      // The main bestiary menu panel
    [SerializeField] private Transform gridContainer;           // Where enemy icons are spawned
    [SerializeField] private Transform detailPanelParent;       // Where the detail panel appears

    [Header("Prefabs")]
    [SerializeField] private GameObject entryIconPrefab;        // Icon button prefab
    [SerializeField] private GameObject detailPanelPrefab;      // Enemy info panel prefab

    [Header("Optional Buttons")]
    [SerializeField] private Button closeButton;                // Button to close the entire bestiary menu

    private List<BestiaryEntry> discoveredEnemies = new();      // List of discovered enemies
    private int currentIndex = -1;                              // Currently viewed enemy in detail panel
    private GameObject currentDetailPanel;                      // The instantiated detail panel

    private void Start()
    {
        // Start with the bestiary menu closed
        bestiaryMenuPanel.SetActive(false);

        // Hook up close button if assigned
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseBestiary);
        }
    }

    /// <summary>
    /// Opens the bestiary menu and loads discovered enemy entries.
    /// </summary>
    public void OpenBestiary()
    {
        bestiaryMenuPanel.SetActive(true);
        RefreshBestiary();
    }

    /// <summary>
    /// Closes the bestiary menu and cleans up UI.
    /// </summary>
    public void CloseBestiary()
    {
        bestiaryMenuPanel.SetActive(false);
        ClearGrid();
        CloseDetailPanel();
    }

    /// <summary>
    /// Regenerates the grid of discovered enemies.
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
            icon.Setup(entry, true, () => OnEntryClicked(entry));
            discoveredEnemies.Add(entry);
        }
    }

    /// <summary>
    /// Destroys all icon buttons from the grid.
    /// </summary>
    private void ClearGrid()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Handles clicking on an enemy icon.
    /// </summary>
    private void OnEntryClicked(BestiaryEntry entry)
    {
        currentIndex = discoveredEnemies.IndexOf(entry);
        ShowDetailPanel(entry);
    }

    /// <summary>
    /// Instantiates and displays the detail panel for the selected enemy.
    /// </summary>
    private void ShowDetailPanel(BestiaryEntry entry)
    {
        CloseDetailPanel();

        currentDetailPanel = Instantiate(detailPanelPrefab, detailPanelParent);
        var detail = currentDetailPanel.GetComponent<BestiaryDetailPanel>();
        detail.Setup(entry, GoToPrevious, GoToNext);
        detail.SetCloseAction(CloseDetailPanel);
    }

    /// <summary>
    /// Destroys the currently open detail panel.
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
    /// Navigates to the previous discovered enemy in the detail panel.
    /// </summary>
    private void GoToPrevious()
    {
        if (discoveredEnemies.Count == 0) return;

        currentIndex = (currentIndex - 1 + discoveredEnemies.Count) % discoveredEnemies.Count;
        ShowDetailPanel(discoveredEnemies[currentIndex]);
    }

    /// <summary>
    /// Navigates to the next discovered enemy in the detail panel.
    /// </summary>
    private void GoToNext()
    {
        if (discoveredEnemies.Count == 0) return;

        currentIndex = (currentIndex + 1) % discoveredEnemies.Count;
        ShowDetailPanel(discoveredEnemies[currentIndex]);
    }
}

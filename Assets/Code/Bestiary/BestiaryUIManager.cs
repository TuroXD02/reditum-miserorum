using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestiaryUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject bestiaryMenuPanel;        // Main menu panel
    [SerializeField] private Transform gridContainer;             // Where enemy icons go
    [SerializeField] private Transform detailPanelParent;         // Where detail panel appears

    [Header("Prefabs")]
    [SerializeField] private GameObject entryIconPrefab;          // Icon prefab for enemy entry
    [SerializeField] private GameObject detailPanelPrefab;        // Detail panel prefab

    [Header("Buttons")]
    [SerializeField] private Button closeButton;                  // Close button (optional)

    private List<BestiaryEntry> discoveredEnemies = new();        // Enemies the player discovered
    private int currentIndex = -1;                                // Currently viewed entry
    private GameObject currentDetailPanel;                        // Instance of the detail panel

    private void Start()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseBestiary);
    }

    /// <summary>
    /// Open the bestiary and pause the game.
    /// </summary>
    public void OpenBestiary()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(true);

        RefreshBestiary();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Close the bestiary and resume the game.
    /// </summary>
    public void CloseBestiary()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(false);

        ClearGrid();
        CloseDetailPanel();
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Clears the UI grid and repopulates it with discovered enemies.
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
    /// Destroys all children of the grid container.
    /// </summary>
    private void ClearGrid()
    {
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Called when an enemy icon is clicked.
    /// </summary>
    private void OnEntryClicked(BestiaryEntry entry)
    {
        currentIndex = discoveredEnemies.IndexOf(entry);
        ShowDetailPanel(entry);
    }

    /// <summary>
    /// Shows the detail panel for the given enemy.
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
    /// Closes and destroys the current detail panel.
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
    /// Navigate to the previous discovered enemy.
    /// </summary>
    private void GoToPrevious()
    {
        if (discoveredEnemies.Count == 0) return;

        currentIndex = (currentIndex - 1 + discoveredEnemies.Count) % discoveredEnemies.Count;
        ShowDetailPanel(discoveredEnemies[currentIndex]);
    }

    /// <summary>
    /// Navigate to the next discovered enemy.
    /// </summary>
    private void GoToNext()
    {
        if (discoveredEnemies.Count == 0) return;

        currentIndex = (currentIndex + 1) % discoveredEnemies.Count;
        ShowDetailPanel(discoveredEnemies[currentIndex]);
    }
}

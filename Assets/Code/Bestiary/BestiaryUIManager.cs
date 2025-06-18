using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestiaryUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject bestiaryMenuPanel;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private GameObject entryIconPrefab;
    [SerializeField] private GameObject detailPanelPrefab;
    [SerializeField] private Transform detailPanelParent;
    [SerializeField] private Button closeButton; // ← Optional close button if needed

    private List<BestiaryEntry> discoveredEnemies = new();
    private int currentIndex = -1;
    private GameObject currentDetailPanel;

    private void Start()
    {
        bestiaryMenuPanel.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseBestiary);
    }

    public void OpenBestiary()
    {
        bestiaryMenuPanel.SetActive(true);
        RefreshBestiary(); // ← Must be here
    }

    public void CloseBestiary()
    {
        bestiaryMenuPanel.SetActive(false);
        ClearGrid();
        CloseDetailPanel();
    }

    private void RefreshBestiary()
    {
        ClearGrid();
        discoveredEnemies.Clear();

        foreach (var entry in BestiaryManager.Instance.GetAllEntries())
        {
            bool isDiscovered = BestiaryManager.Instance.IsDiscovered(entry.enemyID);
            if (!isDiscovered)
                continue;

            GameObject go = Instantiate(entryIconPrefab, gridContainer);
            var icon = go.GetComponent<BestiaryUIEntry>();
            icon.Setup(entry, isDiscovered, () => OnEntryClicked(entry));

            discoveredEnemies.Add(entry);
            Debug.Log($"[BestiaryUIManager] Discovered: {entry.enemyID}");
        }
    }

    private void ClearGrid()
    {
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);
    }

    private void OnEntryClicked(BestiaryEntry entry)
    {
        currentIndex = discoveredEnemies.IndexOf(entry);
        ShowDetailPanel(entry);
    }

    private void ShowDetailPanel(BestiaryEntry entry)
    {
        CloseDetailPanel();

        currentDetailPanel = Instantiate(detailPanelPrefab, detailPanelParent);
        var detail = currentDetailPanel.GetComponent<BestiaryDetailPanel>();
        detail.Setup(entry, GoToPrevious, GoToNext);
    }

    private void CloseDetailPanel()
    {
        if (currentDetailPanel != null)
            Destroy(currentDetailPanel);
    }

    private void GoToPrevious()
    {
        if (discoveredEnemies.Count == 0) return;

        currentIndex = (currentIndex - 1 + discoveredEnemies.Count) % discoveredEnemies.Count;
        ShowDetailPanel(discoveredEnemies[currentIndex]);
    }

    private void GoToNext()
    {
        if (discoveredEnemies.Count == 0) return;

        currentIndex = (currentIndex + 1) % discoveredEnemies.Count;
        ShowDetailPanel(discoveredEnemies[currentIndex]);
    }
}

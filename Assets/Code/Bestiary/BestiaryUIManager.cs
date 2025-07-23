using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BestiaryUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject bestiaryMenuPanel;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private Transform detailPanelParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject entryIconPrefab;
    [SerializeField] private GameObject detailPanelPrefab;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;

    [Header("Notification System")]
    [SerializeField] private GameObject notificationObject; // UI notification object (e.g., Image)
    [SerializeField] private Animator notificationAnimator; // Animator on the UI object
    [SerializeField] private string notificationTrigger = "PlayNotification";

    private List<BestiaryEntry> discoveredEnemies = new();
    private int currentIndex = -1;
    private GameObject currentDetailPanel;

    private HashSet<string> previouslySeenEntries = new();

    private void Start()
    {
        // Hide panels and notification on start
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(false);

        if (notificationObject != null)
            notificationObject.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseBestiary);

        // Cache previously discovered enemies
        foreach (var entry in BestiaryManager.Instance.GetAllEntries())
        {
            if (BestiaryManager.Instance.IsDiscovered(entry.enemyID))
                previouslySeenEntries.Add(entry.enemyID);
        }
    }

    private void Update()
    {
        CheckForNewEntries();
    }

    public void OpenBestiary()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(true);

        Time.timeScale = 0f;

        RefreshBestiary();
        CloseDetailPanel();

        // Hide notification when menu is opened
        if (notificationObject != null)
            notificationObject.SetActive(false);
    }

    public void CloseBestiary()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(false);

        Time.timeScale = 1f;
        ClearGrid();
        CloseDetailPanel();
    }

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

            previouslySeenEntries.Add(entry.enemyID); // Mark as seen now
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

        if (detail != null)
        {
            detail.Setup(entry, GoToPrevious, GoToNext);
            detail.SetCloseAction(CloseDetailPanel);
        }
    }

    private void CloseDetailPanel()
    {
        if (currentDetailPanel != null)
        {
            Destroy(currentDetailPanel);
            currentDetailPanel = null;
        }
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

    /// <summary>
    /// Show notification only if there's something newly discovered.
    /// </summary>
    private void CheckForNewEntries()
    {
        bool foundNew = false;

        foreach (var entry in BestiaryManager.Instance.GetAllEntries())
        {
            if (BestiaryManager.Instance.IsDiscovered(entry.enemyID) &&
                !previouslySeenEntries.Contains(entry.enemyID))
            {
                foundNew = true;
                break;
            }
        }

        if (notificationObject == null) return;

        if (foundNew)
        {
            if (!notificationObject.activeSelf)
                notificationObject.SetActive(true);

            if (notificationAnimator != null)
                notificationAnimator.SetTrigger(notificationTrigger);
        }
        else
        {
            if (notificationObject.activeSelf)
                notificationObject.SetActive(false);
        }
    }
}

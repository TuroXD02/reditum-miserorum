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
    [SerializeField] private GameObject notificationObject;
    [SerializeField] private Animator notificationAnimator;
    [SerializeField] private string notificationTrigger = "PlayNotification";

    private List<BestiaryEntry> discoveredEnemies = new();
    private int currentIndex = -1;
    private GameObject currentDetailPanel;
    private HashSet<string> previouslySeenEntries = new();

    private float storedSpeed = 1f;

    private void Start()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(false);

        if (notificationObject != null)
            notificationObject.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseBestiary);

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

        storedSpeed = Time.timeScale > 0f ? Time.timeScale : storedSpeed;
        Time.timeScale = 0f;

        RefreshBestiary();
        CloseDetailPanel();

        if (notificationObject != null)
            notificationObject.SetActive(false);
    }

    public void CloseBestiary()
    {
        if (bestiaryMenuPanel != null)
            bestiaryMenuPanel.SetActive(false);

        Time.timeScale = storedSpeed;
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

            previouslySeenEntries.Add(entry.enemyID);
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

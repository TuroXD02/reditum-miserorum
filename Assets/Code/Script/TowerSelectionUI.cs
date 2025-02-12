using UnityEngine;
using UnityEngine.UI;

public class TowerSelectionUI : MonoBehaviour
{
    // The turret prefab that should appear as a ghost when this button is clicked.
    [SerializeField] private GameObject turretPrefab;

    // Get the UI Button component.
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnSelectTower);
        }
        else
        {
            Debug.LogError("TowerSelectionUI: No Button component found on " + gameObject.name);
        }
    }

    /// <summary>
    /// Called when the button is clicked. It passes the turret prefab to LevelManager,
    /// which then creates the ghost preview.
    /// </summary>
    private void OnSelectTower()
    {
        if (LevelManager.main != null && turretPrefab != null)
        {
            LevelManager.main.SetSelectedTurret(turretPrefab);
            Debug.Log("TowerSelectionUI: Selected turret " + turretPrefab.name);
        }
        else
        {
            Debug.LogError("TowerSelectionUI: LevelManager.main or turretPrefab is null!");
        }
    }
}
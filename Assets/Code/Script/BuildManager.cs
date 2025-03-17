using UnityEngine;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private Tower[] towers;  // Array of available tower types (assign in Inspector).
    public int selectedTower;                  // Index of the currently selected tower.
    public Tower lastSelectedTower;            // The last selected tower reference.

    [Header("UI Controls")]
    [SerializeField] private Button clearButton; // Button to clear the current turret selection.

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearSelectedTower);
        }
    }

    /// <summary>
    /// Returns the currently selected tower.
    /// If no turret is selected, returns null.
    /// </summary>
    public Tower GetSelectedTower()
    {
        if (selectedTower < 0 || selectedTower >= towers.Length)
        {
            return null;
        }
        return towers[selectedTower];
    }

    /// <summary>
    /// Sets the currently selected turret by index.
    /// </summary>
    public void SetSelectedTower(int _selectedTower)
    {
        selectedTower = _selectedTower;
        lastSelectedTower = towers[selectedTower];
    }

    /// <summary>
    /// Clears the current turret selection and removes the ghost turret from the cursor.
    /// </summary>
    public void ClearSelectedTower()
    {
        selectedTower = -1;
        lastSelectedTower = null;
        if (LevelManager.main != null)
        {
            LevelManager.main.ClearSelectedTurret();
        }
        Debug.Log("Turret selection cleared.");
    }
}
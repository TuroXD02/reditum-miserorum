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
    [SerializeField] private Button clearButton;          // Optional: A single clear button.
    [SerializeField] private Button[] additionalClearButtons; // Array of additional clear buttons.

    private void Awake()
    {
        main = this;
        selectedTower = -1; // Make sure no turret is selected at start
    }


    private void Start()
    {
        ClearSelectedTower(); // Just to be sure

        if (clearButton != null)
            clearButton.onClick.AddListener(ClearSelectedTower);

        if (additionalClearButtons != null && additionalClearButtons.Length > 0)
        {
            foreach (Button btn in additionalClearButtons)
            {
                if (btn != null)
                    btn.onClick.AddListener(ClearSelectedTower);
            }
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
        
    }
}

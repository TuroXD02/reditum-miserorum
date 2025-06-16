using UnityEngine;
using UnityEngine.UI;

public class BuildManager : MonoBehaviour
{
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private Tower[] towers;  // Array of available tower types (assign in Inspector).
    public int selectedTower;                 // Index of the currently selected tower.
    public Tower lastSelectedTower;           // The last selected tower reference.

    [Header("UI Controls")]
    [SerializeField] private Button clearButton;                // Optional: A single clear button.
    [SerializeField] private Button[] additionalClearButtons;   // Array of additional clear buttons.

    [Header("Cursor Settings")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D buildCursor;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    private void Awake()
    {
        main = this;
        selectedTower = -1;
    }

    private void Start()
    {
        ClearSelectedTower();

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
    /// Returns the currently selected tower, or null if none selected.
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
    /// Sets the currently selected tower and changes the cursor to build mode.
    /// </summary>
    public void SetSelectedTower(int _selectedTower)
    {
        selectedTower = _selectedTower;
        lastSelectedTower = towers[selectedTower];

        Cursor.SetCursor(buildCursor, cursorHotspot, cursorMode);
    }

    /// <summary>
    /// Clears the selected tower and resets the cursor.
    /// </summary>
    public void ClearSelectedTower()
    {
        selectedTower = -1;
        lastSelectedTower = null;

        if (LevelManager.main != null)
        {
            LevelManager.main.ClearSelectedTurret();
        }

        Cursor.SetCursor(defaultCursor, cursorHotspot, cursorMode);
    }
}

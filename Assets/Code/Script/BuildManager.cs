
using UnityEngine;


public class BuildManager : MonoBehaviour
{
    // Static instance of BuildManager for global access.
    // This allows other scripts to easily reference this manager without needing a direct reference.
    public static BuildManager main;

    [Header("References")]
    [SerializeField] private Tower[] towers;  // Array of available tower types that can be built (dragged into the Inspector).
    public int selectedTower;  // The index of the currently selected tower in the `towers` array.
    public Tower lastSelectedTower; // Reference to the last selected tower, stored to re-select or track after actions like selling.

    // Called when the script is loaded or the GameObject is instantiated.
    private void Awake()
    {
        // Assigns this instance of BuildManager to the static `main` variable.
        // This effectively makes this script a singleton, ensuring only one instance is used across the game.
        main = this;
    }

    // Returns the currently selected tower based on the `selectedTower` index.
    public Tower GetSelectedTower()
    {
        // Retrieves the tower at the index `selectedTower` from the `towers` array.
        // This gives access to the tower type that the player has chosen in the UI or elsewhere.
        Tower selectedTower = towers[this.selectedTower];

      

        // Returns the tower object so other scripts can use it for building or other logic.
        return selectedTower;
    }

    // Updates the currently selected tower when the player chooses a different tower in the UI/shop.
    public void SetSelectedTower(int _selectedTower)
    {
        // Updates the `selectedTower` index to the one provided by the player (via `_selectedTower`).
        selectedTower = _selectedTower;

        // Updates `lastSelectedTower` to reflect the new selection.
        // This is useful for tracking changes or reverting to the last known selection.
        lastSelectedTower = towers[selectedTower];

        
    }

    
}
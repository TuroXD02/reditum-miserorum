using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [Header("References")]
    // Tracks whether the player is currently interacting with the UI

    [SerializeField] private bool isHoveringUI;
    // Singleton instance to allow global access to UiManager
    public static UiManager main;

    // Tracks whether the player is currently interacting with the UI
    

    // Called when the script instance is loaded
    private void Awake() 
    {
        // Assign this instance to the static singleton variable
        main = this;
        isHoveringUI = false;   
        // Initialize the hovering state to false (not interacting with UI)
        UiManager.main.SetHoveringState(false);
        
    }

    // Sets the current hovering state (true if interacting with the UI, false otherwise)
    public void SetHoveringState(bool state)
    {
        isHoveringUI = state;
    }

    // Returns whether the player is currently interacting with the UI
    public bool IsHoveringUI() 
    {
        return isHoveringUI;
    }
}
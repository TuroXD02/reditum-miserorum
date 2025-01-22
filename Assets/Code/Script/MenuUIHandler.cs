using UnityEngine.EventSystems;
using UnityEngine;

// This class manages the UI interactions for the main menu and tracks mouse hover states.
public class MenuUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // A static reference to the singleton instance of this class.
    public static MenuUIHandler main;

    // Tracks whether the mouse is currently hovering over the UI.
    public bool mouse_over = false;

    // Called when the script instance is being loaded.
    private void Awake()
    {
        // Set this instance as the static reference, allowing global access to this script.
        main = this;
    }

    // This method is called when the mouse pointer enters the UI element this script is attached to.
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Mark that the mouse is hovering over the UI.
        mouse_over = true;

        // Notify the UiManager that the user is interacting with the UI.
        UiManager.main.SetHoveringState(true);
    }

    // This method is called when the mouse pointer exits the UI element this script is attached to.
    public void OnPointerExit(PointerEventData eventData)
    {
        // Mark that the mouse is no longer hovering over the UI.
        mouse_over = false;

        // Notify the UiManager that the user is no longer interacting with the UI.
        UiManager.main.SetHoveringState(false);
    }
}

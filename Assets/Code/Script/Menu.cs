using TMPro;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public static Menu main;

    [Header("References")]
    [SerializeField] TextMeshProUGUI currencyUI;
    [SerializeField] Animator anim;
    private bool isMenuOpen = true;

    private void Awake()
    {
        main = this;  // Set the static reference
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        anim.SetBool("MenuOpen", isMenuOpen);
    }

    private void OnGUI()
    {
        // Update the text of the currency UI element with the player's current currency value
        // Convert the currency (int) to a string using ToString() so it can be displayed in the UI.
        currencyUI.text = LevelManager.main.currency.ToString();
    }

    
}

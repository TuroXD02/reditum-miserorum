using UnityEngine;
using TMPro;

// This class manages the player's health system and handles game over logic.
public class PlayerHealthSystem : MonoBehaviour
{
    // Reference to the TextMeshPro UI element that displays the player's health.
    public TextMeshProUGUI txt_lifeCount;

    public int defaultHealthCount;

    public int healthCount;

    [Header("Lose Screen Reference")]
   
    [SerializeField] private GameObject loseScreen;

    // Initializes the player's health system at the start of the game.
    public void Init()
    {
        // Set the player's current health to the default value.
        healthCount = defaultHealthCount;

        // Update the UI to display the player's current health.
        txt_lifeCount.text = healthCount.ToString();

        // Ensure the LoseScreen is inactive at the start of the game.
        if (loseScreen != null)
        {
            loseScreen.SetActive(false);
        }
    }

    // Decreases the player's health when called.
    public void LoseHealth()
    {
        // Prevent any action if the player is already out of health.
        if (healthCount < 1)
            return;

        // Decrease the player's current health by one.
        healthCount--;

        // Update the UI to reflect the new health value.
        txt_lifeCount.text = healthCount.ToString();

        // Check if the player's health has reached zero to trigger game over.
        CheckHealthCount();
    }

    // Checks the player's health and triggers the game over sequence if necessary.
    private void CheckHealthCount()
    {
        // If the player's health is less than one, trigger game over.
        if (healthCount < 1)
        {
            Debug.Log("Game Over"); // Log the game over message for debugging purposes.

            // If the LoseScreen is set, activate it to display the game over screen.
            if (loseScreen != null)
            {
                loseScreen.SetActive(true);
            }

            // Pause the game by stopping time.
            Time.timeScale = 0f;
        }
    }
}

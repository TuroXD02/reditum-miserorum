using UnityEngine;

public class EndPointTrigger : MonoBehaviour
{
    // Layer index for "Enemy"
    private int enemyLayer = 6;  // Enemy layer is set to 6

    private void OnCollisionEnter2D(Collision2D other)
    {
       

        // Check if the object that entered the trigger is on the "Enemy" layer (Layer 6)
        if (other.gameObject.layer == enemyLayer)
        {
           

            // Access the PlayerHealthSystem and call LoseHealth()
            PlayerHealthSystem playerHealth = LevelManager.main.GetComponent<PlayerHealthSystem>();

            if (playerHealth != null)
            {
                
                playerHealth.LoseHealth();
            }

        }

    }
    
    private void Update()
    {
        if (Time.timeScale == 0f) return; // Prevent spawning during paused state (LoseScreen)

        // Existing Update logic here...
    }
}

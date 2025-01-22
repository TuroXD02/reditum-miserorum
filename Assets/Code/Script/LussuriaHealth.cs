using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LussuriaHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoints;
    [SerializeField] private int currencyWorth;
    [SerializeField] private float armor; // Armor percentage (e.g., 20 means 20% damage reduction)

    private bool isDestroyed = false;

    private EnemyMovement enemyMovement;

    private void Start()
    {
        // Get the EnemyMovement component from the same GameObject
        enemyMovement = GetComponent<EnemyMovement>();
    }

    // Method to handle taking damage, including armor reduction
    public void TakeDamage(int dmg)
    {
        // Get the current speed of the enemy
        float currentSpeed = enemyMovement != null ? enemyMovement.moveSpeed : 0;

        // Modify the armor to increase proportionally with the current speed
        float speedFactor = 20f; // Adjust this factor to control how much speed affects armor
        float adjustedArmor = currentSpeed * speedFactor;

        // Calculate damage reduction based on adjusted armor percentage
        float damageMultiplier = 1f - (adjustedArmor / 100f);
        damageMultiplier = Mathf.Clamp(damageMultiplier, -1f, 1f);

        // Calculate the actual damage after reduction
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);

        // Subtract the final damage from hit points
        hitPoints -= finalDamage;

        // Check if the enemy is destroyed
        if (hitPoints <= 0 && !isDestroyed)
        {
            isDestroyed = true;
            EnemySpawner.onEnemyDestroy.Invoke();
            LevelManager.main.IncreaseCurrency(currencyWorth);
            Destroy(gameObject);
        }

        //Debug.Log($"Incoming Damage: {dmg}, Speed: {currentSpeed}, Adjusted Armor: {adjustedArmor}%, Final Damage: {finalDamage}, Remaining HP: {hitPoints}");
    }
    
    // Corrected naming for the DOT damage method
    public void TakeDamageDOTLU(int dmg)
    {
        // Reduce the hit points of the object by the damage amount (dmg).
        hitPoints -= dmg;

        // Check if the object's hit points have reached 0 or below and it hasn't been destroyed yet.
        if (hitPoints <= 0 && !isDestroyed)
        {
            // Mark the object as destroyed to prevent multiple destruction events.
            isDestroyed = true;

            // Increase the player's currency by the value of this object's currencyWorth.
            LevelManager.main.IncreaseCurrency(currencyWorth);

            // Destroy this GameObject, removing it from the game world.
            Destroy(gameObject);
        }
    }
}

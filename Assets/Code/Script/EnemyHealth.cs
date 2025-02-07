using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] public int hitPoints;            // Base hit points for the enemy
    [SerializeField] private int currencyWorth;       // Currency earned when enemy is killed
    [SerializeField] private float armor;            // Armor percentage, reduces incoming damage

    
    private bool isDestroyed = false;

   
    // Method to handle incoming damage
    public void TakeDamage(int dmg)
    {
        if (isDestroyed) return;  // If already destroyed, do nothing

        // Calculate damage reduction based on armor percentage
        float damageMultiplier = 1f - (armor / 100f);
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);  // Reduced damage after applying armor

        // Reduce hit points based on final damage
        hitPoints -= finalDamage;

        // Check if the enemy is destroyed
        if (hitPoints <= 0 && !isDestroyed)
        {
            // Signal enemy destruction and trigger other effects (e.g., currency increase)
            EnemyDestroyed();
        }

       // Debug.Log($"Incoming Damage: {dmg}, Armor: {armor}%, Final Damage: {finalDamage}, Remaining HP: {hitPoints}");
    }
    
    public void ReduceArmour(int amount)
    {
        // Reduce the armour value (and ensure it doesn't drop below zero).
        armor = Mathf.Max(armor - amount, 0);
    }

    public void TakeDamageDOT(int dmg)
    {
        if (isDestroyed) return;  // If already destroyed, do nothing

        // Calculate damage reduction based on armor percentage
        float damageMultiplier = 1f;
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);  // Reduced damage after applying armor

        // Reduce hit points based on final damage
        hitPoints -= finalDamage;

        // Check if the enemy is destroyed
        if (hitPoints <= 0 && !isDestroyed)
        {
            // Signal enemy destruction and trigger other effects (e.g., currency increase)
            EnemyDestroyed();
        }

        //Debug.Log($"Incoming Damage: {dmg}, Armor: {armor}%, Final Damage: {finalDamage}, Remaining HP: {hitPoints}");
    }

    private void EnemyDestroyed()
    {
        isDestroyed = true;

        // Invoke the event for enemy destruction for tracking kills, spawns, etc.
        EnemySpawner.onEnemyDestroy.Invoke();

        // Increase player's currency upon enemy destruction
        LevelManager.main.IncreaseCurrency(currencyWorth);

        // Destroy the enemy object
        Destroy(gameObject);
    }
    
    
}
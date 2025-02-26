using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] public int hitPoints;            // Base hit points for the enemy.
    [SerializeField] private int currencyWorth;       // Currency earned when enemy is killed.
    [SerializeField] private float armor;             // Armor percentage.

    private bool isDestroyed = false;

    public void TakeDamage(int dmg)
    {
        if (isDestroyed) return;

        float damageMultiplier = 1f - (armor / 100f);
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);

        hitPoints -= finalDamage;
        

        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemyDestroyed();
        }
    }
    
    public void ReduceArmour(int amount)
    {
        float oldArmor = armor;
        armor = Mathf.Max(armor - amount, 0);
        Debug.Log($"{gameObject.name} armor reduced from {oldArmor} to {armor} by {amount}");
        
        if(LevelManager.main != null)
        {
            LevelManager.main.PlayArmorChangeEffect(transform, false);
        }
    }

    public void TakeDamageDOT(int dmg)
    {
        if (isDestroyed) return;

        float damageMultiplier = 1f;
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);
        hitPoints -= finalDamage;
        Debug.Log($"{gameObject.name} took DOT damage {finalDamage}, remaining HP: {hitPoints}");
        
        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemyDestroyed();
        }
    }

    private void EnemyDestroyed()
    {
        isDestroyed = true;
        EnemySpawner.onEnemyDestroy.Invoke();
        LevelManager.main.IncreaseCurrency(currencyWorth);
        Destroy(gameObject);
    }
}
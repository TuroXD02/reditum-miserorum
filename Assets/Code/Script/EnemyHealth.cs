using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] public int hitPoints;            // Base hit points for the enemy.
    [SerializeField] private int currencyWorth;       // Currency earned when enemy is killed.
    [SerializeField] private float armor;             // Armor percentage.

    [Header("Health Sprite Settings")]
    [Tooltip("Alternate sprites to use as health drops. " +
             "Element 0 is used when health falls to 70% or below, " +
             "Element 1 when health falls to 40% or below, " +
             "Element 2 when health falls to 10% or below.")]
    [SerializeField] private Sprite[] healthSprites;

    protected bool isDestroyed = false;
    protected int fullHealth;
    private SpriteRenderer sr;
    private Sprite originalSprite;

    // Public property to expose current hit points.
    public virtual int HitPoints 
    { 
        get { return hitPoints; } 
    }

    protected virtual void Start()
    {
        fullHealth = hitPoints;  // Save the starting hit points as full health.
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalSprite = sr.sprite;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} does not have a SpriteRenderer component!");
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        if (isDestroyed) return;

        float damageMultiplier = 1f - (armor / 100f);
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);
        hitPoints -= finalDamage;
        

        CheckHealthSprite();

        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemyDestroyed();
        }
    }
    
    public virtual void ReduceArmour(int amount)
    {
        if (armor <= 0) return;  // If armor is already zero, do nothing.

        float oldArmor = armor;
        armor = Mathf.Max(armor - amount, 0);
       

        bool isArmorZero = (armor == 0); // Check if armor reached zero.

        if (LevelManager.main != null)
        {
            LevelManager.main.PlayArmorChangeEffect(transform, false, isArmorZero);
        }
    }

    public virtual void TakeDamageDOT(int dmg)
    {
        if (isDestroyed) return;

        float damageMultiplier = 1f;
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);
        hitPoints -= finalDamage;
       

        CheckHealthSprite();

        if (hitPoints <= 0 && !isDestroyed)
        {
            EnemyDestroyed();
        }
    }

    protected virtual void EnemyDestroyed()
    {
        isDestroyed = true;
        EnemySpawner.onEnemyDestroy.Invoke();
        if (LevelManager.main != null)
        {
            LevelManager.main.IncreaseCurrency(currencyWorth);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Checks the current health percentage and updates the sprite accordingly.
    /// </summary>
    protected virtual void CheckHealthSprite()
    {
        if (sr == null)
            return;

        float fraction = (float)hitPoints / fullHealth;
        
        // Above 70% health, use the original sprite.
        if (fraction > 0.7f)
        {
            if (sr.sprite != originalSprite)
            {
                sr.sprite = originalSprite;
                
            }
        }
        // When health is 70% or lower (lost 30%), switch to the first sprite.
        else if (fraction > 0.4f) // between 70% and 40%
        {
            if (healthSprites != null && healthSprites.Length >= 1)
            {
                if (sr.sprite != healthSprites[0])
                {
                    sr.sprite = healthSprites[0];
                   
                }
            }
        }
        // When health is 40% or lower (lost 60%), switch to the second sprite.
        else if (fraction > 0.1f) // between 40% and 10%
        {
            if (healthSprites != null && healthSprites.Length >= 2)
            {
                if (sr.sprite != healthSprites[1])
                {
                    sr.sprite = healthSprites[1];
                    
                }
            }
        }
        // When health is 10% or lower (lost 90%), switch to the third sprite.
        else
        {
            if (healthSprites != null && healthSprites.Length >= 3)
            {
                if (sr.sprite != healthSprites[2])
                {
                    sr.sprite = healthSprites[2];
                    
                }
            }
        }
    }
}

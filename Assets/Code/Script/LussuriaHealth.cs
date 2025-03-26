using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LussuriaHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoints;            // Base hit points for the enemy.
    [SerializeField] private int currencyWorth;        // Currency earned when enemy is killed.
    [SerializeField] private float baseArmor = 100f;     // Base armor value (full protection by default).
    [SerializeField] private float armor;              // Current effective armor value.

    private bool isDestroyed = false;
    private EnemyMovement enemyMovement;

    // Store the previous effective armor value to detect changes.
    private float previousEffectiveArmor;

    // Public property to expose current hit points.
    public int HitPoints 
    { 
        get { return hitPoints; } 
    }

    [Header("Armor Sprite Settings")]
    [Tooltip("Sprite to display when effective armor is above 50.")]
    [SerializeField] private Sprite highEffectiveArmorSprite;
    [Tooltip("Sprite to display when effective armor is above 0 and 50 or below.")]
    [SerializeField] private Sprite lowEffectiveArmorSprite;
    [Tooltip("Optional: Sprite to display when effective armor has reached 0.")]
    [SerializeField] private Sprite armorZeroSprite;

    private SpriteRenderer sr;
    private Sprite originalSprite;

    private void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalSprite = sr.sprite;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} has no SpriteRenderer component!");
        }
        // Initialize effective armor.
        armor = baseArmor;
        previousEffectiveArmor = armor;
    }

    private void Update()
    {
        UpdateArmorBasedOnSpeed();
    }

    /// <summary>
    /// Dynamically updates effective armor based on current speed.
    /// EffectiveArmor = baseArmor * (currentSpeed / BaseSpeed)
    /// Triggers armor up or down effects via LevelManager if a change is detected.
    /// </summary>
    private void UpdateArmorBasedOnSpeed()
    {
        if (enemyMovement == null) return;
        float currentSpeed = enemyMovement.moveSpeed;
        float baseSpeed = enemyMovement.BaseSpeed;
        if (baseSpeed <= 0) return;

        float effectiveArmor = baseArmor * (currentSpeed / baseSpeed);
        effectiveArmor = Mathf.Clamp(effectiveArmor, 0, baseArmor);

        // Detect a drop in effective armor.
        if (effectiveArmor < previousEffectiveArmor)
        {
            // If effective armor has dropped to 0, trigger the armor-zero effect.
            if (effectiveArmor == 0 && previousEffectiveArmor > 0)
            {
                Debug.Log($"{gameObject.name} effective armor dropped to 0. Triggering armor zero effect.");
                if (LevelManager.main != null)
                {
                    LevelManager.main.PlayArmorChangeEffect(transform, false, true);
                }
            }
            else if (effectiveArmor > 0)
            {
                Debug.Log($"{gameObject.name} effective armor dropped from {previousEffectiveArmor} to {effectiveArmor}. Triggering armor down effect.");
                if (LevelManager.main != null)
                {
                    LevelManager.main.PlayArmorChangeEffect(transform, false);
                }
            }
        }
        // Detect an increase in effective armor.
        else if (effectiveArmor > previousEffectiveArmor)
        {
            Debug.Log($"{gameObject.name} effective armor increased from {previousEffectiveArmor} to {effectiveArmor}. Triggering armor up effect.");
            if (LevelManager.main != null)
            {
                LevelManager.main.PlayArmorChangeEffect(transform, true);
            }
        }

        previousEffectiveArmor = effectiveArmor;
        armor = effectiveArmor;
        Debug.Log($"{gameObject.name} updated effective armor to: {armor} (Speed: {currentSpeed}/{baseSpeed}, baseArmor: {baseArmor})");

        CheckArmorSprite();
    }

    /// <summary>
    /// Updates the enemy sprite based on the current effective armor.
    /// - If effective armor > 50, uses highEffectiveArmorSprite (or reverts to the original sprite if not assigned).
    /// - If effective armor is above 0 and ≤ 50, uses lowEffectiveArmorSprite.
    /// - If effective armor == 0, uses armorZeroSprite (if assigned), otherwise remains on lowEffectiveArmorSprite.
    /// </summary>
    private void CheckArmorSprite()
    {
        if (sr == null) return;

        if (armor <= 0)
        {
            if (armorZeroSprite != null)
            {
                if (sr.sprite != armorZeroSprite)
                {
                    sr.sprite = armorZeroSprite;
                    Debug.Log($"{gameObject.name} switched to armor zero sprite (armor == 0).");
                }
            }
            else
            {
                if (lowEffectiveArmorSprite != null && sr.sprite != lowEffectiveArmorSprite)
                {
                    sr.sprite = lowEffectiveArmorSprite;
                    Debug.Log($"{gameObject.name} switched to low armor sprite (armor == 0, no armor zero sprite assigned).");
                }
            }
        }
        else if (armor > 50)
        {
            if (highEffectiveArmorSprite != null)
            {
                if (sr.sprite != highEffectiveArmorSprite)
                {
                    sr.sprite = highEffectiveArmorSprite;
                    Debug.Log($"{gameObject.name} switched to high armor sprite (armor > 50).");
                }
            }
            else
            {
                if (sr.sprite != originalSprite)
                {
                    sr.sprite = originalSprite;
                    Debug.Log($"{gameObject.name} reverted to original sprite (armor > 50, no high armor sprite assigned).");
                }
            }
        }
        else // armor > 0 and <= 50
        {
            if (lowEffectiveArmorSprite != null)
            {
                if (sr.sprite != lowEffectiveArmorSprite)
                {
                    sr.sprite = lowEffectiveArmorSprite;
                    Debug.Log($"{gameObject.name} switched to low armor sprite (armor <= 50).");
                }
            }
        }
    }

    /// <summary>
    /// Applies damage using the effective armor value.
    /// </summary>
    public virtual void TakeDamage(int dmg)
    {
        float damageMultiplier = 1f - (armor / 100f);
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);
        hitPoints -= finalDamage;
        Debug.Log($"{gameObject.name} took {finalDamage} damage (Effective Armor: {armor}), remaining HP: {hitPoints}");

        CheckArmorSprite();

        if (hitPoints <= 0 && !isDestroyed)
        {
            isDestroyed = true;
            EnemySpawner.onEnemyDestroy.Invoke();
            if (LevelManager.main != null)
            {
                LevelManager.main.IncreaseCurrency(currencyWorth);
            }
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Applies flat DOT damage.
    /// </summary>
    public virtual void TakeDamageDOTLU(int dmg)
    {
        hitPoints -= dmg;
        Debug.Log($"{gameObject.name} took {dmg} DOT damage, remaining HP: {hitPoints}");
        CheckArmorSprite();
        if (hitPoints <= 0 && !isDestroyed)
        {
            isDestroyed = true;
            if (LevelManager.main != null)
            {
                LevelManager.main.IncreaseCurrency(currencyWorth);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Permanently reduces the enemy’s base armor.
    /// Immediately updates effective armor and triggers an armor change effect.
    /// If baseArmor is already 0, no further reduction is performed.
    /// </summary>
    public virtual void ReduceArmour(int amount)
    {
        Debug.Log($"{gameObject.name} ReduceArmour() called with amount: {amount}");
        
        // If baseArmor is already zero, do nothing.
        if (baseArmor <= 0)
        {
            Debug.Log($"{gameObject.name} baseArmor is already 0; no further reduction.");
            return;
        }

        float oldBaseArmor = baseArmor;
        baseArmor = Mathf.Max(baseArmor - amount, 0);
        Debug.Log($"{gameObject.name} baseArmor reduced from {oldBaseArmor} to {baseArmor} by {amount}");

        EnemyMovement enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement != null)
        {
            float currentSpeed = enemyMovement.moveSpeed;
            float baseSpeed = enemyMovement.BaseSpeed;
            float effectiveArmor = baseArmor * (currentSpeed / baseSpeed);
            effectiveArmor = Mathf.Clamp(effectiveArmor, 0, baseArmor);
            armor = effectiveArmor;
        }

        if (LevelManager.main != null)
        {
            if (armor <= 0)
            {
                Debug.Log($"{gameObject.name} armor reached 0, triggering armor zero effect.");
                LevelManager.main.PlayArmorChangeEffect(transform, false, true);
            }
            else
            {
                LevelManager.main.PlayArmorChangeEffect(transform, false);
            }
        }
        
        CheckArmorSprite();
    }
}

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
    [Tooltip("Sprite to display when effective armor is 50 or below.")]
    [SerializeField] private Sprite lowEffectiveArmorSprite;

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
    /// Triggers an armor up effect if effective armor increases, and an armor down effect if it drops.
    /// Also updates the enemy sprite based on effective armor.
    /// </summary>
    private void UpdateArmorBasedOnSpeed()
    {
        if (enemyMovement == null) return;

        float currentSpeed = enemyMovement.moveSpeed;
        float baseSpeed = enemyMovement.BaseSpeed;
        if (baseSpeed <= 0) return;

        float effectiveArmor = baseArmor * (currentSpeed / baseSpeed);
        effectiveArmor = Mathf.Clamp(effectiveArmor, 0, baseArmor);

        if (effectiveArmor < previousEffectiveArmor)
        {
            Debug.Log($"{gameObject.name} effective armor dropped from {previousEffectiveArmor} to {effectiveArmor}. Triggering armor reduction effect.");
            if (LevelManager.main != null)
            {
                LevelManager.main.PlayArmorChangeEffect(transform, false);
            }
        }
        else if (effectiveArmor > previousEffectiveArmor)
        {
            Debug.Log($"{gameObject.name} effective armor increased from {previousEffectiveArmor} to {effectiveArmor}. Triggering armor increase effect.");
            if (LevelManager.main != null)
            {
                LevelManager.main.PlayArmorChangeEffect(transform, true);
            }
        }

        previousEffectiveArmor = effectiveArmor;
        armor = effectiveArmor;
        Debug.Log($"{gameObject.name} updated effective armor to: {armor} (Speed: {currentSpeed}/{baseSpeed}, baseArmor: {baseArmor})");

        // Update the sprite based on effective armor.
        CheckArmorSprite();
    }

    /// <summary>
    /// Checks the current effective armor value and updates the sprite accordingly.
    /// If effective armor is above 50, uses highEffectiveArmorSprite (if assigned, otherwise original sprite).
    /// If effective armor is 50 or below, uses lowEffectiveArmorSprite.
    /// </summary>
    private void CheckArmorSprite()
    {
        if (sr == null) return;

        if (armor > 50)
        {
            if (highEffectiveArmorSprite != null)
            {
                if (sr.sprite != highEffectiveArmorSprite)
                {
                    sr.sprite = highEffectiveArmorSprite;
                    Debug.Log($"{gameObject.name} switched to high effective armor sprite (armor > 50).");
                }
            }
            else
            {
                if (sr.sprite != originalSprite)
                {
                    sr.sprite = originalSprite;
                    Debug.Log($"{gameObject.name} reverted to original sprite (armor > 50, no high effective armor sprite assigned).");
                }
            }
        }
        else // armor <= 50
        {
            if (lowEffectiveArmorSprite != null)
            {
                if (sr.sprite != lowEffectiveArmorSprite)
                {
                    sr.sprite = lowEffectiveArmorSprite;
                    Debug.Log($"{gameObject.name} switched to low effective armor sprite (armor <= 50).");
                }
            }
        }
    }

    /// <summary>
    /// Applies damage using the effective armor value.
    /// </summary>
    public void TakeDamage(int dmg)
    {
        float damageMultiplier = 1f - (armor / 100f);
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);
        hitPoints -= finalDamage;
        Debug.Log($"{gameObject.name} took {finalDamage} damage (Effective Armor: {armor}), remaining HP: {hitPoints}");

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
    public void TakeDamageDOTLU(int dmg)
    {
        hitPoints -= dmg;
        Debug.Log($"{gameObject.name} took {dmg} DOT damage, remaining HP: {hitPoints}");
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
    /// Permanently reduces the enemy’s base armor (e.g., via an armor breaker turret).
    /// This permanently lowers the maximum protection.
    /// Also triggers an armor reduction effect and immediately updates effective armor.
    /// </summary>
    public void ReduceArmour(int amount)
    {
        Debug.Log($"{gameObject.name} ReduceArmour() called with amount: {amount}");
        float oldBaseArmor = baseArmor;
        baseArmor = Mathf.Max(baseArmor - amount, 0);
        Debug.Log($"{gameObject.name} baseArmor reduced from {oldBaseArmor} to {baseArmor} by {amount}");

        // Immediately update effective armor.
        if (enemyMovement != null)
        {
            float currentSpeed = enemyMovement.moveSpeed;
            float baseSpeed = enemyMovement.BaseSpeed;
            float effectiveArmor = baseArmor * (currentSpeed / baseSpeed);
            effectiveArmor = Mathf.Clamp(effectiveArmor, 0, baseArmor);
            armor = effectiveArmor;
            previousEffectiveArmor = effectiveArmor;
        }

        if (LevelManager.main != null)
        {
            Debug.Log($"{gameObject.name} calling PlayArmorChangeEffect for permanent armor reduction");
            LevelManager.main.PlayArmorChangeEffect(transform, false);
        }
        
        // Update sprite immediately after armor reduction.
        CheckArmorSprite();
    }
}

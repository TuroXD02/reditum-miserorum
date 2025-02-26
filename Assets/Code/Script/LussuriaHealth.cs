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
    public int HitPoints { get { return hitPoints; } }

    private void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        // Initialize effective armor with full protection.
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
    /// If effective armor increases (i.e. enemy regains speed), trigger the "armor up" effect.
    /// If it drops (i.e. enemy is slowed), trigger the "armor down" effect.
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
            LevelManager.main.IncreaseCurrency(currencyWorth);
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
            LevelManager.main.IncreaseCurrency(currencyWorth);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Permanently reduces the enemy’s base armor (e.g., via an armor breaker turret).
    /// This permanently lowers the maximum protection.
    /// Also triggers an armor reduction effect.
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
    }
}

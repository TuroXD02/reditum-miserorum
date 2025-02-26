using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretSlow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask enemyMask;                // Mask to identify enemies.
    [SerializeField] private GameObject upgradeUI;                 // UI for turret upgrades.
    [SerializeField] private Button upgradeButton;                 // Button for triggering upgrades.
    [SerializeField] private SpriteRenderer turretSpriteRenderer;  // Turret's SpriteRenderer.
    [SerializeField] private Sprite[] upgradeSprites;              // Array of turret sprites for upgrades.

    [Header("Attributes")]
    [SerializeField] public float targetingRange;                 // Detection range.
    [SerializeField] private float aps;                            // Attacks per second.
    [SerializeField] private float freezeTime;                     // Duration enemy remains slowed (to be extended on re-hit).
    [SerializeField] public int baseUpgradeCost;                  // Base cost for upgrading the turret.

    [Header("Freeze Effect Animation (on Turret)")]
    [SerializeField] private GameObject freezeEffectPrefab;        // Prefab for turret freeze effect.
    [SerializeField] private RuntimeAnimatorController freezeAnimatorController; // Animator for turret freeze effect.
    [SerializeField] private float freezeEffectDuration = 2f;      // Duration of turret freeze effect.

    [Header("Enemy Visual Effect Settings")]
    [SerializeField] private GameObject enemyVisualEffectPrefab;   // Prefab for enemy visual effect.
    [SerializeField] private float enemyEffectDuration = 3f;       // Duration enemy visual effect stays active.

    [Header("Enemy Tint Settings")]
    [SerializeField] private Color enemyBlueColor = new Color(0.7f, 0.7f, 1f, 1f); // Blue tint to apply.
    [SerializeField] private float tintLerpDuration = 0.5f;          // Time to fade in/out the tint.

    // Internal state.
    private float apsBase;         // Base APS.
    private float timeUntilFire;   // Timer for attack rate.
    private int level = 1;         // Current turret level.

    // Dictionary to track active reset speed coroutines per enemy.
    private static Dictionary<EnemyMovement, Coroutine> activeResetCoroutines = new Dictionary<EnemyMovement, Coroutine>();
    // Dictionary to track the last slow hit time for each enemy.
    private static Dictionary<EnemyMovement, float> lastSlowHitTime = new Dictionary<EnemyMovement, float>();

    private void Start()
    {
        apsBase = aps;
        upgradeButton.onClick.AddListener(Upgrade);
    }

    private void Update()
    {
        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / aps)
        {
            Freeze();
            timeUntilFire = 0f;
        }
    }

    /// <summary>
    /// Applies the slow effect to all enemies in range:
    /// - Sets their speed to 0.1.
    /// - Updates a timestamp so that the slow effect lasts freezeTime seconds after the last hit.
    /// - Instantiates a visual effect prefab on them.
    /// - Applies the blue tint effect via the EnemySlowEffect component.
    /// Also plays the turret's freeze effect.
    /// </summary>
    private void Freeze()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                EnemyMovement em = hit.transform.GetComponent<EnemyMovement>();
                if (em != null)
                {
                    // Apply the slow by setting speed.
                    em.UpdateSpeed(0.1f);
                    // Update last slow hit time.
                    lastSlowHitTime[em] = Time.time;

                    // If there's no reset coroutine for this enemy, start one.
                    if (!activeResetCoroutines.ContainsKey(em))
                    {
                        Coroutine resetCoroutine = StartCoroutine(ResetEnemySpeed(em, freezeTime));
                        activeResetCoroutines.Add(em, resetCoroutine);
                    }
                    // Instantiate enemy visual effect.
                    if (enemyVisualEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(enemyVisualEffectPrefab, hit.transform.position, Quaternion.identity, hit.transform);
                        Destroy(effect, enemyEffectDuration);
                    }
                    // Apply the blue tint effect.
                    EnemySlowEffect slowEffect = hit.transform.GetComponent<EnemySlowEffect>();
                    if (slowEffect == null)
                    {
                        slowEffect = hit.transform.gameObject.AddComponent<EnemySlowEffect>();
                    }
                    slowEffect.ApplySlowEffect(enemyBlueColor, tintLerpDuration, freezeTime);
                }
            }
        }

        // Play turret freeze effect.
        if (freezeEffectPrefab != null)
        {
            GameObject turretEffect = Instantiate(freezeEffectPrefab, transform.position, Quaternion.identity, transform);
            Animator effectAnim = turretEffect.GetComponent<Animator>();
            if (effectAnim != null && freezeAnimatorController != null)
            {
                effectAnim.runtimeAnimatorController = freezeAnimatorController;
            }
            float scaleFactor = targetingRange / 2f;
            turretEffect.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
            Destroy(turretEffect, freezeEffectDuration);
        }
    }

    /// <summary>
    /// Continuously checks if the enemy should be unslowed.
    /// Waits until the current time is at least freezeTime seconds past the last slow hit, then resets speed.
    /// </summary>
    private IEnumerator ResetEnemySpeed(EnemyMovement enemy, float duration)
    {
        while (true)
        {
            if (lastSlowHitTime.ContainsKey(enemy))
            {
                if (Time.time >= lastSlowHitTime[enemy] + duration)
                {
                    enemy.ResetSpeed();
                    lastSlowHitTime.Remove(enemy);
                    break;
                }
            }
            else
            {
                break;
            }
            yield return null;
        }
        if (activeResetCoroutines.ContainsKey(enemy))
        {
            activeResetCoroutines.Remove(enemy);
        }
    }

    public void OpenUpgradeUI()
    {
        upgradeUI.SetActive(true);
    }

    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        UiManager.main.SetHoveringState(false);
    }

    public void Upgrade()
    {
        if (CalculateCost() > LevelManager.main.currency) return;
        LevelManager.main.SpendCurrency(CalculateCost());
        level++;
        aps = CalculateAttackSpeed();
        targetingRange = CalculateRange();
        UpdateSprite();
        CloseUpgradeUI();
        Debug.Log("New APS:" + aps);
        Debug.Log("New Range:" + targetingRange);
        Debug.Log("New Cost:" + CalculateCost());
    }

    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 2f));
    }

    private float CalculateAttackSpeed()
    {
        return apsBase * Mathf.Pow(level, 0.32f);
    }

    private float CalculateRange()
    {
        return targetingRange * Mathf.Pow(level, 0.3f);
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
        {
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        }
        else
        {
            Debug.LogWarning("Sprite or sprite array is missing!");
        }
    }
}

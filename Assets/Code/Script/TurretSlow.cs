using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretSlow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private Sprite[] upgradeSprites;

    [Header("Attributes")]
    [SerializeField] public float targetingRange;
    [SerializeField] private float aps;
    [SerializeField] private float freezeTime;
    [SerializeField] public int baseUpgradeCost;

    [Header("Freeze Effect Animation (on Turret)")]
    [SerializeField] private GameObject freezeEffectPrefab;
    [SerializeField] private RuntimeAnimatorController freezeAnimatorController;
    [SerializeField] private float freezeEffectDuration = 2f;

    [Header("Enemy Visual Effect Settings")]
    [SerializeField] private GameObject enemyVisualEffectPrefab;
    [SerializeField] private float enemyEffectDuration = 3f;

    [Header("Enemy Tint Settings")]
    [SerializeField] private Color enemyBlueColor = new Color(0.7f, 0.7f, 1f, 1f);
    [SerializeField] private float tintLerpDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip freezeClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip placedClip;
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

    // Internal
    private float apsBase;
    private float timeUntilFire;
    private int level = 1;

    private static Dictionary<EnemyMovement, Coroutine> activeResetCoroutines = new Dictionary<EnemyMovement, Coroutine>();
    private static Dictionary<EnemyMovement, float> lastSlowHitTime = new Dictionary<EnemyMovement, float>();

    private void Start()
    {
        apsBase = aps;
        upgradeButton.onClick.AddListener(Upgrade);

        if (placedClip != null)
            PlaySound(placedClip);
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

    private void Freeze()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length > 0)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.transform.GetComponent<SlowImmunity>() != null)
                {
                    Debug.Log($"{hit.transform.name} is immune to slow. Skipping slow effect.");
                    continue;
                }

                EnemyMovement em = hit.transform.GetComponent<EnemyMovement>();
                if (em != null)
                {
                    em.UpdateSpeed(0.1f);
                    lastSlowHitTime[em] = Time.time;
                    Debug.Log($"[{hit.transform.name}] Slow hit at time {Time.time}");

                    if (!activeResetCoroutines.ContainsKey(em))
                    {
                        Coroutine resetCoroutine = StartCoroutine(ResetEnemySpeed(em, freezeTime));
                        activeResetCoroutines.Add(em, resetCoroutine);
                    }

                    if (enemyVisualEffectPrefab != null)
                    {
                        GameObject effect = Instantiate(enemyVisualEffectPrefab, hit.transform.position, Quaternion.identity, hit.transform);
                        Destroy(effect, enemyEffectDuration);
                    }

                    EnemySlowEffect slowEffect = hit.transform.GetComponent<EnemySlowEffect>();
                    if (slowEffect == null)
                        slowEffect = hit.transform.gameObject.AddComponent<EnemySlowEffect>();

                    slowEffect.ApplySlowEffect(enemyBlueColor, tintLerpDuration, freezeTime);
                }
            }
        }

        if (freezeEffectPrefab != null)
        {
            GameObject turretEffect = Instantiate(freezeEffectPrefab, transform.position, Quaternion.identity, transform);
            Animator effectAnim = turretEffect.GetComponent<Animator>();
            if (effectAnim != null && freezeAnimatorController != null)
                effectAnim.runtimeAnimatorController = freezeAnimatorController;

            float scaleFactor = targetingRange / 2f;
            turretEffect.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
            Destroy(turretEffect, freezeEffectDuration);
        }

        if (freezeClip != null)
            PlaySound(freezeClip);
    }

    private IEnumerator ResetEnemySpeed(EnemyMovement enemy, float duration)
    {
        while (true)
        {
            if (lastSlowHitTime.ContainsKey(enemy))
            {
                if (Time.time >= lastSlowHitTime[enemy] + duration)
                {
                    enemy.ResetSpeed();
                    Debug.Log($"{enemy.gameObject.name} speed reset at time {Time.time}");
                    lastSlowHitTime.Remove(enemy);
                    break;
                }
            }
            else break;

            yield return null;
        }

        if (activeResetCoroutines.ContainsKey(enemy))
            activeResetCoroutines.Remove(enemy);
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

        if (upgradeClip != null)
            PlaySound(upgradeClip);

        Debug.Log("New APS: " + aps);
        Debug.Log("New Range: " + targetingRange);
        Debug.Log("New Cost: " + CalculateCost());
    }

    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1.3f));
    }

    private float CalculateAttackSpeed()
    {
        return apsBase * Mathf.Pow(level, 0.1f);
    }

    private float CalculateRange()
    {
        return targetingRange * Mathf.Pow(level, 0.12f);
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        else
            Debug.LogWarning("Sprite or sprite array is missing!");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float volume = Random.Range(volumeMin, volumeMax);
            audioSource.PlayOneShot(clip, volume);
        }
    }
}

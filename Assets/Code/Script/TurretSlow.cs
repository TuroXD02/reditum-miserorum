using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

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
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private float apsBase;
    private float timeUntilFire;
    private int level = 1;

    private static Dictionary<EnemyMovement, Coroutine> activeResetCoroutines = new Dictionary<EnemyMovement, Coroutine>();
    private static Dictionary<EnemyMovement, float> lastSlowHitTime = new Dictionary<EnemyMovement, float>();

    private void Start()
    {
        apsBase = aps;
        upgradeButton.onClick.AddListener(Upgrade);

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

                EnemySlowEffect slowEffect = em.GetComponent<EnemySlowEffect>() ?? em.gameObject.AddComponent<EnemySlowEffect>();
                slowEffect.ApplySlowEffect(enemyBlueColor, tintLerpDuration, freezeTime);
            }
        }

        if (freezeEffectPrefab != null)
        {
            GameObject turretEffect = Instantiate(freezeEffectPrefab, transform.position, Quaternion.identity, transform);
            if (turretEffect.TryGetComponent(out Animator effectAnim) && freezeAnimatorController != null)
                effectAnim.runtimeAnimatorController = freezeAnimatorController;

            turretEffect.transform.localScale = new Vector3(targetingRange / 2f, targetingRange / 2f, 1f);
            Destroy(turretEffect, freezeEffectDuration);
        }

        PlaySound(freezeClip);
    }

    private IEnumerator ResetEnemySpeed(EnemyMovement enemy, float duration)
    {
        while (lastSlowHitTime.ContainsKey(enemy) && Time.time < lastSlowHitTime[enemy] + duration)
        {
            yield return null;
        }

        if (enemy != null)
        {
            enemy.ResetSpeed();
            Debug.Log($"{enemy.name} speed reset at {Time.time}");
        }

        lastSlowHitTime.Remove(enemy);
        activeResetCoroutines.Remove(enemy);
    }

    public void OpenUpgradeUI() => upgradeUI.SetActive(true);

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

        PlaySound(upgradeClip);
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1.3f));

    private float CalculateAttackSpeed() => apsBase * Mathf.Pow(level, 0.1f);

    private float CalculateRange() => targetingRange * Mathf.Pow(level, 0.12f);

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        else
            Debug.LogWarning("Sprite or upgradeSprites missing");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        float volume = Random.Range(volumeMin, volumeMax);
        if (audioSource != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
            audioSource.PlayOneShot(clip, volume);
        }
    }
}

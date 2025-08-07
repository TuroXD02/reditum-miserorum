using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TurretSlow : Turret
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

    [Header("Freeze Effect Animation")]
    [SerializeField] private GameObject freezeEffectPrefab;
    [SerializeField] private RuntimeAnimatorController freezeAnimatorController;
    [SerializeField] private float freezeEffectDuration = 2f;

    [Header("Enemy Visual FX")]
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

    // Internals
    private float apsBase;
    private float targetingRangeBase;
    private int level = 1;
    private float timeUntilFire;

    public int GetLevel() => level;
    public int BaseCost => baseUpgradeCost;
    public float CalculateBPS(int level) => 1f / CalculateAPS(level);

    private static Dictionary<EnemyMovement, Coroutine> activeResetCoroutines = new();
    private static Dictionary<EnemyMovement, float> lastSlowHitTime = new();

    protected override void Start()
    {
        base.Start(); // Call base implementation
        apsBase = aps;
        targetingRangeBase = targetingRange;
        upgradeButton.onClick.AddListener(Upgrade);
        PlaySound(placedClip);
    }

    protected override void Update()
    {
        base.Update(); // Maintains activeTime tracking
        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / aps)
        {
            Freeze();
            timeUntilFire = 0f;
        }
    }

    private void Freeze()
    {
        var hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);

        foreach (var hit in hits)
        {
            if (hit.transform.GetComponent<SlowImmunity>() != null) continue;

            var em = hit.transform.GetComponent<EnemyMovement>();
            if (em == null) continue;

            em.UpdateSpeed(0.1f);
            lastSlowHitTime[em] = Time.time;

            if (!activeResetCoroutines.ContainsKey(em))
                activeResetCoroutines[em] = StartCoroutine(ResetEnemySpeed(em, freezeTime));

            if (enemyVisualEffectPrefab != null)
            {
                GameObject effect = Instantiate(enemyVisualEffectPrefab, hit.transform.position, Quaternion.identity, hit.transform);
                Destroy(effect, enemyEffectDuration);
            }

            var slowEffect = em.GetComponent<EnemySlowEffect>() ?? em.gameObject.AddComponent<EnemySlowEffect>();
            slowEffect.ApplySlowEffect(enemyBlueColor, tintLerpDuration, freezeTime);
        }

        if (freezeEffectPrefab != null)
        {
            GameObject effect = Instantiate(freezeEffectPrefab, transform.position, Quaternion.identity, transform);
            if (effect.TryGetComponent(out Animator anim) && freezeAnimatorController != null)
                anim.runtimeAnimatorController = freezeAnimatorController;

            effect.transform.localScale = Vector3.one * (targetingRange / 2f);
            Destroy(effect, freezeEffectDuration);
        }

        PlaySound(freezeClip);
    }

    private IEnumerator ResetEnemySpeed(EnemyMovement enemy, float duration)
    {
        while (lastSlowHitTime.ContainsKey(enemy) && Time.time < lastSlowHitTime[enemy] + duration)
            yield return null;

        if (enemy != null)
            enemy.ResetSpeed();

        lastSlowHitTime.Remove(enemy);
        activeResetCoroutines.Remove(enemy);
    }

    public void OpenUpgradeUI() => upgradeUI.SetActive(true);

    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false);
    }

    public void Upgrade()
    {
        if (CalculateCost() > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(CalculateCost());
        level++;

        aps = CalculateAPS(level);
        targetingRange = CalculateRange(level);

        UpdateSprite();
        PlaySound(upgradeClip);
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1.3f));

    public float CalculateAPS(int lvl) => apsBase * Mathf.Pow(lvl, 0.1f);
    public float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.12f);

    public float CalculateAPS() => CalculateAPS(level);
    public float CalculateRange() => CalculateRange(level);

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        float volume = Random.Range(volumeMin, volumeMax);
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.PlayOneShot(clip, volume);
    }
    
    // Add override keyword and move to the bottom of the class
    public override float CalculateCurrentDPS() => 0f;
}
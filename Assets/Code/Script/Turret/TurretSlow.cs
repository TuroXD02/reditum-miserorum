using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class TurretSlow : Turret
{
    [Header("Slow turret specific")]
    [SerializeField] private float aps = 1f; // pulses per second
    [SerializeField] private float freezeTime = 2f;

    [Header("Visuals & FX")]
    [SerializeField] private GameObject freezeEffectPrefab;
    [SerializeField] private RuntimeAnimatorController freezeAnimatorController;
    [SerializeField] private float freezeEffectDuration = 2f;
    [SerializeField] private GameObject enemyVisualEffectPrefab;
    [SerializeField] private float enemyEffectDuration = 3f;
    [SerializeField] private Color enemyBlueColor = new Color(0.7f, 0.7f, 1f, 1f);
    [SerializeField] private float tintLerpDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip freezeClip;
    [SerializeField] private AudioClip placedClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    // Cached base values
    private float apsBase;

    // Local firing timer
    private float timeUntilFire;

    // --- Scaling methods ---
    // Increased exponent for more noticeable upgrades
    public float CalculateAPS(int lvl) => apsBase * Mathf.Pow(lvl, 0.4f);
    public override float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.25f);
    public override float CalculateBPS(int lvl) => CalculateAPS(lvl); // UI compatibility

    protected override void Start()
    {
        // Cache before calling base
        apsBase = aps;
        base.Start();

        // Ensure bps matches initial aps
        bps = aps;
        PlaySound(placedClip);
    }

    protected override void Update()
    {
        // Increment timer
        timeUntilFire += Time.deltaTime;

        // Fire based on APS (via bps mapping)
        if (timeUntilFire >= 1f / bps)
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

            // Slow to 10% of normal speed
            em.UpdateSpeed(0.1f);

            // Enemy visual FX
            if (enemyVisualEffectPrefab != null)
            {
                GameObject effect = Instantiate(enemyVisualEffectPrefab, hit.transform.position, Quaternion.identity, hit.transform);
                Destroy(effect, enemyEffectDuration);
            }

            // Tint + slow effect
            var slowEffect = em.GetComponent<EnemySlowEffect>() ?? em.gameObject.AddComponent<EnemySlowEffect>();
            slowEffect.ApplySlowEffect(enemyBlueColor, tintLerpDuration, freezeTime);
        }

        // Turret visual FX
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

    protected override void OnUpgraded()
    {
        // Update APS scaling
        aps = CalculateAPS(level);
        bps = aps; // ensure turret timing matches new APS

        Debug.Log($"Upgraded Slow Turret (Lvl {level}): APS={aps:F2}, Range={targetingRange:F2}");
    }

    public int CalculateCostForThisTurret() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1.3f));

    public override float CalculateCurrentDPS() => 0f; // DPS not applicable for slow turret
}

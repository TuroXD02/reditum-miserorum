using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class TurretLongRange : Turret
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private Sprite[] upgradeSprites;

    [Header("Attributes")]
    public float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bps; // bullets per second (fire rate)
    public int baseUpgradeCost;
    [SerializeField] private int bulletDamage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip sellClip;
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    [Header("Audio Randomization")]
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

    [Header("Shooting Timing")]
    [SerializeField] private float shootSoundDelay = 0.1f;

    [Header("UI Display")]
    // You can assign either TextMeshProUGUI OR Text in the inspector (or both).
    [SerializeField] private TextMeshProUGUI killsText_TMP;
    [SerializeField] private TextMeshProUGUI dpsText_TMP;
    [SerializeField] private Text killsText_UI;
    [SerializeField] private Text dpsText_UI;

    [Header("DPS display multipliers (shown after DPS)")]
    [SerializeField] private float dpsMinMultiplier = 1f;
    [SerializeField] private float dpsMaxMultiplier = 350f;

    // Cached base stats for upgrade scaling
    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;

    private Transform target;
    private float timeSinceLastShot = 0f;
    private int level = 1;

    // To prevent multiple shooting coroutines stacking
    private bool isShooting = false;

    public int GetLevel() => level;
    public int BaseCost => baseUpgradeCost;

    private void Start()
    {
        // Cache base values so CalculateBPS/CalculateBulletDamage use them as scaling base
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(Upgrade);

        UpdateSprite();
        PlaySound(placeClip);

        // Quick inspector sanity log
        if (dpsText_TMP == null && dpsText_UI == null)
            Debug.LogWarning($"{name}: Neither dpsText_TMP nor dpsText_UI is assigned on {nameof(TurretLongRange)}. DPS suffix won't be visible until you assign a UI Text or TextMeshProUGUI.");
    }

    private void Update()
    {
        // activeTime comes from base Turret (if you keep that), incrementing here keeps compatibility
        activeTime += Time.deltaTime;

        UpdateUI();

        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if (!IsTargetInRange())
        {
            target = null;
            return;
        }

        timeSinceLastShot += Time.deltaTime;
        // Guard bps to avoid divide-by-zero
        float rate = Mathf.Max(0.0001f, bps);
        if (!isShooting && timeSinceLastShot >= 1f / rate)
        {
            StartCoroutine(ShootWithDelay(shootSoundDelay));
            timeSinceLastShot = 0f;
        }
    }

    private IEnumerator ShootWithDelay(float delay)
    {
        isShooting = true;

        PlaySound(shootClip);
        yield return new WaitForSeconds(delay);

        if (target != null && bulletPrefab != null && firingPoint != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
            LongRangeBullet bulletScript = bulletObj.GetComponent<LongRangeBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(target);
                bulletScript.SetDamage(bulletDamage);
                bulletScript.SetOwner(this);
                Debug.Log($"[{name}] Shooting bullet (damage={bulletDamage}) at {target.name}");
            }
        }

        isShooting = false;
    }

    private void FindTarget()
    {
        var hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length > 0)
            target = hits[0].transform;
    }

    private bool IsTargetInRange() =>
        target != null && Vector2.Distance(target.position, transform.position) <= targetingRange;

    private void RotateTowardsTarget()
    {
        if (target == null || turretRotationPoint == null) return;
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion desiredRotation = Quaternion.Euler(0, 0, angle);
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }

    private void UpdateUI()
    {
        // Prepare the suffix once
        string suffix = $" (x{dpsMinMultiplier} min dist. - x{dpsMaxMultiplier} max dist)";

        // Get DPS value (stat-based: damage * fire rate)
        float dpsValue = CalculateCurrentDPS();

        // Format strings
        string killsStr = $"Kills: {KillCount}";
        string dpsStr = $"DPS: {dpsValue:F1}" + suffix;

        // Update TMP if assigned
        if (killsText_TMP != null) killsText_TMP.text = killsStr;
        if (dpsText_TMP != null) dpsText_TMP.text = dpsStr;

        // Update UnityEngine.UI.Text if assigned
        if (killsText_UI != null) killsText_UI.text = killsStr;
        if (dpsText_UI != null) dpsText_UI.text = dpsStr;
    }

    public void OpenUpgradeUI() => upgradeUI?.SetActive(true);

    public void CloseUpgradeUI()
    {
        upgradeUI?.SetActive(false);
        UiManager.main?.SetHoveringState(false);
    }

    public void Upgrade()
    {
        int cost = CalculateCost();
        if (cost > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(cost);
        level++;

        // Recalculate stats using cached base values so upgrades are multiplicative from original base
        bps = CalculateBPS(level);
        targetingRange = CalculateRange(level);
        bulletDamage = CalculateBulletDamage(level);

        UpdateSprite();
        PlaySound(upgradeClip);

        Debug.Log($"[{name}] Upgraded to level {level}: BPS={bps}, Range={targetingRange}, Damage={bulletDamage}");
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1.5f));

    // Upgrade scaling formulas (base * multiplier)
    public float CalculateBPS(int lvl) => bpsBase * (1f + 0.15f * (lvl - 1));
    public float CalculateRange(int lvl) => targetingRangeBase * (1f + 0.25f * (lvl - 1));
    public int CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * (1f + 0.4f * (lvl - 1)));

    private void UpdateSprite()
    {
        int spriteIndex = Mathf.Clamp(level - 1, 0, upgradeSprites != null ? upgradeSprites.Length - 1 : 0);
        if (turretSpriteRenderer != null && upgradeSprites != null && spriteIndex < upgradeSprites.Length)
            turretSpriteRenderer.sprite = upgradeSprites[spriteIndex];
    }

    public void PlaySellSound() => PlaySound(sellClip);

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        float volume = Random.Range(volumeMin, volumeMax);
        if (audioSource != null && audioSource.outputAudioMixerGroup != null)
            audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.PlayOneShot(clip, volume);
    }

    // DPS here derived from current stats (bulletDamage * bps)
    public override float CalculateCurrentDPS()
    {
        // Guard bps to avoid non-sense values
        float rate = Mathf.Max(0f, bps);
        return bulletDamage * rate;
    }
}

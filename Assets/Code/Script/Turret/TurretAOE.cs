using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TurretAreaDamage : Turret
{
    // UI / stats event
    public event Action OnStatsUpdated;

    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject areaBulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button upgradeConfirmButton; // optional confirm inside UI
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private Sprite[] upgradeSprites;

    [Header("Attributes (forwarded to base)")]
    [SerializeField] private float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bps;              // bullets (shots) per second
    [SerializeField] private int bulletDamage;      // damage per bullet
    [SerializeField] private float aoeRadius;
    [SerializeField] private int baseUpgradeCost;

    [Header("Audio (forwarded)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip sellClip;
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("UI Stats Display")]
    [SerializeField] private Text killsText;     // assign in Inspector (or use TMP)
    [SerializeField] private Text dpsText;       // assign in Inspector (or use TMP)
    [SerializeField] private float statsRefreshInterval = 0.5f; // seconds

    // subclass cached base-values are already in base (bpsBase, targetingRangeBase, bulletDamageBase)
    private float aoeRadiusBaseLocal; // we'll forward this into base by storing locally then forwarding

    // local helpers
    private float lastUpgradeTime = -10f;
    private const float upgradeGuardSeconds = 0.2f;

    #region Properties (expose base values as before)
    public int CurrentKills => KillCount; // uses base.KillCount
    public float CurrentDPS => CalculateCurrentDPS();
    public float CurrentRange => targetingRange;
    public float CurrentAOERadius => aoeRadius;
    #endregion

    private void Start()
    {
        // --- Forward references to base BEFORE base.Start() ---
        base.turretRotationPoint = this.turretRotationPoint;
        base.enemyMask = this.enemyMask;
        base.bulletPrefab = null; // this turret uses areaBulletPrefab in Shoot(), keep base.bulletPrefab null to avoid confusion
        base.firingPoint = this.firingPoint;
        base.upgradeUI = this.upgradeUI;
        base.upgradeButton = this.upgradeButton;
        base.turretSpriteRenderer = this.turretSpriteRenderer;
        base.towerStates = this.upgradeSprites;

        // --- Forward attributes to base ---
        base.targetingRange = this.targetingRange;
        base.rotationSpeed = this.rotationSpeed;
        base.bps = this.bps;
        base.baseUpgradeCost = this.baseUpgradeCost;
        base.bulletDamage = this.bulletDamage;

        // forward audio fields
        base.audioSource = this.audioSource;
        base.shootClip = this.shootClip;
        base.placeClip = this.placeClip;
        base.upgradeClip = this.upgradeClip;
        base.sellClip = this.sellClip;
        base.volumeMin = this.volumeMin;
        base.volumeMax = this.volumeMax;
        base.audioMixerGroup = this.sfxMixerGroup;

        // forward aoe base value (there is no aoeBase in Turret, keep local base)
        aoeRadiusBaseLocal = aoeRadius;

        // Now call base.Start() so it caches base stats and wires the default buttons.
        base.Start();

        // After base.Start() we can wire confirm button (if you have one in the UI)
        if (upgradeConfirmButton != null)
        {
            upgradeConfirmButton.onClick.RemoveAllListeners();
            upgradeConfirmButton.onClick.AddListener(OnUpgradeConfirmClicked);
        }

        // subscribe UI update event
        OnStatsUpdated += UpdateStatsUI;

        // initial UI and sprite
        UpdateSprite();
        OnStatsUpdated?.Invoke();
        StartCoroutine(StatsRefreshCoroutine());
    }

    private void OnDestroy()
    {
        // cleanup listeners
        if (upgradeConfirmButton != null)
            upgradeConfirmButton.onClick.RemoveListener(OnUpgradeConfirmClicked);
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OpenUpgradeUI); // base wired it, make sure it's removed too

        OnStatsUpdated -= UpdateStatsUI;
        StopAllCoroutines();
    }

    private IEnumerator StatsRefreshCoroutine()
    {
        while (true)
        {
            OnStatsUpdated?.Invoke();
            yield return new WaitForSeconds(statsRefreshInterval);
        }
    }

    private void OnUpgradeConfirmClicked()
    {
        int cost = CalculateCost();
        int currency = (LevelManager.main != null) ? LevelManager.main.currency : -1;
        Debug.Log($"[TurretAreaDamage] Upgrade confirm clicked. cost={cost}, playerCurrency={currency}, level={GetLevel()}");
        Upgrade();
    }

    private void Update()
    {
        activeTime += Time.deltaTime;

        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if (!CheckTargetIsInRange())
        {
            target = null;
            return;
        }

        timeUntilFire += Time.deltaTime;
        float safeBps = Mathf.Max(FireRateEpsilon, bps);
        if (timeUntilFire >= 1f / safeBps)
        {
            Shoot();
            timeUntilFire = 0f;
        }
    }

    #region Targeting / shooting

    private void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyMask);
        if (hits.Length == 0) return;

        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (var h in hits)
        {
            if (h == null || h.transform == null) continue;
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < closestDistance)
            {
                closest = h.transform;
                closestDistance = d;
            }
        }
        target = closest;
    }

    private bool CheckTargetIsInRange() => target != null && Vector2.Distance(target.position, transform.position) <= targetingRange;

    private void RotateTowardsTarget()
    {
        if (target == null || turretRotationPoint == null) return;

        Vector3 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, rot, rotationSpeed * Time.deltaTime);
    }

    public override void Shoot()
    {
        if (areaBulletPrefab == null || firingPoint == null || target == null) return;

        GameObject bulletObj = Instantiate(areaBulletPrefab, firingPoint.position, Quaternion.identity);
        AreaDamageBullet bulletScript = bulletObj.GetComponent<AreaDamageBullet>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(target);
            bulletScript.SetDamage(bulletDamage);
            bulletScript.SetAOERadius(aoeRadius);
            bulletScript.SetSourceTurret(this);
        }

        PlaySound(shootClip);
    }

    #endregion

    #region Upgrades & scaling

    // Small guard to avoid accidental double-upgrades (keeps your previous guard)
    public override void Upgrade()
    {
        if (Time.time - lastUpgradeTime < upgradeGuardSeconds) return;
        lastUpgradeTime = Time.time;

        if (LevelManager.main == null) return;

        int cost = CalculateCost();
        if (cost > LevelManager.main.currency) 
        {
            Debug.Log($"[TurretAreaDamage] Upgrade blocked: cost {cost} > currency {LevelManager.main.currency}");
            return;
        }

        LevelManager.main.SpendCurrency(cost);
        totalInvested += cost;

        // increment base level (protected in Turret)
        level++;

        // Recalculate using base cached base-values (bpsBase/targetingRangeBase/bulletDamageBase)
        bps = CalculateBPS(level);
        targetingRange = CalculateRange(level);
        bulletDamage = CalculateBulletDamage(level);
        aoeRadius = CalculateAOERadius(level);

        UpdateSprite();
        PlaySound(upgradeClip);
        OnStatsUpdated?.Invoke();

        Debug.Log($"[TurretAreaDamage] Upgraded to level {level}: bps={bps}, range={targetingRange}, dmg={bulletDamage}, aoe={aoeRadius}");
    }

    // Cost and scaling (use base cached values where appropriate)
    public new int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1.2f));
    public float CalculateBPS(int lvl) => bpsBase * Mathf.Pow(lvl, 0.3f);
    public float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.2f);
    public int CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 0.5f));
    public float CalculateAOERadius(int lvl) => aoeRadiusBaseLocal * Mathf.Pow(lvl, 0.3f);

    // convenience wrappers
    public float CalculateBPS() => CalculateBPS(level);
    public float CalculateRange() => CalculateRange(level);
    public int CalculateBulletDamage() => CalculateBulletDamage(level);
    public float CalculateAOERadius() => CalculateAOERadius(level);

    #endregion

    #region UI & visuals

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        else
            base.UpdateSprite();
    }

    private void UpdateStatsUI()
    {
        if (killsText != null)
            killsText.text = $"Kills: {KillCount}";

        if (dpsText != null)
            dpsText.text = $"DPS: {CurrentDPS:F1}";
    }

    public void OpenUpgradeUI() => upgradeUI?.SetActive(true);

    public void CloseUpgradeUI()
    {
        if (upgradeUI != null) upgradeUI.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false);
    }

    public void PlaySellSound() => PlaySound(sellClip);

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TurretPoison : Turret
{
    [Header("Poison - unique")]
    [SerializeField] private GameObject poisonBulletPrefab;
    [SerializeField] private Sprite[] upgradeSprites; // optional sprite per level
    [SerializeField] private float rotationSpeedLocal = 5f;
    [SerializeField] private float rotationOffset = -90f;

    [Header("Optional UI wiring")]
    [Tooltip("Button inside the upgrade UI that actually confirms the upgrade.")]
    [SerializeField] private Button upgradeConfirmButton;

    [Header("Optional audio overrides (will be forwarded to base)")]
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip sellClipOverride; // optional, will be forwarded to base.sellClip
    [SerializeField] private float volumeMinLocal = 0.9f, volumeMaxLocal = 1.1f;

    // NOTE: We intentionally do NOT redeclare bps/targetingRange/bulletDamage/level etc.
    // We'll forward inspector values to the base fields below so base.InitializeBaseStats() caches correct bases.

    private void Start()
    {
        // --- Forward subclass inspector settings into base BEFORE calling base.Start() ---

        // Forward sprite/tower state fields if you want base.UpdateSprite fallback to work
        if (upgradeSprites != null && upgradeSprites.Length > 0)
            base.towerStates = upgradeSprites;

        // forward audio overrides
        if (sellClipOverride != null) base.sellClip = sellClipOverride;
        if (mixerGroup != null) base.audioMixerGroup = mixerGroup;

        // Forward volume if you want the base randomization to use these values
        base.volumeMin = volumeMinLocal;
        base.volumeMax = volumeMaxLocal;

        // IMPORTANT: if you set base.bps/targetingRange/bulletDamage from inspector on this component
        // (e.g., through the base serialized fields in the inspector), you don't have to forward them here.
        // If you prefer to expose them only on this subclass, forward them like:
        // base.bps = this.someBps; base.targetingRange = this.someRange; base.bulletDamage = this.someDamage;

        // Now call the base start so it caches base stats and wires the default UI buttons.
        base.Start();

        // After base.Start() we can wire additional UI (confirm button inside upgrade UI).
        if (upgradeConfirmButton != null)
        {
            upgradeConfirmButton.onClick.RemoveAllListeners();
            upgradeConfirmButton.onClick.AddListener(OnUpgradeConfirmClicked);
        }
        // If you prefer the turret's external upgrade button to directly perform upgrades (instead of opening UI),
        // you can re-wire base.upgradeButton here (danger: this replaces open-UI behavior):
        // if (base.upgradeButton != null) { base.upgradeButton.onClick.RemoveAllListeners(); base.upgradeButton.onClick.AddListener(OnUpgradeConfirmClicked); }

        // Ensure audioSource exists and use mixer if provided
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
        if (mixerGroup != null)
            audioSource.outputAudioMixerGroup = mixerGroup;

        UpdateSprite();
    }

    private void OnUpgradeConfirmClicked()
    {
        int cost = CalculateCost();
        int currency = (LevelManager.main != null) ? LevelManager.main.currency : -1;
        Debug.Log($"[TurretPoison] Upgrade confirm clicked. cost={cost}, playerCurrency={currency}, level={GetLevel()}");
        Upgrade(); // call the (possibly overridden) Upgrade method
    }

    private void Update()
    {
        // Use base timing and targeting helpers where possible
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

    // override Shoot to spawn poison bullet prefab instead of base bulletPrefab
    public override void Shoot()
    {
        if (poisonBulletPrefab == null || firingPoint == null || target == null) return;

        Debug.Log($"[TurretPoison] Shooting bullet (damage={bulletDamage}), DPS before shot: {CalculateCurrentDPS()}");

        GameObject bulletObj = Instantiate(poisonBulletPrefab, firingPoint.position, Quaternion.identity);

        if (bulletObj.TryGetComponent(out PoisonBullet pb))
        {
            pb.SetTarget(target);
            pb.SetDamage(bulletDamage);
            pb.SetOwner(this);
        }

        RegisterShot();
        PlaySound(shootClip);

        Debug.Log($"[TurretPoison] Shot fired. DPS after shot: {CalculateCurrentDPS()}");
    }

    public override float CalculateCurrentDPS()
    {
        return bulletDamage * bps;
    }

    private void RotateTowardsTarget()
    {
        if (target == null || turretRotationPoint == null) return;

        Vector3 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        turretRotationPoint.rotation = Quaternion.Lerp(turretRotationPoint.rotation, targetRotation, Time.deltaTime * rotationSpeedLocal);
    }

    // Update sprite using the subclass upgradeSprites (if provided)
    protected void UpdateSprite()
    {
        // Use the base.level (protected) for indexing
        int idx = Mathf.Clamp(level - 1, 0, (upgradeSprites != null && upgradeSprites.Length > 0) ? upgradeSprites.Length - 1 : (towerStates != null ? towerStates.Length - 1 : 0));

        if (turretSpriteRenderer != null && upgradeSprites != null && upgradeSprites.Length > 0)
        {
            turretSpriteRenderer.sprite = upgradeSprites[idx];
        }
        else
        {
            base.UpdateSprite();
        }
    }

    // Poison-specific scaling (these override the base virtual methods)
    public override float CalculateBPS(int lvl) => bpsBase * Mathf.Pow(lvl, 1f);
    public override float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.55f);
    public override int CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 1.5f));

    // Provide a poison-specific cost formula — base.CalculateCost() is non-virtual, so we 'new' it here.
    // Note: base.Upgrade() uses base.CalculateCost(). If you want your custom cost to actually be used,
    // override Upgrade() and implement the cost check there (see comment below).
    public new int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));

    // Use base.Upgrade() so level++ and recalculation use your overridden Calculate* methods.
    // If you want custom cost to be applied, replace this with a full override (see comment).
    public override void Upgrade()
    {
        Debug.Log($"[TurretPoison] Attempting Upgrade at level {level} using base.Upgrade()");
        base.Upgrade(); // handles currency check (using base.CalculateCost()), level++, and recalculation via overridden Calculate*
        UpdateSprite();
        PlaySound(upgradeClip);
        Debug.Log($"[TurretPoison] Upgraded to level {level}: bps={bps}, range={targetingRange}, dmg={bulletDamage}");
    }

    public void PlaySellSound() => PlaySound(sellClipOverride ?? sellClip);
}

using UnityEngine;
using UnityEngine.UI;

public class TurretArmourBreaker : Turret
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private Sprite[] towerStates;

    [Header("Attributes")]
    [SerializeField] private int armourReduction;
    private int armourReductionBase;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip sellClip;
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

    // Stats & UI
    public int KillCount { get; private set; } = 0;
    public float TotalDamageDealt { get; private set; } = 0f;
    public event System.Action OnStatsUpdated;

    protected override void Start()
    {
        base.Start();

        armourReductionBase = armourReduction;
        armourReduction = CalculateArmourReduction(level);

        // Ensure ONLY this turret's Upgrade is bound
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners(); // remove any old bindings (base or inspector)
            upgradeButton.onClick.AddListener(() => Upgrade());
        }

        UpdateSprite();
    }


    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(Upgrade);
    }

    protected override void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        if (!CheckTargetIsInRange())
        {
            target = null;
            return;
        }

        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / bps)
        {
            Shoot();
            timeUntilFire = 0f;
        }
    }

    public override void Shoot()
    {
        if (bulletPrefab == null || firingPoint == null || target == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

        if (bulletObj.TryGetComponent(out ArmourBreakerBullet bullet))
        {
            bullet.SetTarget(target);
            bullet.SetDamage(bulletDamage);
            bullet.SetArmourReduction(armourReduction);
            bullet.SetSourceTurret(this);
            bullet.transform.localScale = Vector3.one * Mathf.Max(0.1f, targetingRange / 5f);
        }

        RegisterShot();
        PlaySound(shootClip);
    }

    public override void Upgrade()
    {
        base.Upgrade(); // Handles currency check, level++, and stat recalculation
        armourReduction = CalculateArmourReduction(level);
        UpdateSprite();
        PlaySound(upgradeClip);
        OnStatsUpdated?.Invoke();

        Debug.Log($"ArmourBreaker upgraded to level {level}: bps={bps}, range={targetingRange}, dmg={bulletDamage}, armRed={armourReduction}");
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && towerStates != null && towerStates.Length > 0)
        {
            int idx = Mathf.Clamp(level - 1, 0, towerStates.Length - 1);
            turretSpriteRenderer.sprite = towerStates[idx];
        }
    }

    public override float CalculateCurrentDPS() => bulletDamage * bps;

    public int CalculateArmourReduction(int lvl) =>
        Mathf.RoundToInt(armourReductionBase * Mathf.Pow(lvl, 0.3f));

    public override float CalculateBPS(int lvl) =>
        bpsBase * Mathf.Pow(lvl, 1f);

    public override float CalculateRange(int lvl) =>
        targetingRangeBase * Mathf.Pow(lvl, 0.1f);

    public override int CalculateBulletDamage(int lvl) =>
        Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 0.4f));

    public string DamageStats => $"DMG: {bulletDamage} | ARM: -{armourReduction}";

    public void RegisterDamage(int damage)
    {
        TotalDamageDealt += damage;
        OnStatsUpdated?.Invoke();
    }

    public void RegisterKill()
    {
        KillCount++;
        OnStatsUpdated?.Invoke();
    }

    public void OpenUpgradeUI() => upgradeUI?.SetActive(true);

    public void CloseUpgradeUI()
    {
        if (upgradeUI != null) upgradeUI.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false);
    }

    public void PlaySellSound() => PlaySound(sellClip);

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float vol = Random.Range(volumeMin, volumeMax);
            audioSource.PlayOneShot(clip, vol);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}

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

    // Stats & UI event
    public int KillCount { get; private set; } = 0;
    public float TotalDamageDealt { get; private set; } = 0f;
    public event System.Action OnStatsUpdated;

    protected override void Start()
    {
        // Initialize base class fields properly
        base.Start(); // Call base Start first
        
        // Cache base armour reduction value
        armourReductionBase = armourReduction;
        
        // Set initial armour reduction using base value
        armourReduction = CalculateArmourReduction(level);
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
        ArmourBreakerBullet bullet = bulletObj.GetComponent<ArmourBreakerBullet>();

        if (bullet != null)
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
        base.Upgrade(); // Use base upgrade functionality
        armourReduction = CalculateArmourReduction(level); // Recalculate after upgrade
        OnStatsUpdated?.Invoke();
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && towerStates != null && level - 1 < towerStates.Length)
            turretSpriteRenderer.sprite = towerStates[level - 1];
    }

    public override float CalculateCurrentDPS()
    {
        // DPS = damage per bullet * bullets per second
        return bulletDamage * bps;
    }

    // Armour reduction scaling
    public int CalculateArmourReduction(int lvl)
    {
        return Mathf.RoundToInt(armourReductionBase * Mathf.Pow(lvl, 0.3f));
    }

    // Override the base class methods to calculate the correct stats with your scaling
    public override float CalculateBPS(int lvl)
    {
        // Use your own scaling if needed or default
        return bpsBase * Mathf.Pow(lvl, 1f);
    }

    public override float CalculateRange(int lvl)
    {
        return targetingRangeBase * Mathf.Pow(lvl, 0.1f);
    }

    public override int CalculateBulletDamage(int lvl)
    {
        return Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 0.4f));
    }

    // For UI display
    public string DamageStats => $"DMG: {bulletDamage} | ARM: -{armourReduction}";

    // Register damage & kills (can be called from bullet scripts)
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

using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform turretRotationPoint;
    [SerializeField] protected LayerMask enemyMask;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firingPoint;
    [SerializeField] protected GameObject upgradeUI;
    [SerializeField] protected Button upgradeButton;
    [SerializeField] protected SpriteRenderer turretSpriteRenderer;
    [SerializeField] protected Sprite[] towerStates;

    [Header("Attributes")]
    [SerializeField] protected float targetingRange; // read-only via property
    [SerializeField] protected float rotationSpeed;
    [SerializeField] protected float bps; // Bullets per second
    [SerializeField] public int baseUpgradeCost;
    [SerializeField] protected int bulletDamage;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip shootClip;
    [SerializeField] protected AudioClip placeClip;
    [SerializeField] protected AudioClip upgradeClip;

    [Header("Audio Randomization")]
    [SerializeField] protected float volumeMin = 0.9f;
    [SerializeField] protected float volumeMax = 1.1f;

    // Cached base stats for scaling
    protected float bpsBase;
    protected float targetingRangeBase;
    protected int bulletDamageBase;

    // Performance tracking
    public int KillCount { get; protected set; } = 0;
    public float TotalDamageDealt { get; protected set; } = 0f;
    protected float activeTime = 0f;

    // Track bullets fired for DPS calculation (optional)
    protected int bulletsFired = 0;

    // Targeting & shooting
    protected Transform target;
    protected float timeUntilFire;
    protected int level = 1;

    // Read-only access
    public float TargetingRange => targetingRange;

    // === DPS: you can override this; default calculates using average damage per fired bullet * bps
    public virtual float CalculateCurrentDPS()
    {
        return bulletDamage * bps;
    }
    public void RegisterKill() => KillCount++;
    public void RegisterDamage(int damage) => TotalDamageDealt += damage;
    public void RecordDamage(float damage) => RegisterDamage(Mathf.RoundToInt(damage));
    public void RecordKill() => RegisterKill();

    // Called each time the turret actually shoots a bullet (subclasses that override Shoot must call this)
    protected void RegisterShot() => bulletsFired++;

    public int BaseCost => baseUpgradeCost;
    public int GetLevel() => level;

    public void OpenUpgradeUI() => upgradeUI?.SetActive(true);

    public void CloseUpgradeUI()
    {
        upgradeUI?.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false);
    }

    // === Unity lifecycle ===
    protected virtual void Start()
    {
        InitializeBaseStats();
        SetupUpgradeButton();
        UpdateSprite();
        PlaySound(placeClip);
    }

    protected virtual void Update()
    {
        activeTime += Time.deltaTime;

        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();
        HandleShooting();
    }

    // --- initialization helpers ---
    protected void InitializeBaseStats()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;
    }

    // NOTE: upgradeButton opens the upgrade UI (avoid calling Upgrade() directly here to prevent double-invokes)
    protected void SetupUpgradeButton()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OpenUpgradeUI);
        }
    }

    // === Targeting & shooting ===
    protected void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length > 0)
        {
            Transform closest = null;
            float closestDistance = Mathf.Infinity;
            foreach (var h in hits)
            {
                float d = Vector2.Distance(transform.position, h.transform.position);
                if (d < closestDistance)
                {
                    closest = h.transform;
                    closestDistance = d;
                }
            }
            target = closest;
        }
    }

    protected bool CheckTargetIsInRange()
    {
        return target != null && Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    protected void RotateTowardsTarget()
    {
        if (target == null || turretRotationPoint == null) return;
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion desired = Quaternion.Euler(0f, 0f, angle);
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, desired, rotationSpeed * Time.deltaTime);
    }

    protected void HandleShooting()
    {
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

    public virtual void Shoot()
    {
        if (bulletPrefab == null || firingPoint == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

        if (bulletObj.TryGetComponent(out Bullet b))
        {
            b.SetTarget(target);
            b.SetDamage(bulletDamage);
            b.SetOwner(this);
        }
        else if (bulletObj.TryGetComponent(out LongRangeBullet lrb))
        {
            lrb.SetTarget(target);
            lrb.SetDamage(bulletDamage);
            lrb.SetOwner(this);
        }

        RegisterShot();
        PlaySound(shootClip);
    }

    // === Upgrades ===
    public virtual void Upgrade()
    {
        int cost = CalculateCost();
        if (cost > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(cost);
        level++;

        // Recalculate stats using virtual Calculate* methods (subclasses can override)
        bps = CalculateBPS(level);
        targetingRange = CalculateRange(level);
        bulletDamage = CalculateBulletDamage(level);

        UpdateSprite();
        PlaySound(upgradeClip);

        // Hook for subclasses to update additional fields
        OnUpgraded();
    }

    // Optional hook for subclasses (was missing before -> caused override error)
    protected virtual void OnUpgraded() { }

    protected void UpdateSprite()
    {
        if (turretSpriteRenderer == null || towerStates == null || towerStates.Length == 0) return;
        int idx = Mathf.Clamp(level - 1, 0, towerStates.Length - 1);
        turretSpriteRenderer.sprite = towerStates[idx];
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));

    public virtual float CalculateBPS(int lvl) => bpsBase * Mathf.Pow(lvl, 1f);
    public virtual float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.1f);
    public virtual int CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 0.4f));

    protected void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        float vol = Random.Range(volumeMin, volumeMax);
        audioSource.PlayOneShot(clip, vol);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}

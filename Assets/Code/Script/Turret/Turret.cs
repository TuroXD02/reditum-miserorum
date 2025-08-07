using UnityEngine;
using UnityEngine.UI;

public class Turret : MonoBehaviour
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
    public float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bps; // Bullets per second
    public int baseUpgradeCost; // Made public for accessibility
    [SerializeField] private int bulletDamage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip upgradeClip;

    [Header("Audio Randomization")]
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;
    
    // Stats properties - made protected for access in derived classes
    public int KillCount { get; protected set; } = 0;
    public float TotalDamageDealt { get; protected set; } = 0f;
    protected float activeTime = 0f;
    
    private Transform target;
    private float timeUntilFire;
    private int level = 1;
    
    public void RegisterKill() => KillCount++;
    public void RegisterDamage(int damage) => TotalDamageDealt += damage;
    public virtual float CalculateCurrentDPS()
    {
        return (activeTime > 0f) ? TotalDamageDealt / activeTime : 0f;
    }
    public void OpenUpgradeUI() => upgradeUI.SetActive(true);
    public int BaseCost => baseUpgradeCost;
    public int GetLevel() => level;

    protected virtual void Start()
    {
        InitializeBaseStats();
        SetupUpgradeButton();
        UpdateSprite();
        PlaySound(placeClip);
    }
    
    private void InitializeBaseStats()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;
    }

    private void SetupUpgradeButton()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(Upgrade);
        }
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

    private void RotateTowardsTarget()
    {
        if (target == null) return;

        Vector2 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        turretRotationPoint.rotation = Quaternion.RotateTowards(
            turretRotationPoint.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleShooting()
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

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position,
            targetingRange,
            Vector2.zero,
            0f,
            enemyMask
        );

        if (hits.Length > 0)
        {
            Transform closest = null;
            float closestDistance = Mathf.Infinity;

            foreach (RaycastHit2D hit in hits)
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closest = hit.transform;
                    closestDistance = distance;
                }
            }

            target = closest;
        }
    }

    private bool CheckTargetIsInRange()
    {
        return target != null &&
               Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    public void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();
        bulletScript.SetTarget(target);
        bulletScript.SetDamage(bulletDamage);
        bulletScript.SetOwner(this); // Assign this turret as the owner
        PlaySound(shootClip);
    }

    /// <summary>
    /// Called by Bullet when it deals damage
    /// </summary>
    public void RecordDamage(float damage)
    {
        RegisterDamage(Mathf.RoundToInt(damage));
    }

    /// <summary>
    /// Called by Bullet when it scores a kill
    /// </summary>
    public void RecordKill()
    {
        RegisterKill();
    }
    

    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false);
    }

    public void Upgrade()
    {
        int cost = CalculateCost();
        if (cost > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(cost);
        level++;

        bps = CalculateBPS(level);
        targetingRange = CalculateRange(level);
        bulletDamage = CalculateBulletDamage(level);

        UpdateSprite();
        PlaySound(upgradeClip);
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer == null || towerStates.Length == 0) return;

        int spriteIndex = Mathf.Clamp(level - 1, 0, towerStates.Length - 1);
        turretSpriteRenderer.sprite = towerStates[spriteIndex];
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    public float CalculateBPS(int level) => bpsBase * Mathf.Pow(level, 1f);
    public float CalculateRange(int level) => targetingRangeBase * Mathf.Pow(level, 0.1f);
    public int CalculateBulletDamage(int level) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 0.4f));

    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        float randomVolume = Random.Range(volumeMin, volumeMax);
        audioSource.PlayOneShot(clip, randomVolume);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}
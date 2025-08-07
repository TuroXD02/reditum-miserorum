using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TurretAreaDamage : Turret
{
    // Add combat statistics properties
    public int KillCount { get; private set; } = 0;
    public float TotalDamageDealt { get; private set; } = 0f;
    private float activeTime = 0f;
    
    // Existing references
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject areaBulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private Sprite[] upgradeSprites;

    [Header("Attributes")]
    [SerializeField] public float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bps;
    [SerializeField] private int bulletDamage;
    [SerializeField] private float aoeRadius;
    [SerializeField] private int baseUpgradeCost;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip sellClip;
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;
    private float aoeRadiusBase;

    private Transform target;
    private float timeUntilFire;
    private int level = 1;

    public int GetLevel() => level;
    public int BaseCost => baseUpgradeCost;
    
    // Add DPS calculation property
    public float CalculateCurrentDPS() => 
        (activeTime > 0) ? TotalDamageDealt / activeTime : 0;

    private void Start()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;
        aoeRadiusBase = aoeRadius;

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(Upgrade);
            
        UpdateSprite();
        PlaySound(placeClip);
    }

    private void Update()
    {
        // Track active time for DPS calculation
        activeTime += Time.deltaTime;
        
        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if (!IsTargetInRange())
        {
            target = null;
        }
        else
        {
            timeUntilFire += Time.deltaTime;
            if (timeUntilFire >= 1f / bps)
            {
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }

    // Add methods to register damage and kills
    public void RegisterDamage(int damage) => TotalDamageDealt += damage;
    public void RegisterKill() => KillCount++;

    private void Shoot()
    {
        GameObject bulletObj = Instantiate(areaBulletPrefab, firingPoint.position, Quaternion.identity);
        AreaDamageBullet bulletScript = bulletObj.GetComponent<AreaDamageBullet>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(target);
            bulletScript.SetDamage(bulletDamage);
            bulletScript.SetAOERadius(aoeRadius);
            
            // Add reference to this turret for damage/kill tracking
            bulletScript.SetSourceTurret(this);
        }

        PlaySound(shootClip);
    }

    private void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyMask);
        if (hits.Length > 0)
        {
            // Find closest enemy
            Transform closest = null;
            float closestDistance = Mathf.Infinity;
            
            foreach (Collider2D hit in hits)
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

    private bool IsTargetInRange()
    {
        return target != null && 
               Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget()
    {
        Vector3 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion rot = Quaternion.Euler(0, 0, angle);
        turretRotationPoint.rotation = Quaternion.RotateTowards(
            turretRotationPoint.rotation,
            rot,
            rotationSpeed * Time.deltaTime
        );
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
        aoeRadius = CalculateAOERadius(level);

        UpdateSprite();
        PlaySound(upgradeClip);
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1.2f));
    public float CalculateBPS(int lvl) => bpsBase * Mathf.Pow(lvl, 0.3f);
    public float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.2f);
    public int CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 0.5f));
    public float CalculateAOERadius(int lvl) => aoeRadiusBase * Mathf.Pow(lvl, 0.3f);

    // For UI previews
    public float CalculateBPS() => CalculateBPS(level);
    public float CalculateRange() => CalculateRange(level);
    public int CalculateBulletDamage() => CalculateBulletDamage(level);
    public float CalculateAOERadius() => CalculateAOERadius(level);

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
        {
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        }
    }

    public void OpenUpgradeUI() => upgradeUI.SetActive(true);

    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false); // Added null check
    }

    public void PlaySellSound() => PlaySound(sellClip);

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        float volume = Random.Range(volumeMin, volumeMax);
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.PlayOneShot(clip, volume);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}
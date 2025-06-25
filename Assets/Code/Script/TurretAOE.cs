using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretAreaDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;   // Point for turret rotation
    [SerializeField] private LayerMask enemyMask;             // Mask to detect enemies
    [SerializeField] private GameObject areaBulletPrefab;     // Prefab for area-damage bullet
    [SerializeField] private Transform firingPoint;           // Firing position
    [SerializeField] private GameObject upgradeUI;            // Upgrade UI panel
    [SerializeField] private Button upgradeButton;            // Button to trigger upgrades
    [SerializeField] private SpriteRenderer turretSpriteRenderer; // For visual changes
    [SerializeField] private Sprite[] upgradeSprites;         // Sprites for different levels

    [Header("Attributes")]
    [SerializeField] public float targetingRange;  // Range for acquiring targets
    [SerializeField] private float rotationSpeed;   // How quickly the turret rotates
    [SerializeField] private float bps;             // Bullets per second (firing rate)
    [SerializeField] public int baseUpgradeCost;     // Base cost to upgrade
    [SerializeField] private int bulletDamage;        // Base damage per bullet
    [SerializeField] private float aoeRadius;         // Explosion (AOE) radius for area damage

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // AudioSource component
    [SerializeField] private AudioClip shootClip;     // Sound played when turret shoots
    [SerializeField] private AudioClip placeClip;     // Sound played when turret is placed
    [SerializeField] private AudioClip upgradeClip;   // Sound played on upgrade
    [SerializeField] private AudioClip sellClip;      // Sound played when turret is sold

    [Header("Audio Randomization")]
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;
    private float aoeRadiusBase;

    private Transform target;
    private float timeUntilFire;
    private int level = 1;

    private void Start()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;
        aoeRadiusBase = aoeRadius;

        upgradeButton.onClick.AddListener(Upgrade);

        UpdateSprite();
        PlaySound(placeClip);
    }

    private void Update()
    {
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

    private void Shoot()
    {
        GameObject bulletObj = Instantiate(areaBulletPrefab, firingPoint.position, Quaternion.identity);
        AreaDamageBullet bulletScript = bulletObj.GetComponent<AreaDamageBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetTarget(target);
            bulletScript.SetDamage(bulletDamage);
            bulletScript.SetAOERadius(aoeRadius);
        }

        PlaySound(shootClip);
    }

    private void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyMask);
        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    private bool IsTargetInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget()
    {
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void OpenUpgradeUI()
    {
        upgradeUI.SetActive(true);
    }

    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        UiManager.main.SetHoveringState(false);
    }

    public void Upgrade()
    {
        if (CalculateCost() > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(CalculateCost());
        level++;

        bps = CalculateBPS();
        targetingRange = CalculateRange();
        bulletDamage = CalculateBulletDamage();
        aoeRadius = CalculateAOERadius();

        UpdateSprite();
        CloseUpgradeUI();

        PlaySound(upgradeClip);
    }

    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1.2f));
    }

    private float CalculateBPS()
    {
        return bpsBase * Mathf.Pow(level, 0.3f);
    }

    private float CalculateRange()
    {
        return targetingRangeBase * Mathf.Pow(level, 0.2f);
    }

    private int CalculateBulletDamage()
    {
        return Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 0.5f));
    }

    private float CalculateAOERadius()
    {
        return aoeRadiusBase * Mathf.Pow(level, 0.3f);
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
        {
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        }
        else
        {
            Debug.LogWarning("Turret sprite or upgrade sprites are missing!");
        }
    }

    public void PlaySellSound()
    {
        PlaySound(sellClip);
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float randomVolume = Random.Range(volumeMin, volumeMax);
            audioSource.PlayOneShot(clip, randomVolume);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}

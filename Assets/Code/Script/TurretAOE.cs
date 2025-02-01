using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretAreaDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;   // Point for turret rotation
    [SerializeField] private LayerMask enemyMask;               // Mask to detect enemies
    [SerializeField] private GameObject areaBulletPrefab;       // Prefab for area-damage bullet
    [SerializeField] private Transform firingPoint;             // Firing position
    [SerializeField] private GameObject upgradeUI;              // Upgrade UI panel
    [SerializeField] private Button upgradeButton;              // Button to trigger upgrades
    [SerializeField] private SpriteRenderer turretSpriteRenderer; // For visual changes
    [SerializeField] private Sprite[] upgradeSprites;           // Sprites for different levels

    [Header("Attributes")]
    [SerializeField] public float targetingRange;  // Range for acquiring targets
    [SerializeField] private float rotationSpeed;   // How quickly the turret rotates
    [SerializeField] private float bps;             // Bullets per second (firing rate)
    [SerializeField] public int baseUpgradeCost;     // Base cost to upgrade
    [SerializeField] private int bulletDamage;        // Base damage per bullet
    [SerializeField] private float aoeRadius;         // Explosion (AOE) radius for area damage

    // Base values used for calculating upgrades
    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;
    private float aoeRadiusBase;

    private Transform target;      // Current enemy target
    private float timeUntilFire;   // Timer for firing control
    private int level = 1;         // Current turret level

    private void Start()
    {
        // Store base values for use in upgrade calculations
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;
        aoeRadiusBase = aoeRadius;

        // Hook up the upgrade method to the upgrade button
        upgradeButton.onClick.AddListener(Upgrade);

        // Optionally, set the initial sprite
        UpdateSprite();
    }

    private void Update()
    {
        // If no target, attempt to find one
        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        // If the target has moved out of range, clear it
        if (!IsTargetInRange())
        {
            target = null;
        }
        else
        {
            // Update firing timer and shoot when ready
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
        // Instantiate the area-damage bullet at the firing point
        GameObject bulletObj = Instantiate(areaBulletPrefab, firingPoint.position, Quaternion.identity);

        // Get the AreaDamageBullet script to set its parameters
        AreaDamageBullet bulletScript = bulletObj.GetComponent<AreaDamageBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetTarget(target);
            bulletScript.SetDamage(bulletDamage);
            bulletScript.SetAOERadius(aoeRadius);
        }
    }

    private void FindTarget()
    {
        // Use an OverlapCircle to find enemies within targetingRange
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
        // Calculate the direction and desired rotation
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Smoothly rotate the turret toward the target
        turretRotationPoint.rotation = Quaternion.RotateTowards(
            turretRotationPoint.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
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
        // Ensure the player has enough currency to upgrade
        if (CalculateCost() > LevelManager.main.currency)
            return;

        LevelManager.main.SpendCurrency(CalculateCost());
        level++;

        // Recalculate turret attributes based on the new level
        bps = CalculateBPS();
        targetingRange = CalculateRange();
        bulletDamage = CalculateBulletDamage();
        aoeRadius = CalculateAOERadius();

        UpdateSprite();
        CloseUpgradeUI();
    }

    // Upgrade cost scales with level (you can adjust the exponent as needed)
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

    // For debugging: visualize the targeting range in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}

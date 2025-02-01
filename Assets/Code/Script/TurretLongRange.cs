using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretLongRange : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint; // Point for turret rotation
    [SerializeField] private LayerMask enemyMask; // Layer containing enemies
    [SerializeField] private GameObject bulletPrefab; // Bullet prefab to shoot
    [SerializeField] private Transform firingPoint; // Point from which bullets are fired
    [SerializeField] private GameObject upgradeUI; // UI for turret upgrades
    [SerializeField] private Button upgradeButton; // Button for upgrading
    [SerializeField] private SpriteRenderer turretSpriteRenderer; // Reference to SpriteRenderer for visual changes
    [SerializeField] private Sprite[] upgradeSprites; // Array of sprites representing turret levels

    [Header("Attributes")]
    [SerializeField] public float targetingRange; // Range to detect enemies
    [SerializeField] private float rotationSpeed; // Speed of turret rotation
    [SerializeField] private float bps; // Bullets per second
    [SerializeField] public int baseUpgradeCost; // Base cost of upgrades
    [SerializeField] private int bulletDamage; // Bullet damage value

    private float bpsBase; // Base bullets-per-second value
    private float targetingRangeBase; // Base targeting range value
    private int bulletDamageBase; // Base bullet damage value

    private Transform target; // Reference to the current target
    private float timeUntilFire; // Timer to manage fire rate

    private int level = 1; // Current level of the turret

    private void Start()
    {
        // Save base values for upgrades
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;

        // Attach the upgrade method to the upgrade button
        upgradeButton.onClick.AddListener(Upgrade);
    }

    private void Update()
    {
        // If there is no target, find one
        if (target == null)
        {
            FindTarget();
            return;
        }

        // Rotate the turret towards the target
        RotateTowardsTarget();

        // Check if the target is still in range
        if (!CheckTargetIsInRange())
        {
            target = null; // Reset target if it's out of range
        }
        else
        {
            // Update the timer for firing
            timeUntilFire += Time.deltaTime;

            // If enough time has passed, shoot a bullet
            if (timeUntilFire >= 1f / bps)
            {
                Shoot();
                timeUntilFire = 0f; // Reset the timer
            }
        }
    }

    private void Shoot()
    {
        // Instantiate a new bullet at the firing point
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

        // Attach the LongRangeBullet script to control its behavior
        LongRangeBullet bulletScript = bulletObj.GetComponent<LongRangeBullet>();

        // Set the target and damage for the bullet
        bulletScript.SetTarget(target);
        bulletScript.SetDamage(bulletDamage);
    }

    private void FindTarget()
    {
        // Use a CircleCast to find enemies in the targeting range
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position, // Center of the detection circle
            targetingRange,     // Radius of the detection circle
            Vector2.zero,       // No movement
            0f,                 // Circle is stationary
            enemyMask           // Detect only objects on the "enemy" layer
        );

        // If any enemies are found, set the first one as the target
        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    private bool CheckTargetIsInRange()
    {
        // Check if the target is within the turret's range
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget()
    {
        // Calculate the angle between the turret and the target
        float angle = Mathf.Atan2(
            target.position.y - transform.position.y, // Difference in y-coordinates
            target.position.x - transform.position.x  // Difference in x-coordinates
        ) * Mathf.Rad2Deg - 90f; // Convert from radians to degrees and align with turret

        // Rotate the turret smoothly towards the target
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        turretRotationPoint.rotation = Quaternion.RotateTowards(
            turretRotationPoint.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime // Smooth rotation based on frame rate
        );
    }

    public void OpenUpgradeUI()
    {
        // Show the upgrade UI
        upgradeUI.SetActive(true);
    }

    public void CloseUpgradeUI()
    {
        // Hide the upgrade UI and reset the cursor state
        upgradeUI.SetActive(false);
        UiManager.main.SetHoveringState(false);
    }

    public void Upgrade()
    {
        // Check if the player has enough currency for the upgrade
        if (CalculateCost() > LevelManager.main.currency) return;

        // Deduct the currency and increment the turret level
        LevelManager.main.SpendCurrency(CalculateCost());
        level++;

        // Update turret attributes based on the new level
        bps = CalculateBPS();
        targetingRange = CalculateRange();
        bulletDamage = CalculateBulletDamage();

        // Update the turret's sprite to reflect the new level
        UpdateSprite();

        // Close the upgrade UI after upgrading
        CloseUpgradeUI();
    }

    public int CalculateCost()
    {
        // Calculate the upgrade cost based on the turret's level
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1f));
    }

    private float CalculateBPS()
    {
        // Increase bullets per second based on the turret's level
        return bpsBase * Mathf.Pow(level, 0.4f);
    }

    private float CalculateRange()
    {
        // Increase targeting range based on the turret's level
        return targetingRangeBase * Mathf.Pow(level, 0.4f);
    }

    private int CalculateBulletDamage()
    {
        // Increase bullet damage based on the turret's level
        return Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 0.55f));
    }

    private void UpdateSprite()
    {
        // Update the sprite based on the current turret level
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
        {
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        }
        else
        {
            Debug.LogWarning("Sprite or sprite array is missing!");
        }
    }
}

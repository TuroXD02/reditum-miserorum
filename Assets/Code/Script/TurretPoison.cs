using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretPoison : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint; // Point for turret rotation
    [SerializeField] private LayerMask enemyMask; // Layer to detect enemies
    [SerializeField] private GameObject poisonBulletPrefab; // Prefab for poison bullets
    [SerializeField] private Transform firingPoint; // Point to spawn bullets
    [SerializeField] private GameObject upgradeUI; // Upgrade menu UI
    [SerializeField] private Button upgradeButton; // Button to handle upgrades
    [SerializeField] private SpriteRenderer turretSpriteRenderer; // Renderer to change turret sprite
    [SerializeField] private Sprite[] upgradeSprites; // Array of sprites for turret levels

    [Header("Attributes")]
    [SerializeField] public float targetingRange; // Range to detect enemies
    [SerializeField] private float rotationSpeed; // Speed of turret rotation
    [SerializeField] private float bps; // Bullets per second
    [SerializeField] public int baseUpgradeCost; // Base upgrade cost
    [SerializeField] private int bulletDamage; // Base damage for bullets

    private float bpsBase; // Base bullet-per-second value
    private float targetingRangeBase; // Base targeting range value
    private int bulletDamageBase; // Base damage value for bullets
    private Transform target; // Current target
    private float timeUntilFire; // Timer for next fire
    private int level = 1; // Current level of the turret

    private void Start()
    {
        // Store initial values for scaling upgrades
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;

        // Attach Upgrade method to the button
        upgradeButton.onClick.AddListener(Upgrade);
    }

    private void Update()
    {
        // If no target, find one
        if (target == null)
        {
            FindTarget();
            return;
        }

        // Rotate turret to face target
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        turretRotationPoint.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));

        // If target is out of range, clear it
        if (!CheckTargetIsInRange())
        {
            target = null;
        }
        else
        {
            // Manage firing timer
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
        // Instantiate a poison bullet
        GameObject bulletObj = Instantiate(poisonBulletPrefab, firingPoint.position, Quaternion.identity);

        // Set target and damage for the bullet
        PoisonBullet bulletScript = bulletObj.GetComponent<PoisonBullet>();
        bulletScript.SetTarget(target);
        bulletScript.SetDamage(bulletDamage);
    }

    private void FindTarget()
    {
        // Use CircleCast to detect enemies
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);

        // If enemies are found, set the first one as the target
        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    private bool CheckTargetIsInRange()
    {
        // Check if the current target is within range
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    public void Upgrade()
    {
        // Check if there is enough currency to upgrade
        if (CalculateCost() > LevelManager.main.currency) return;

        // Spend currency and increase level
        LevelManager.main.SpendCurrency(CalculateCost());
        level++;

        // Update attributes
        bps = CalculateBPS();
        targetingRange = CalculateRange();
        bulletDamage = CalculateBulletDamage();

        // Update the turret's sprite to reflect the new level
        UpdateSprite();

        // Close the upgrade UI
        CloseUpgradeUI();
    }

    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }

    private float CalculateBPS()
    {
        return bpsBase * Mathf.Pow(level, 1f);
    }

    private float CalculateRange()
    {
        return targetingRangeBase * Mathf.Pow(level, 0.55f);
    }

    private int CalculateBulletDamage()
    {
        return Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 1.5f));
    }

    private void UpdateSprite()
    {
        // Change the turret sprite to match the new level
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
        {
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        }
        else
        {
            Debug.LogWarning("Sprite or sprite array is missing!");
        }
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
}

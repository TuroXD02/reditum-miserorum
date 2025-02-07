using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretArmourBreaker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint; // Rotating part of the turret
    [SerializeField] private LayerMask enemyMask; // Layer Mask for enemy detection
    [SerializeField] private GameObject bulletPrefab; // Slash animation bullet
    [SerializeField] private Transform firingPoint; // Position where slashes originate
    [SerializeField] private GameObject upgradeUI; // UI for turret upgrades
    [SerializeField] private Button upgradeButton; // Button for upgrading the turret
    [SerializeField] private SpriteRenderer turretSpriteRenderer; // Sprite Renderer for the turret
    [SerializeField] private Sprite[] towerStates; // Sprites for turret levels

    [Header("Attributes")]
    [SerializeField] public float targetingRange; // How far the turret can hit enemies
    [SerializeField] private float rotationSpeed; // Speed of turret rotation
    [SerializeField] private float attacksPerSecond; // Attack frequency
    [SerializeField] public int baseUpgradeCost; // Base cost for upgrading
    [SerializeField] private int attackDamage; // Damage per hit
    [SerializeField] private int armourReduction; // Amount of armour reduced per hit

    // Private values to track base stats for upgrades
    private float baseAttacksPerSecond;
    private float baseTargetingRange;
    private int baseAttackDamage;
    private int baseArmourReduction;

    private Transform target; // Current target enemy
    private float attackCooldown; // Timer for managing attack speed

    private int level = 1; // Current turret level

    private void Start()
    {
        // Save base stats for upgrade calculations
        baseAttacksPerSecond = attacksPerSecond;
        baseTargetingRange = targetingRange;
        baseAttackDamage = attackDamage;
        baseArmourReduction = armourReduction;

        // Assign upgrade function to the button
        upgradeButton.onClick.AddListener(Upgrade);

        // Set initial sprite
        UpdateSprite();
    }

    private void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        if (!IsTargetInRange())
        {
            target = null;
        }
        else
        {
            attackCooldown += Time.deltaTime;

            if (attackCooldown >= 1f / attacksPerSecond)
            {
                Attack();
                attackCooldown = 0f;
            }
        }
    }

    private void Attack()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        ArmourBreakerBullet bullet = bulletObj.GetComponent<ArmourBreakerBullet>();

        if (bullet != null)
        {
            bullet.SetTarget(target);
            bullet.SetDamage(attackDamage);
            bullet.SetArmourReduction(armourReduction);

            // Scale the slash effect based on targeting range
            float scaleFactor = targetingRange / 5f; // Adjust as needed
            bullet.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);

        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    private bool IsTargetInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
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

        attacksPerSecond = CalculateAttackSpeed();
        targetingRange = CalculateRange();
        attackDamage = CalculateDamage();
        armourReduction = CalculateArmourReduction();

        UpdateSprite();
        CloseUpgradeUI();

        Debug.Log("New APS:" + attacksPerSecond);
        Debug.Log("New Range:" + targetingRange);
        Debug.Log("New Damage:" + attackDamage);
        Debug.Log("New Armour Reduction:" + armourReduction);
        Debug.Log("New Cost:" + CalculateCost());
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && level - 1 < towerStates.Length)
        {
            turretSpriteRenderer.sprite = towerStates[level - 1];
        }
        else
        {
            Debug.LogWarning("Sprite not updated: Check sprite array or level.");
        }
    }

    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }

    private float CalculateAttackSpeed()
    {
        return baseAttacksPerSecond * Mathf.Pow(level, 1f);
    }

    private float CalculateRange()
    {
        return baseTargetingRange * Mathf.Pow(level, 0.1f);
    }

    private int CalculateDamage()
    {
        return Mathf.RoundToInt(baseAttackDamage * Mathf.Pow(level, 0.4f));
    }

    private int CalculateArmourReduction()
    {
        return Mathf.RoundToInt(baseArmourReduction * Mathf.Pow(level, 0.3f));
    }
}

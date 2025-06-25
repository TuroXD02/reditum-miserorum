using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretArmourBreaker : MonoBehaviour
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
    [SerializeField] public float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float attacksPerSecond;
    [SerializeField] public int baseUpgradeCost;
    [SerializeField] private int attackDamage;
    [SerializeField] private int armourReduction;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip sellClip;

    [Header("Audio Randomization")]
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

    // Base stats for upgrade calculations
    private float baseAttacksPerSecond;
    private float baseTargetingRange;
    private int baseAttackDamage;
    private int baseArmourReduction;

    private Transform target;
    private float attackCooldown;
    private int level = 1;

    private void Start()
    {
        baseAttacksPerSecond = attacksPerSecond;
        baseTargetingRange = targetingRange;
        baseAttackDamage = attackDamage;
        baseArmourReduction = armourReduction;

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

            float scaleFactor = targetingRange / 5f;
            bullet.transform.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }

        PlaySound(shootClip);
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

        PlaySound(upgradeClip);

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
}

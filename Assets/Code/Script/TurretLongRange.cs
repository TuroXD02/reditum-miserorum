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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // AudioSource component
    [SerializeField] private AudioClip shootClip; // Sound played when turret shoots
    [SerializeField] private AudioClip placeClip; // Sound played when turret is placed
    [SerializeField] private AudioClip upgradeClip; // Sound played on upgrade
    [SerializeField] private AudioClip sellClip; // Sound played when turret is sold

    [Header("Audio Randomization")]
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

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

        if (!CheckTargetIsInRange())
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
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        LongRangeBullet bulletScript = bulletObj.GetComponent<LongRangeBullet>();
        bulletScript.SetTarget(target);
        bulletScript.SetDamage(bulletDamage);

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

    private bool CheckTargetIsInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget()
    {
        float angle = Mathf.Atan2(target.position.y - transform.position.y, target.position.x - transform.position.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
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

        UpdateSprite();
        CloseUpgradeUI();

        PlaySound(upgradeClip);
    }

    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1f));
    }

    private float CalculateBPS()
    {
        return bpsBase * Mathf.Pow(level, 0.2f);
    }

    private float CalculateRange()
    {
        return targetingRangeBase * Mathf.Pow(level, 0.4f);
    }

    private int CalculateBulletDamage()
    {
        return Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 0.55f));
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
        {
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        }
        else
        {
            Debug.LogWarning("Sprite or sprite array is missing!");
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
}

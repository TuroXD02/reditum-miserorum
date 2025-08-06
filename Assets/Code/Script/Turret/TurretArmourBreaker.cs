using System.Collections;
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
    public float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float attacksPerSecond;
    public int baseUpgradeCost;
    [SerializeField] private int attackDamage;
    [SerializeField] private int armourReduction;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip sellClip;
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

    private float baseAPS, baseRange;
    private int baseDamage, baseArmourReduction;

    private Transform target;
    private float cooldown;
    private int level = 1;

    public int GetLevel() => level;
    public int BaseCost => baseUpgradeCost; // Added property
    public float CalculateBPS(int level) => 1f / CalculateAPS(level); // Added method

    private void Start()
    {
        baseAPS = attacksPerSecond;
        baseRange = targetingRange;
        baseDamage = attackDamage;
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

        if (!IsTargetInRange()) target = null;
        else
        {
            cooldown += Time.deltaTime;
            if (cooldown >= 1f / attacksPerSecond)
            {
                Attack();
                cooldown = 0f;
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
            bullet.transform.localScale = Vector3.one * (targetingRange / 5f);
        }

        PlaySound(shootClip);
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length > 0) target = hits[0].transform;
    }

    private bool IsTargetInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    public void Upgrade()
    {
        int cost = CalculateCost();
        if (cost > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(cost);
        level++;

        attacksPerSecond = CalculateAPS(level);
        targetingRange = CalculateRange(level);
        attackDamage = CalculateDamage(level);
        armourReduction = CalculateArmourReduction(level);

        UpdateSprite();
        PlaySound(upgradeClip);
    }

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && level - 1 < towerStates.Length)
            turretSpriteRenderer.sprite = towerStates[level - 1];
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));

    public float CalculateAPS() => CalculateAPS(level);
    public float CalculateRange() => CalculateRange(level);
    public int CalculateDamage() => CalculateDamage(level);
    public int CalculateArmourReduction() => CalculateArmourReduction(level);

    public float CalculateAPS(int lvl) => baseAPS * Mathf.Pow(lvl, 1f);
    public float CalculateRange(int lvl) => baseRange * Mathf.Pow(lvl, 0.1f);
    public int CalculateDamage(int lvl) => Mathf.RoundToInt(baseDamage * Mathf.Pow(lvl, 0.4f));
    public int CalculateArmourReduction(int lvl) => Mathf.RoundToInt(baseArmourReduction * Mathf.Pow(lvl, 0.3f));

    public void OpenUpgradeUI() => upgradeUI.SetActive(true);
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false);
    }

    public void PlaySellSound() => PlaySound(sellClip);

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            float vol = Random.Range(volumeMin, volumeMax);
            audioSource.PlayOneShot(clip, vol);
        }
    }
}
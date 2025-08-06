using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TurretPoison : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject poisonBulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private Sprite[] upgradeSprites;

    [Header("Attributes")]
    public float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bps;
    public int baseUpgradeCost;
    [SerializeField] private int bulletDamage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip, placeClip, upgradeClip, sellClip;
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private float volumeMin = 0.9f, volumeMax = 1.1f;

    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;
    private float timeUntilFire;
    private Transform target;
    private int level = 1;

    public int GetLevel() => level;
    public int BaseCost => baseUpgradeCost; // Added property

    private void Start()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;

        upgradeButton.onClick.AddListener(Upgrade);
        UpdateSprite();
        PlaySound(placeClip);
    }

    private void Update()
    {
        if (target == null) { FindTarget(); return; }

        RotateTowardsTarget();

        if (!IsTargetInRange()) { target = null; return; }

        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / bps)
        {
            Shoot();
            timeUntilFire = 0f;
        }
    }

    private void Shoot()
    {
        GameObject bulletObj = Instantiate(poisonBulletPrefab, firingPoint.position, Quaternion.identity);
        PoisonBullet bullet = bulletObj.GetComponent<PoisonBullet>();

        if (bullet != null)
        {
            bullet.SetTarget(target);
            bullet.SetDamage(bulletDamage);
        }

        Destroy(bulletObj, 8f);
        PlaySound(shootClip);
    }

    private void RotateTowardsTarget()
    {
        Vector3 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        turretRotationPoint.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length > 0) target = hits[0].transform;
    }

    private bool IsTargetInRange()
    {
        return Vector2.Distance(transform.position, target.position) <= targetingRange;
    }

    public void Upgrade()
    {
        if (CalculateCost() > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(CalculateCost());
        level++;

        bps = CalculateBPS(level);
        targetingRange = CalculateRange(level);
        bulletDamage = CalculateBulletDamage(level);

        UpdateSprite();
        PlaySound(upgradeClip);
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    public float CalculateBPS() => CalculateBPS(level);
    public float CalculateRange() => CalculateRange(level);
    public int CalculateBulletDamage() => CalculateBulletDamage(level);

    public float CalculateBPS(int lvl) => bpsBase * Mathf.Pow(lvl, 1f);
    public float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.55f);
    public int CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 1.5f));

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && level - 1 < upgradeSprites.Length)
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
    }

    public void OpenUpgradeUI() => upgradeUI.SetActive(true);
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false);
    }
    public void PlaySellSound() => PlaySound(sellClip);

    private void PlaySound(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        float vol = Random.Range(volumeMin, volumeMax);
        audioSource.outputAudioMixerGroup = mixerGroup;
        audioSource.PlayOneShot(clip, vol);
    }
}
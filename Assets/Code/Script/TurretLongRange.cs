using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TurretLongRange : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private GameObject upgradeUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private Sprite[] upgradeSprites;

    [Header("Attributes")]
    [SerializeField] public float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bps;
    [SerializeField] public int baseUpgradeCost;
    [SerializeField] private int bulletDamage;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip placeClip;
    [SerializeField] private AudioClip upgradeClip;
    [SerializeField] private AudioClip sellClip;
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    [Header("Audio Randomization")]
    [SerializeField] private float volumeMin = 0.9f;
    [SerializeField] private float volumeMax = 1.1f;

    [Header("Shooting Timing")]
    [SerializeField] private float shootSoundDelay = 0.1f;

    public int GetLevel() => level;
    // Internal state
    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;

    private Transform target;
    private float timeUntilFire;
    private int level = 1;

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
                StartCoroutine(ShootWithDelay(shootSoundDelay));
                timeUntilFire = 0f;
            }
        }
    }

    private IEnumerator ShootWithDelay(float delay)
    {
        PlaySound(shootClip); // Play shoot sound immediately

        yield return new WaitForSeconds(delay);

        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        LongRangeBullet bulletScript = bulletObj.GetComponent<LongRangeBullet>();

        if (bulletScript != null)
        {
            bulletScript.SetTarget(target);
            bulletScript.SetDamage(bulletDamage);
        }
        else
        {
            Debug.LogWarning("Missing LongRangeBullet component on bullet prefab.");
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

    private void RotateTowardsTarget()
    {
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion desiredRotation = Quaternion.Euler(0, 0, angle);
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
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
        int cost = CalculateCost();
        if (cost > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(cost);
        level++;

        bps = CalculateBPS();
        targetingRange = CalculateRange();
        bulletDamage = CalculateBulletDamage();

        UpdateSprite();
        CloseUpgradeUI();
        PlaySound(upgradeClip);
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1f));
    private float CalculateBPS() => bpsBase * Mathf.Pow(level, 0.2f);
    private float CalculateRange() => targetingRangeBase * Mathf.Pow(level, 0.4f);
    private int CalculateBulletDamage() => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 0.55f));

    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
        {
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        }
        else
        {
            Debug.LogWarning("Sprite or upgrade sprite array missing or index out of bounds.");
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
            audioSource.outputAudioMixerGroup = audioMixerGroup;
            audioSource.PlayOneShot(clip, randomVolume);
        }
    }
}

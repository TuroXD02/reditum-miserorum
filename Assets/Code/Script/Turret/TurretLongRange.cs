using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TurretLongRange : MonoBehaviour, IUpgradableTurret
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
    public float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bps;
    public int baseUpgradeCost;
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

    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;

    private Transform target;
    private float timeUntilFire;
    private int level = 1;

    public int GetLevel() => level;
    public int BaseCost => baseUpgradeCost;

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
        PlaySound(shootClip);
        yield return new WaitForSeconds(delay);

        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);
        LongRangeBullet bulletScript = bulletObj.GetComponent<LongRangeBullet>();
        if (bulletScript != null)
        {
            bulletScript.SetTarget(target);
            bulletScript.SetDamage(bulletDamage);
        }
    }

    private void FindTarget()
    {
        var hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length > 0) target = hits[0].transform;
    }

    private bool IsTargetInRange() => Vector2.Distance(target.position, transform.position) <= targetingRange;

    private void RotateTowardsTarget()
    {
        Vector3 dir = target.position - transform.position;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        Quaternion desired = Quaternion.Euler(0, 0, ang);
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, desired, rotationSpeed * Time.deltaTime);
    }

    public void OpenUpgradeUI() => upgradeUI.SetActive(true);
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        UiManager.main?.SetHoveringState(false);
    }

    public void Upgrade()
    {
        int cost = CalculateCost();
        if (cost > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(cost);
        level++;
        bps = CalculateBPS(level);
        targetingRange = CalculateRange(level);
        bulletDamage = CalculateBulletDamage(level);

        UpdateSprite();
        PlaySound(upgradeClip);
    }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 1f));
    public float CalculateBPS(int lvl)    => bpsBase * Mathf.Pow(lvl, 0.2f);
    public float CalculateRange(int lvl)  => targetingRangeBase * Mathf.Pow(lvl, 0.4f);
    public int   CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 0.55f));

    private void UpdateSprite()
    {
        int idx = level - 1;
        if (turretSpriteRenderer != null && upgradeSprites != null && idx < upgradeSprites.Length)
            turretSpriteRenderer.sprite = upgradeSprites[idx];
    }

    public void PlaySellSound() => PlaySound(sellClip);
    private void PlaySound(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        float vol = Random.Range(volumeMin, volumeMax);
        audioSource.outputAudioMixerGroup = audioMixerGroup;
        audioSource.PlayOneShot(clip, vol);
    }
}

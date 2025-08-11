using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Base turret class: targeting, shooting, upgrades and selling.
/// </summary>
public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Transform turretRotationPoint;
    [SerializeField] protected LayerMask enemyMask;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firingPoint;
    [SerializeField] protected GameObject upgradeUI;
    [SerializeField] protected Button upgradeButton;
    [SerializeField] protected Button sellButton;
    [SerializeField] protected SpriteRenderer turretSpriteRenderer;
    [SerializeField] protected Sprite[] towerStates;

    [Header("Attributes")]
    [SerializeField] protected float targetingRange;
    [SerializeField] protected float rotationSpeed;
    [SerializeField] protected float bps = 1f;
    [SerializeField] public int baseUpgradeCost = 10;
    [SerializeField] protected int bulletDamage = 1;
    [SerializeField] protected float rotationAngleOffset = 0f;

    [Header("Audio")]
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip shootClip;
    [SerializeField] protected AudioClip placeClip;
    [SerializeField] protected AudioClip upgradeClip;
    [SerializeField] protected AudioClip sellClip;
    [SerializeField] protected AudioMixerGroup audioMixerGroup;

    [Header("Audio Randomization")]
    [SerializeField] protected float volumeMin = 0.9f;
    [SerializeField] protected float volumeMax = 1.1f;

    [Header("Sell")]
    [SerializeField] [Range(0f, 1f)] protected float sellRefundMultiplier = 0.5f;

    // Cached base stats
    protected float bpsBase;
    protected float targetingRangeBase;
    protected int bulletDamageBase;

    // Performance tracking
    public int KillCount { get; protected set; } = 0;
    public float TotalDamageDealt { get; protected set; } = 0f;
    protected float activeTime = 0f;
    protected int bulletsFired = 0;

    // Targeting & shooting
    protected Transform target;
    protected float timeUntilFire;
    protected int level = 1;

    // Invested amount (placement + upgrades)
    protected int totalInvested = 0;

    public float TargetingRange => targetingRange;
    public int GetLevel() => level;
    public int BaseCost => baseUpgradeCost;

    protected const float FireRateEpsilon = 0.0001f;
    protected bool isPreview = false;

    // Event args and static event
    public class TurretSoldEventArgs
    {
        public Vector3 position;
        public AudioClip clip;
        public float volume;
        public AudioMixerGroup mixerGroup;
        public Turret source;

        public TurretSoldEventArgs(Vector3 pos, AudioClip clip, float vol, AudioMixerGroup mixer, Turret src)
        {
            position = pos;
            this.clip = clip;
            volume = vol;
            mixerGroup = mixer;
            source = src;
        }
    }

    public static event Action<TurretSoldEventArgs> OnTurretSold;

    #region Unity lifecycle

    protected virtual void Start()
    {
        InitializeBaseStats();
        SetupUpgradeButton();
        SetupSellButton();
        UpdateSprite();

        if (!isPreview)
            PlaySound(placeClip);

        totalInvested = baseUpgradeCost;
    }

    protected virtual void Update()
    {
        activeTime += Time.deltaTime;

        if (target == null)
            FindTarget();

        if (target == null)
            return;

        RotateTowardsTarget();
        HandleShooting();
    }

    #endregion

    #region Init helpers

    protected void InitializeBaseStats()
    {
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;
    }

    protected void SetupUpgradeButton()
    {
        if (upgradeButton == null) return;
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OpenUpgradeUI);
    }

    protected void SetupSellButton()
    {
        if (sellButton == null) return;
        sellButton.onClick.RemoveAllListeners();
        sellButton.onClick.AddListener(Sell);
    }

    public void SetPreview(bool preview) => isPreview = preview;

    public void OnPlaced(int placedCost)
    {
        totalInvested = placedCost;
        isPreview = false;
        PlaySound(placeClip);
    }

    #endregion

    #region Targeting & shooting

    protected void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);
        if (hits.Length == 0) return;

        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        foreach (var h in hits)
        {
            if (h.transform == null) continue;
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < closestDistance)
            {
                closest = h.transform;
                closestDistance = d;
            }
        }
        target = closest;
    }

    protected bool CheckTargetIsInRange() =>
        target != null && Vector2.Distance(target.position, transform.position) <= targetingRange;

    protected void RotateTowardsTarget()
    {
        if (target == null || turretRotationPoint == null) return;
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationAngleOffset;
        Quaternion desired = Quaternion.Euler(0f, 0f, angle);
        turretRotationPoint.rotation = Quaternion.RotateTowards(turretRotationPoint.rotation, desired, rotationSpeed * Time.deltaTime);
    }

    protected void HandleShooting()
    {
        if (!CheckTargetIsInRange())
        {
            target = null;
            return;
        }

        timeUntilFire += Time.deltaTime;
        float safeBps = Mathf.Max(FireRateEpsilon, bps);
        if (timeUntilFire >= 1f / safeBps)
        {
            Shoot();
            timeUntilFire = 0f;
        }
    }

    public virtual void Shoot()
    {
        if (bulletPrefab == null || firingPoint == null || target == null) return;

        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

        if (bulletObj.TryGetComponent(out Bullet b))
        {
            b.SetTarget(target);
            b.SetDamage(bulletDamage);
            b.SetOwner(this);
        }
        else if (bulletObj.TryGetComponent(out LongRangeBullet lrb))
        {
            lrb.SetTarget(target);
            lrb.SetDamage(bulletDamage);
            lrb.SetOwner(this);
        }

        RegisterShot();
        PlaySound(shootClip);
    }

    #endregion

    #region Stats & upgrades

    public virtual float CalculateCurrentDPS()
    {
        float safeBps = Mathf.Max(0f, bps);
        return bulletDamage * safeBps;
    }

    public void RegisterKill() => KillCount++;
    public void RegisterDamage(int damage) => TotalDamageDealt += damage;
    public void RecordDamage(float damage) => RegisterDamage(Mathf.RoundToInt(damage));
    public void RecordKill() => RegisterKill();
    protected void RegisterShot() => bulletsFired++;

    public virtual void Upgrade()
    {
        int cost = CalculateCost();
        if (LevelManager.main == null) return;
        if (cost > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(cost);
        totalInvested += cost;

        level++;

        bps = CalculateBPS(level);
        targetingRange = CalculateRange(level);
        bulletDamage = CalculateBulletDamage(level);

        UpdateSprite();
        PlaySound(upgradeClip);

        OnUpgraded();
    }

    protected virtual void OnUpgraded() { }

    public int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));

    public virtual float CalculateBPS(int lvl) => bpsBase * Mathf.Pow(lvl, 1f);
    public virtual float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.1f);
    public virtual int CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 0.4f));

    #endregion

    #region Selling

    public virtual int CalculateSellValue()
    {
        return Mathf.RoundToInt(totalInvested * sellRefundMultiplier);
    }

    public virtual void Sell()
    {
        Debug.Log($"[Turret] Sell() called on '{name}'. sellClip assigned? {(sellClip != null)} totalInvested={totalInvested}");

        // Refund player
        int refund = CalculateSellValue();
        if (LevelManager.main != null)
        {
            LevelManager.main.currency += refund;
            Debug.Log($"[Turret] Refunded {refund} to player.");
        }

        // Close UI and bookkeeping
        CloseUpgradeUI();
        OnSold();

        // Prepare event args
        float vol = UnityEngine.Random.Range(volumeMin, volumeMax);
        var args = new TurretSoldEventArgs(transform.position, sellClip, vol, audioMixerGroup, this);

        // Prefer raising global event; fallback to local persistent playback if nobody is listening.
        if (OnTurretSold != null)
        {
            OnTurretSold.Invoke(args);
            Debug.Log($"[Turret] Raised OnTurretSold event for '{name}'.");
        }
        else
        {
            Debug.Log($"[Turret] No subscribers to OnTurretSold; playing locally.");
            PlayPersistingClip(sellClip);
        }

        // Destroy turret
        Destroy(gameObject);
    }

    protected virtual void OnSold() { }

    protected void PlayPersistingClip(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[Turret] PlayPersistingClip called with null clip on '{name}'.");
            return;
        }

        float vol = UnityEngine.Random.Range(volumeMin, volumeMax);
        vol = Mathf.Clamp01(vol);

        GameObject tmp = new GameObject("OneShotAudio");
        tmp.transform.position = transform.position;
        AudioSource src = tmp.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = vol;
        src.spatialBlend = 0f;
        src.playOnAwake = false;

        if (audioMixerGroup != null)
            src.outputAudioMixerGroup = audioMixerGroup;
        else if (audioSource != null && audioSource.outputAudioMixerGroup != null)
            src.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;

        DontDestroyOnLoad(tmp);
        src.Play();
        Destroy(tmp, clip.length + 0.1f);
    }

    #endregion

    #region UI helpers

    public void OpenUpgradeUI() => upgradeUI?.SetActive(true);

    public void CloseUpgradeUI()
    {
        upgradeUI?.SetActive(false);
        if (UiManager.main) UiManager.main.SetHoveringState(false);
    }

    #endregion

    #region Visuals / audio helpers

    protected void UpdateSprite()
    {
        if (turretSpriteRenderer == null || towerStates == null || towerStates.Length == 0) return;
        int idx = Mathf.Clamp(level - 1, 0, towerStates.Length - 1);
        turretSpriteRenderer.sprite = towerStates[idx];
    }

    protected void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        float vol = UnityEngine.Random.Range(volumeMin, volumeMax);
        vol = Mathf.Clamp01(vol);

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip, vol);
        }
        else
        {
            // Fallback (no mixer control)
            AudioSource.PlayClipAtPoint(clip, transform.position, vol);
        }
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }
}

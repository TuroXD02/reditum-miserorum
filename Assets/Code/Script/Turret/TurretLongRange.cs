using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurretLongRange : Turret
{
    [Header("References (inspector)")]
    [SerializeField] private Transform turretRotationPoint;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firingPoint;
    [SerializeField] private GameObject upgradeUI;
    [Tooltip("Button that opens the upgrade UI (attached to the turret).")]
    [SerializeField] private Button upgradeButton;
    [Tooltip("Button inside the upgradeUI that actually confirms the upgrade.")]
    [SerializeField] private Button upgradeConfirmButton;
    [SerializeField] private SpriteRenderer turretSpriteRenderer;
    [SerializeField] private Sprite[] upgradeSprites;

    [Header("Attributes (inspector)")]
    [SerializeField] private float targetingRange;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bps; // bullets per second
    [SerializeField] private int baseUpgradeCost;
    [SerializeField] private int bulletDamage;

    // ... your other fields (audio, UI TMPs etc.) kept as needed ...

    // local state
    
    private float timeSinceLastShot;
    private bool isShooting;

    protected override void Start()
    {
        // --- Forward references to base protected fields BEFORE base.Start() ---
        base.turretRotationPoint = this.turretRotationPoint;
        base.enemyMask = this.enemyMask;
        base.bulletPrefab = this.bulletPrefab;
        base.firingPoint = this.firingPoint;
        base.upgradeUI = this.upgradeUI;
        base.upgradeButton = this.upgradeButton;
        base.turretSpriteRenderer = this.turretSpriteRenderer;
        base.towerStates = this.upgradeSprites;

        // --- Forward attributes ---
        base.targetingRange = this.targetingRange;
        base.rotationSpeed = this.rotationSpeed;
        base.bps = this.bps;
        base.baseUpgradeCost = this.baseUpgradeCost;
        base.bulletDamage = this.bulletDamage;

        // --- Forward any audio UI fields if you have them ---
        // base.audioSource = this.audioSource; etc...

        // Now call base.Start() so it initializes base stats and wires the default open-UI button.
        base.Start();

        // After base.Start() we can re-wire or wire extra UI buttons.
        // IMPORTANT: base.SetupUpgradeButton wired upgradeButton to OpenUpgradeUI().
        // We recommend having a separate "confirm upgrade" button inside the upgradeUI.
        if (upgradeConfirmButton != null)
        {
            upgradeConfirmButton.onClick.RemoveAllListeners();
            upgradeConfirmButton.onClick.AddListener(OnUpgradeConfirmClicked);
        }
        else
        {
            // If you only have one button and want it to perform the actual upgrade directly,
            // re-wire it here (warning: this replaces the open-UI behavior)
            // if (upgradeButton != null) { upgradeButton.onClick.RemoveAllListeners(); upgradeButton.onClick.AddListener(OnUpgradeConfirmClicked); }
        }

        UpdateSprite();
    }

    private void OnUpgradeConfirmClicked()
    {
        // Helpful debug: show cost + player's currency
        int cost = CalculateCost();
        int currency = (LevelManager.main != null) ? LevelManager.main.currency : -1;
        Debug.Log($"[TurretLongRange] Upgrade button pressed. cost={cost}, playerCurrency={currency}, level={GetLevel()}");

        Upgrade(); // call the base upgrade (we didn't override it here)
    }

    // Keep your shooting implementation — but make sure you DO NOT declare a separate `level` here.
// REMOVE this line from TurretLongRange
// private Transform target;

// In Update(), use base.target instead
    private void Update()
    {
        // track active time (base also does this, but harmless)
        activeTime += Time.deltaTime;

        // Ask base to find a target if none
        if (target == null)
        {
            FindTarget();
            Debug.Log($"[TurretLongRange] After FindTarget -> target = {(target==null ? "null" : target.name)}");
        }

        // still no target: bail
        if (target == null) return;

        // rotate & check range
        RotateTowardsTarget();

        if (!CheckTargetIsInRange())
        {
            Debug.Log($"[TurretLongRange] Target out of range. Clearing target.");
            target = null;
            return;
        }

        // shooting cadence
        timeSinceLastShot += Time.deltaTime;
        float rate = Mathf.Max(0.0001f, bps); // bps is your serialized inspector value
        if (!isShooting && timeSinceLastShot >= 1f / rate)
        {
            StartCoroutine(ShootWithDelay(0.1f));
            timeSinceLastShot = 0f;
        }
    }



    private IEnumerator ShootWithDelay(float delay)
    {
        isShooting = true;
        PlaySound(shootClip);
        yield return new WaitForSeconds(delay);

        // debug: inspector values
        Debug.Log($"[TurretLongRange] Attempting to shoot. target={(target==null? "null" : target.name)}, bulletPrefab={(bulletPrefab==null? "null" : bulletPrefab.name)}, firingPoint={(firingPoint==null? "null" : firingPoint.name)}");

        if (target != null && bulletPrefab != null && firingPoint != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

            // Try both bullet types
            if (bulletObj.TryGetComponent(out LongRangeBullet lrb))
            {
                lrb.SetTarget(target);
                lrb.SetDamage(bulletDamage);
                lrb.SetOwner(this);
                Debug.Log($"[{name}] Shooting LongRangeBullet (damage={bulletDamage}) at {target.name}");
            }
            else if (bulletObj.TryGetComponent(out Bullet b))
            {
                b.SetTarget(target);
                b.SetDamage(bulletDamage);
                b.SetOwner(this);
                Debug.Log($"[{name}] Shooting Bullet (damage={bulletDamage}) at {target.name}");
            }
            else
            {
                Debug.LogWarning($"[TurretLongRange] Bullet prefab '{bulletPrefab.name}' has no Bullet or LongRangeBullet component.");
            }

            RegisterShot();
        }
        else
        {
            Debug.LogWarning("[TurretLongRange] Can't shoot: missing target or bulletPrefab or firingPoint.");
        }

        isShooting = false;
    }

    // Optionally override Upgrade if you need subclass-specific behavior, but ALWAYS call base.Upgrade()
    public override void Upgrade()
    {
        // Check and log before calling base
        int cost = CalculateCost();
        Debug.Log($"[TurretLongRange] Attempting Upgrade: cost={cost}, levelBefore={GetLevel()}");
        base.Upgrade();
        Debug.Log($"[TurretLongRange] After base.Upgrade: level={GetLevel()}, bps={bps}, range={targetingRange}, damage={bulletDamage}");
    }
}

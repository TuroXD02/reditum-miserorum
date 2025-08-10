using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TurretPoison : Turret
{
    [Header("Poison - unique")]
    [SerializeField] private GameObject poisonBulletPrefab;
    [SerializeField] private Sprite[] upgradeSprites; // optional sprite per level
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float rotationOffset = -90f;

    [Header("Audio (optional override)")]
    [SerializeField] private AudioMixerGroup mixerGroup;
    [SerializeField] private AudioClip sellClip;           // <--- added so PlaySellSound compiles
    [SerializeField] private float volumeMin = 0.9f, volumeMax = 1.1f;

    // NOTE: do NOT re-declare bps/targetingRange/bulletDamage/etc. Use the protected fields from base Turret.
    // base.Start() will cache bpsBase/targetingRangeBase/bulletDamageBase.

    private void Start()
    {
        // Base initialization caches base stats, sets up the upgrade button and plays placeClip.
        base.Start();

        // Ensure audioSource exists and assign mixer if requested
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (mixerGroup != null)
            audioSource.outputAudioMixerGroup = mixerGroup;

        UpdateSprite();
    }

    private void Update()
    {
        // Use the same targeting / firing rhythm as base but with custom rotation.
        activeTime += Time.deltaTime;

        if (target == null)
        {
            FindTarget();
            return;
        }

        RotateTowardsTarget();

        if (!CheckTargetIsInRange())
        {
            target = null;
            return;
        }

        timeUntilFire += Time.deltaTime;
        if (timeUntilFire >= 1f / bps) // uses protected bps from base class
        {
            Shoot(); // overridden below
            timeUntilFire = 0f;
        }
    }

    // override Shoot to spawn poison bullet prefab instead of base bulletPrefab
    public override void Shoot()
    {
        if (poisonBulletPrefab == null || firingPoint == null) return;

        Debug.Log($"Shooting bullet with damage: {bulletDamage}, DPS before shot: {CalculateCurrentDPS()}");

        GameObject bulletObj = Instantiate(poisonBulletPrefab, firingPoint.position, Quaternion.identity);

        if (bulletObj.TryGetComponent(out PoisonBullet pb))
        {
            pb.SetTarget(target);
            pb.SetDamage(bulletDamage);
            pb.SetOwner(this);
        }

        RegisterShot();
        PlaySound(shootClip);

        Debug.Log($"DPS after shot: {CalculateCurrentDPS()}");
    }

    public override float CalculateCurrentDPS()
    {
        return bulletDamage * bps;
    }
    private void RotateTowardsTarget()
    {
        if (target == null || turretRotationPoint == null) return;

        Vector3 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        turretRotationPoint.rotation = Quaternion.Lerp(turretRotationPoint.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // Update sprite using the subclass upgradeSprites (if provided)
    protected void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && upgradeSprites.Length > 0)
        {
            int idx = Mathf.Clamp(level - 1, 0, upgradeSprites.Length - 1);
            turretSpriteRenderer.sprite = upgradeSprites[idx];
        }
        else
        {
            base.UpdateSprite();
        }
    }

    // Override Calculate* methods for Poison-specific scaling (base methods are virtual)
    public override float CalculateBPS(int lvl) => bpsBase * Mathf.Pow(lvl, 1f);
    public override float CalculateRange(int lvl) => targetingRangeBase * Mathf.Pow(lvl, 0.55f);
    public override int CalculateBulletDamage(int lvl) => Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(lvl, 1.5f));

    // Provide a poison-specific cost formula — base.CalculateCost() is non-virtual, so we 'new' it here.
    public new int CalculateCost() => Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));

    // Use base.Upgrade() so level++ and recalculation use your overridden Calculate* methods.
    public override void Upgrade()
    {
        base.Upgrade(); // this does currency check, level++ and sets bps/targetingRange/bulletDamage via Calculate* overrides
        UpdateSprite();
        PlaySound(upgradeClip); // upgradeClip is protected in base Turret
        Debug.Log($"Poison turret upgraded to level {level}: bps={bps}, range={targetingRange}, dmg={bulletDamage}");
    }

    public void PlaySellSound() => PlaySound(sellClip);
}

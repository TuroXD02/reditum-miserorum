using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class LongRangeBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject pivot;
    [SerializeField] private GameObject afterimagePrefab;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float damageMultiplier = 1f;

    [Header("Color Settings")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private float maxDistanceForWhiteness = 6f;

    [Header("Impact VFX")]
    [SerializeField] private GameObject normalImpactEffectPrefab;
    [SerializeField] private GameObject maxRangeImpactEffectPrefab;
    [SerializeField] private float impactEffectDuration = 2f;
    [SerializeField] private float impactRotationOffset = -90f;
    [SerializeField, Range(0.5f, 1f)] private float maxDistanceThreshold = 0.95f;

    [Header("Impact SFX")]
    [SerializeField] private AudioClip normalImpactSound;
    [SerializeField] private AudioClip maxRangeImpactSound;
    [SerializeField] private float impactVolume = 1f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Afterimage Settings")]
    [SerializeField] private float afterimageSpawnInterval = 0.05f;
    [SerializeField] private float afterimageFadeDuration = 0.3f;

    private int bulletDamage;
    private Vector2 startPosition;
    private SpriteRenderer spriteRenderer;
    public Transform target;

    private float afterimageTimer = 0f;

    // owner is the base Turret type (so SetOwner accepts any turret subtype)
    private Turret owner;

    private void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogError("LongRangeBullet: No SpriteRenderer found on bullet!");
        else
            spriteRenderer.color = startColor;

        Destroy(gameObject, 6f); // safety cleanup
    }

    // owner uses base Turret -> this keeps types simple and compatible
    public void SetOwner(Turret turretOwner) => owner = turretOwner;
    public void SetDamage(int damage) => bulletDamage = damage;
    public void SetTarget(Transform _target) => target = _target;

    private void Update()
    {
        // slight rotation for visual effect
        transform.Rotate(0f, 0f, -2f * Time.deltaTime);

        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        float factor = Mathf.Clamp01(distanceTraveled / maxDistanceForWhiteness);

        if (spriteRenderer != null)
            spriteRenderer.color = Color.Lerp(startColor, endColor, factor);

        afterimageTimer += Time.deltaTime;
        if (afterimageTimer >= afterimageSpawnInterval)
        {
            SpawnAfterimage(factor);
            afterimageTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (target == null || rb == null) return;
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (rb == null || spriteRenderer == null) return;

        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        int scaledDamage = Mathf.CeilToInt(bulletDamage + (distanceTraveled * damageMultiplier));

        bool enemyKilled = false;

        // Prefer EnemyHealth.TakeDamage(dmg, owner) - it already records damage/kill back to the turret
        if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
        {
            // This overload returns true if killed and also internally calls damageSource?.RecordDamage(...)
            bool wasKilled = enemyHealth.TakeDamage(scaledDamage, owner);
            enemyKilled = wasKilled;
            // No need to manually call owner.RecordDamage()/RegisterDamage() when using this overload
        }
        else if (other.gameObject.TryGetComponent(out LussuriaHealth lussuriaHealth))
        {
            // LussuriaHealth doesn't accept an owner parameter in your provided code,
            // so we manually notify the turret about damage and possible kill.
            lussuriaHealth.TakeDamage(scaledDamage);
            owner?.RecordDamage(scaledDamage);

            if (lussuriaHealth.IsDestroyed)
            {
                owner?.RecordKill();
                enemyKilled = true;
            }
        }

        // If you still needed any other enemy types, handle them here similarly.

        bool isMaxRange = (distanceTraveled / maxDistanceForWhiteness) >= maxDistanceThreshold;
        PlayImpactEffectAndSound(isMaxRange);

        DisableBulletVisualsAndPhysics();
        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    private void PlayImpactEffectAndSound(bool isMaxRange)
    {
        GameObject chosenVFX = isMaxRange ? maxRangeImpactEffectPrefab : normalImpactEffectPrefab;
        AudioClip chosenSFX = isMaxRange ? maxRangeImpactSound : normalImpactSound;

        if (chosenVFX != null && rb != null)
        {
            Vector2 bulletDir = rb.velocity.normalized;
            float angle = Mathf.Atan2(-bulletDir.y, -bulletDir.x) * Mathf.Rad2Deg + impactRotationOffset;
            GameObject impact = Instantiate(chosenVFX, transform.position, Quaternion.Euler(0f, 0f, angle));
            Destroy(impact, impactEffectDuration);
        }

        if (chosenSFX != null && sfxMixerGroup != null)
        {
            GameObject tempAudio = new GameObject("TempImpactSFX");
            tempAudio.transform.position = transform.position;
            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.volume = impactVolume;
            source.spatialBlend = 0f;
            source.PlayOneShot(chosenSFX);
            Destroy(tempAudio, chosenSFX.length + 0.2f);
        }
    }

    private void DisableBulletVisualsAndPhysics()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    private void SpawnAfterimage(float factor)
    {
        if (afterimagePrefab == null || spriteRenderer == null) return;

        GameObject afterimage = Instantiate(afterimagePrefab, transform.position, transform.rotation);
        afterimage.transform.parent = null;

        SpriteRenderer sr = afterimage.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = spriteRenderer.sprite;
            sr.sortingOrder = spriteRenderer.sortingOrder - 1;
            sr.color = Color.Lerp(startColor, endColor, factor);
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}

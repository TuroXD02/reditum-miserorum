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

    private void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = startColor;
        }
        else
        {
            Debug.LogError("LongRangeBullet: No SpriteRenderer found!");
        }

        Destroy(gameObject, 6f);
    }

    public void SetDamage(int damage) => bulletDamage = damage;
    public void SetTarget(Transform _target) => target = _target;

    private void Update()
    {
        transform.Rotate(0, 0, -2f * Time.deltaTime);

        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        float factor = Mathf.Clamp01(distanceTraveled / maxDistanceForWhiteness);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, factor);
        }

        afterimageTimer += Time.deltaTime;
        if (afterimageTimer >= afterimageSpawnInterval)
        {
            SpawnAfterimage(distanceTraveled, factor);
            afterimageTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (rb == null || spriteRenderer == null) return;

        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        int scaledDamage = Mathf.CeilToInt(bulletDamage + (distanceTraveled * damageMultiplier));

        if (other.gameObject.TryGetComponent(out EnemyHealth enemyHealth))
            enemyHealth.TakeDamage(scaledDamage);

        if (other.gameObject.TryGetComponent(out LussuriaHealth lussuriaHealth))
            lussuriaHealth.TakeDamage(scaledDamage);

        bool isMaxRange = (distanceTraveled / maxDistanceForWhiteness) >= maxDistanceThreshold;
        PlayImpactEffectAndSound(isMaxRange);

        DisableBulletVisualsAndPhysics();
        StartCoroutine(DestroyAfterDelay(0.5f));
    }

    private void PlayImpactEffectAndSound(bool isMaxRange)
    {
        GameObject chosenVFX = isMaxRange ? maxRangeImpactEffectPrefab : normalImpactEffectPrefab;
        AudioClip chosenSFX = isMaxRange ? maxRangeImpactSound : normalImpactSound;

        // 🎇 Spawn VFX
        if (chosenVFX != null)
        {
            Vector2 bulletDir = rb.velocity.normalized;
            float angle = Mathf.Atan2(-bulletDir.y, -bulletDir.x) * Mathf.Rad2Deg + impactRotationOffset;
            GameObject impact = Instantiate(chosenVFX, transform.position, Quaternion.Euler(0f, 0f, angle));
            Destroy(impact, impactEffectDuration);
        }

        // 🎵 Play SFX immediately with PlayOneShot
        if (chosenSFX != null && sfxMixerGroup != null)
        {
            GameObject tempAudio = new GameObject("TempImpactSFX");
            tempAudio.transform.position = transform.position;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.outputAudioMixerGroup = sfxMixerGroup;
            source.volume = impactVolume;
            source.spatialBlend = 0f; // 2D sound, set to 1f if you want 3D positioning

            // Play instantly and independently
            source.PlayOneShot(chosenSFX);

            Destroy(tempAudio, chosenSFX.length + 0.2f); // wait slightly longer
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

    private void SpawnAfterimage(float distanceTraveled, float factor)
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

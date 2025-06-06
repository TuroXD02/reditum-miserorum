using System.Collections;
using UnityEngine;

public class LongRangeBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject pivot;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float damageMultiplier = 1f;

    [Header("Color Settings")]
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private float maxDistanceForWhiteness = 6f;

    [Header("Impact Settings")]
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private GameObject maxRangeImpactEffectPrefab;
    [SerializeField] private float impactEffectDuration = 2f;
    [SerializeField] private float impactRotationOffset = -90f;
    [SerializeField, Range(0.5f, 1f)] private float maxDistanceThreshold = 0.95f;

    private int bulletDamage;
    private Vector2 startPosition;
    private SpriteRenderer spriteRenderer;
    public Transform target;

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
    }

    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private void Update()
    {
        transform.Rotate(0, 0, -2 * Time.deltaTime);

        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        float factor = Mathf.Clamp01(distanceTraveled / maxDistanceForWhiteness);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, factor);
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
        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        int scaledDamage = Mathf.CeilToInt(bulletDamage + (distanceTraveled * damageMultiplier));

        // ✅ Debug log always prints
        Debug.Log($"LongRangeBullet hit {other.gameObject.name} for {scaledDamage} damage (Distance: {distanceTraveled:F2})");

        // Apply damage
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
        LussuriaHealth lussuriaHealth = other.gameObject.GetComponent<LussuriaHealth>();

        if (enemyHealth != null) enemyHealth.TakeDamage(scaledDamage);
        if (lussuriaHealth != null) lussuriaHealth.TakeDamage(scaledDamage);

        // Play the correct animation
        GameObject chosenImpact = impactEffectPrefab;

        if ((distanceTraveled / maxDistanceForWhiteness) >= maxDistanceThreshold && maxRangeImpactEffectPrefab != null)
        {
            chosenImpact = maxRangeImpactEffectPrefab;
        }

        if (chosenImpact != null)
        {
            Vector2 bulletDir = rb.velocity.normalized;
            float angle = Mathf.Atan2(-bulletDir.y, -bulletDir.x) * Mathf.Rad2Deg + impactRotationOffset;

            GameObject impact = Instantiate(chosenImpact, transform.position, Quaternion.Euler(0f, 0f, angle));
            Destroy(impact, impactEffectDuration);
        }

        Destroy(gameObject);
    }
}

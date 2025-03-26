using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongRangeBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject pivot;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float damageMultiplier; // Multiplier for damage scaling with distance
    [SerializeField, Tooltip("Distance over which the bullet transitions from startColor to white.")]
    private float maxDistanceForWhiteness = 6f;

    [Header("Color Settings")]
    [SerializeField, Tooltip("The starting (darker) color for the bullet.")]
    private Color startColor = Color.white; // Set to a dark color in the Inspector.
    private Color endColor = Color.red;  // Final color is white.

    private int bulletDamage;
    public Transform target;
    private Vector2 startPosition; // Store the starting position

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Store the bullet's initial position.
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Initialize with the dark starting color.
            spriteRenderer.color = startColor;
        }
        else
        {
            Debug.LogError("LongRangeBullet: No SpriteRenderer found!");
        }
    }

    // Sets the damage value.
    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    // Sets the target for the bullet.
    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private void Update()
    {
        // Rotate the bullet for visual effect.
        transform.Rotate(0, 0, -2 * Time.deltaTime);

        // Calculate the distance traveled.
        float distanceTraveled = Vector2.Distance(startPosition, transform.position);
        // Calculate an interpolation factor from 0 to 1.
        float factor = Mathf.Clamp01(distanceTraveled / maxDistanceForWhiteness);

        // Lerp the color from the dark startColor to white based on the factor.
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(startColor, endColor, factor);
        }
    }

    private void FixedUpdate()
    {
        // If no target is set, do nothing.
        if (target == null) return;

        // Move the bullet toward the target.
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Calculate the distance traveled.
        float distanceTraveled = Vector2.Distance(startPosition, transform.position);

        // Calculate the final damage based on distance.
        int finalDamage = Mathf.CeilToInt(bulletDamage + (distanceTraveled * damageMultiplier));

        // Attempt to get the EnemyHealth component.
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();

        // Attempt to get the LussuriaHealth component.
        LussuriaHealth lussuriaHealth = other.gameObject.GetComponent<LussuriaHealth>();

        // Apply damage if EnemyHealth component exists.
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(finalDamage);
        }

        // Apply damage if LussuriaHealth component exists.
        if (lussuriaHealth != null)
        {
            lussuriaHealth.TakeDamage(finalDamage);
        }

        // Destroy the bullet after collision.
        Destroy(gameObject);
    }
}

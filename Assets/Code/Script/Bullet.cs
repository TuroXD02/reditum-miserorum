using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject pivot;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 10f;

    [Header("Impact Effects")]
    [SerializeField] private List<GameObject> impactEffects; // Visual FX prefabs
    [SerializeField] private float effectDuration = 1.5f;
    [SerializeField] private float rotationOffset = -90f; // Optional angle adjustment

    private static int lastEffectIndex = -1;

    private int bulletDamage = 1;
    public Transform target;

    // Setup methods
    public void SetDamage(int damage) => bulletDamage = damage;
    public void SetTarget(Transform _target) => target = _target;

    private void Start()
    {
        Destroy(gameObject, 12f); // Automatically destroy bullet after 12 seconds
    }
    private void Update()
    {
        transform.Rotate(0f, 0f, -720f * Time.deltaTime); // Fast spin
    }

    private void FixedUpdate()
    {
        if (!target)
        {
            return; // Don't destroy immediately — allow the bullet to expire after 12s
        }

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Damage handling
        if (collision.gameObject.TryGetComponent(out EnemyHealth enemy))
        {
            enemy.TakeDamage(bulletDamage);
        }

        if (collision.gameObject.TryGetComponent(out LussuriaHealth boss))
        {
            boss.TakeDamage(bulletDamage);
        }

        PlayImpactEffect();
        Destroy(gameObject);
    }

    private void PlayImpactEffect()
    {
        if (impactEffects == null || impactEffects.Count == 0) return;

        // Avoid using the same effect twice in a row
        int index;
        do
        {
            index = Random.Range(0, impactEffects.Count);
        }
        while (impactEffects.Count > 1 && index == lastEffectIndex);

        lastEffectIndex = index;

        // Determine bullet's opposite direction
        Vector2 bulletDir = rb.velocity.normalized;
        Vector2 oppositeDir = -bulletDir;

        float angle = Mathf.Atan2(oppositeDir.y, oppositeDir.x) * Mathf.Rad2Deg + rotationOffset;
        Quaternion rot = Quaternion.Euler(0f, 0f, angle);

        GameObject impact = Instantiate(impactEffects[index], transform.position, rot);
        Destroy(impact, effectDuration);
    }
}

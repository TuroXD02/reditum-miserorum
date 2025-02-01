using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDamageBullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed;  // Speed at which the bullet travels
    private int damage;                          // Damage the bullet deals
    private float aoeRadius;                     // Radius for area damage
    private Transform target;                    // Target the bullet is homing toward

    // Called by the turret to set the damage value
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    // Called by the turret to set the explosion (AOE) radius
    public void SetAOERadius(float radius)
    {
        aoeRadius = radius;
    }

    // Called by the turret to assign a target for the bullet
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // When the bullet hits, perform an overlap check to damage all enemies within aoeRadius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (Collider2D collider in hitColliders)
        {
            // If the collider has an EnemyHealth component, deal damage to it
            EnemyHealth enemyHealth = collider.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // Optionally, play explosion effects or sounds here

        Destroy(gameObject);
    }

    // For debugging: visualize the explosion radius when selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}

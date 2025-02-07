using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourBreakerBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float bulletSpeed = 10f; // Speed at which the bullet travels.
    
    private int damage;              // Damage to apply when hitting the enemy.
    private int armourReduction;     // Amount by which to reduce the enemy's armour.
    private Transform target;        // The enemy target.
    private float aoeRadius;         // Area-of-effect radius provided by the turret.

    [Header("Swiping Animation Settings")]
    // Animator Controller for the swiping animation (assign this via the Inspector).
    [SerializeField] private RuntimeAnimatorController swipeAnimatorController;
    // Base effect radius used for scaling; when the turret's aoeRadius exceeds this, the bullet scales up.
    [SerializeField] private float baseEffectRadius = 5f;

    private Animator anim; // Reference to the bullet's Animator.

    private void Awake()
    {
        // Get the Animator component from this bullet (it should be attached on the prefab).
        anim = GetComponent<Animator>();
        // If an Animator and a swiping animation controller have been assigned, set the controller.
        if (anim != null && swipeAnimatorController != null)
        {
            anim.runtimeAnimatorController = swipeAnimatorController;
        }
    }

    // ---------------------------
    // Public Methods to Set Parameters
    // ---------------------------

    // Set the damage value.
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    // Set the armour reduction value.
    public void SetArmourReduction(int reduction)
    {
        armourReduction = reduction;
    }

    // Set the target for the bullet.
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    // Set the area-of-effect radius and adjust the bullet's scale accordingly.
    public void SetAOERadius(float radius)
    {
        aoeRadius = radius;
        // Calculate a scaling factor based on the ratio of the turret's aoeRadius to a base value.
        float scaleFactor = aoeRadius / baseEffectRadius;
        // Scale the bullet (and its animation) so that larger aoeRadius values result in a larger slash.
        transform.localScale = Vector3.one * scaleFactor;
    }

    // ---------------------------
    // Update: Moves the bullet toward its target.
    // ---------------------------
    private void Update()
    {
        if (target != null)
        {
            // Move toward the target using MoveTowards.
            transform.position = Vector2.MoveTowards(transform.position, target.position, bulletSpeed * Time.deltaTime);
            
            // When the bullet is close enough, trigger the hit.
            if (Vector2.Distance(transform.position, target.position) < 0.2f)
            {
                HitTarget();
            }
        }
        else
        {
            // If the target is missing, destroy the bullet.
            Destroy(gameObject);
        }
    }

    // ---------------------------
    // HitTarget: Called when the bullet reaches the enemy.
    // ---------------------------
    private void HitTarget()
    {
        // Try to get the EnemyHealth component.
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            enemyHealth.ReduceArmour(armourReduction);
        }
        
        // Also check for a LussuriaHealth component.
        LussuriaHealth lussuriaHealth = target.GetComponent<LussuriaHealth>();
        if (lussuriaHealth != null)
        {
            lussuriaHealth.TakeDamage(damage);
            lussuriaHealth.ReduceArmour(armourReduction);
        }
        
        // Optionally, you can trigger additional visual effects here if needed.

        // Destroy the bullet after impact.
        Destroy(gameObject);
    }
}

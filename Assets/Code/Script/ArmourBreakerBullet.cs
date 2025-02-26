using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmourBreakerBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float bulletDuration = 0.5f; // How long the bullet remains before applying damage.

    private int damage;              // Damage applied on hit.
    private int armourReduction;     // Armour reduction value.
    private Transform target;        // The enemy target.
    private Vector3 startPosition;   // Turret firing point (bullet's spawn position).

    [Header("Animation & Scaling Settings")]
    [SerializeField] private RuntimeAnimatorController swipeAnimatorController; // Controller for the swipe animation.
    [SerializeField] private float baseBulletSize = 1f;  // Scale multiplier for overall bullet size.

    private Animator anim;           // Reference to the bullet's Animator.
    private SpriteRenderer sr;       // Reference to the bullet's SpriteRenderer.
    private float spriteWidth;       // The natural width of the sprite in world units.

    private float timer = 0f;
    private bool hasHit = false;

    private void Awake()
    {
        // Get components from this bullet (or its children).
        anim = GetComponent<Animator>();
        if (anim != null && swipeAnimatorController != null)
        {
            anim.runtimeAnimatorController = swipeAnimatorController;
        }

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            spriteWidth = sr.sprite.bounds.size.x;
            if (spriteWidth <= 0)
            {
                Debug.LogError("ArmourBreakerBullet: Sprite width is zero or negative!");
            }
        }
        else
        {
            Debug.LogError("ArmourBreakerBullet: No SpriteRenderer found!");
        }
    }

    private void Start()
    {
        // If target hasn't been set before Start(), destroy the bullet.
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        // Record the turret firing point.
        startPosition = transform.position;
        timer = 0f;
        AdjustBullet();
    }

    private void Update()
    {
        // If the target gets destroyed mid-animation, destroy the bullet.
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Optionally update if target moves. (If you want it to follow a moving target, uncomment the next line.)
        // UpdateBulletTransform();

        timer += Time.deltaTime;
        if (timer >= bulletDuration)
        {
            HitTarget();
        }
    }

    /// <summary>
    /// Computes the transformation so that the bullet stretches from the turret firing point (startPosition) to the enemy.
    /// </summary>
    private void AdjustBullet()
    {
        // Calculate direction and distance from turret (point A) to enemy (point B).
        Vector3 direction = (target.position - startPosition).normalized;
        float distance = Vector2.Distance(startPosition, target.position);

        // Rotate the bullet so that it points from turret to enemy.
        // (If your animation is reversed, adding 180° flips it.)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180);

        // Scale the bullet's width so that its stretched width equals the distance.
        // (spriteWidth is the natural width of the sprite when localScale.x is 1.)
        if (spriteWidth > 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = (distance / spriteWidth) * baseBulletSize;
            newScale.y = baseBulletSize;
            transform.localScale = newScale;
        }

        // Position the bullet so its center is exactly halfway between turret and enemy.
        // With a centered pivot, this makes the left edge align with the turret firing point.
        transform.position = startPosition + direction * (distance / 2);
    }

    /// <summary>
    /// Optionally, if you want the bullet to update its transform continuously if the enemy moves.
    /// Uncomment this function and call it in Update().
    /// </summary>
    private void UpdateBulletTransform()
    {
        Vector3 direction = (target.position - startPosition).normalized;
        float distance = Vector2.Distance(startPosition, target.position);
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180);
        if (spriteWidth > 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = (distance / spriteWidth) * baseBulletSize;
            newScale.y = baseBulletSize;
            transform.localScale = newScale;
        }
        transform.position = startPosition + direction * (distance / 2);
    }

    /// <summary>
    /// Sets the damage value.
    /// </summary>
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    /// <summary>
    /// Sets the armour reduction value.
    /// </summary>
    public void SetArmourReduction(int reduction)
    {
        armourReduction = reduction;
    }

    /// <summary>
    /// Sets the enemy target for this bullet.
    /// </summary>
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    /// <summary>
    /// Applies damage and armour reduction to the target, then destroys the bullet.
    /// </summary>
    private void HitTarget()
    {
        if (hasHit) return;
        hasHit = true;

        if (target != null)
        {
            EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                enemyHealth.ReduceArmour(armourReduction);
            }
            LussuriaHealth lussuriaHealth = target.GetComponent<LussuriaHealth>();
            if (lussuriaHealth != null)
            {
                lussuriaHealth.TakeDamage(damage);
                lussuriaHealth.ReduceArmour(armourReduction);
            }
        }
        Destroy(gameObject);
    }
}

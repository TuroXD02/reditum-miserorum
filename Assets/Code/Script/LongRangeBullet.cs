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

    private int bulletDamage;
    public Transform target;
    private Vector2 startPosition; // Store the starting position

    private void Start()
    {
        // Store the bullet's initial position when it is fired
        startPosition = transform.position;
    }

    // Metodo per settare il danno del proiettile
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
        // Rotates the bullet for visual effect
        transform.Rotate(0, 0, -2 * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // If no target is set, do nothing
        if (!target) return;

        // If the target is destroyed, destroy the bullet
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        // Move the bullet towards the target
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Calculate the distance the bullet has traveled
        float distanceTraveled = Vector2.Distance(startPosition, transform.position);

        // Calculate the final damage based on distance
        int finalDamage = Mathf.CeilToInt(bulletDamage + (distanceTraveled * damageMultiplier));

        // Attempt to get the EnemyHealth component from the collided object
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();

        // Attempt to get the LussuriaHealth component from the collided object
        LussuriaHealth lussuriaHealth = other.gameObject.GetComponent<LussuriaHealth>();

        // Check if the collided object has an EnemyHealth component
        if (enemyHealth != null)
        {
            // Apply the calculated damage to the enemy
            enemyHealth.TakeDamage(finalDamage);
            
            //Debug.Log($"Damage dealt to {enemyHealth.name}: {finalDamage}");
        }

        // Check if the collided object has a LussuriaHealth component
        if (lussuriaHealth != null)
        {
            // Apply the calculated damage to the Lussuria
            lussuriaHealth.TakeDamage(finalDamage);
        }

        // Destroy the bullet after collision
        Destroy(gameObject);
    }
}
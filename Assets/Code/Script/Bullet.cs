using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject pivot;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 10f;

    private int bulletDamage = 1;
    public Transform target;

    [Header("Impact Effects")]
    [SerializeField] private List<GameObject> impactEffects; // List of impact animation prefabs
    [SerializeField] private float effectDuration = 1.5f;

    private static int lastEffectIndex = -1; // To prevent repetition

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
        transform.Rotate(0, 0, -720 * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();
        LussuriaHealth lussuriaHealth = other.gameObject.GetComponent<LussuriaHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(bulletDamage);
        }

        if (lussuriaHealth != null)
        {
            lussuriaHealth.TakeDamage(bulletDamage);
        }

        PlayImpactEffect();

        Destroy(gameObject);
    }

    private void PlayImpactEffect()
    {
        if (impactEffects == null || impactEffects.Count == 0) return;

        int index;
        do
        {
            index = Random.Range(0, impactEffects.Count);
        }
        while (impactEffects.Count > 1 && index == lastEffectIndex); // Avoid repeat if multiple exist

        lastEffectIndex = index;

        GameObject impact = Instantiate(impactEffects[index], transform.position, Quaternion.identity);
        Destroy(impact, effectDuration);
    }
}

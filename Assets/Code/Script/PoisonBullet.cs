using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;  
    [SerializeField] private SpriteRenderer spriteRenderer; 
    [SerializeField] private Collider2D bulletCollider;    

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed;  

    [Header("Poison Damage Settings")]
    [Tooltip("Duration of the poison effect on the target.")]
    [SerializeField] private float poisonDuration;   
    [Tooltip("Interval between poison damage ticks.")]
    [SerializeField] private float poisonTickInterval;  

    private Transform target;  
    private bool hasHitTarget = false;  
    private int poisonDamagePerTick;  
    
    [Header("Poison Visual Effect Settings")]
    [SerializeField] private GameObject poisonVfxPrefab;  // Prefab for the poison visual effect.
    [SerializeField] private Color poisonOverlayColor = new Color(0.7f, 1f, 0.7f, 1f); // Slight green tint.
    [SerializeField] private float poisonTintFadeDuration = 0.5f; // Fade in/out duration for tint.

    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private void Start()
    {
        int numberOfTicks = Mathf.CeilToInt(poisonDuration / poisonTickInterval);
        if (numberOfTicks == 0)
        {
            numberOfTicks = 1;
        }
        poisonDamagePerTick = 50;
    }

    private void Update()
    {
        if (!hasHitTarget)
        {
            transform.Rotate(0, 0, -720 * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (!target) return;
        if (!hasHitTarget)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            rb.velocity = direction * bulletSpeed;
            if (Vector2.Distance(transform.position, target.position) < 0.1f)
            {
                OnHitTarget();
            }
        }
    }

    private void OnHitTarget()
    {
        hasHitTarget = true;
        rb.velocity = Vector2.zero;

        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        LussuriaHealth lussuriaHealth = target.GetComponent<LussuriaHealth>();

        // Apply poison damage over time and visual effects for EnemyHealth.
        if (enemyHealth != null)
        {
            StartCoroutine(ApplyPoisonDamageOverTime(enemyHealth));
            ApplyPoisonVisualEffects(target);
        }

        // Apply poison damage over time and visual effects for LussuriaHealth.
        if (lussuriaHealth != null)
        {
            StartCoroutine(ApplyPoisonDamageOverTime(lussuriaHealth));
            ApplyPoisonVisualEffects(target);
        }

        HideBullet();
        Destroy(gameObject, poisonDuration);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform == target)
        {
            OnHitTarget();
        }
    }

    private IEnumerator ApplyPoisonDamageOverTime(EnemyHealth enemy)
    {
        float elapsedTime = 0f;
        while (elapsedTime < poisonDuration)
        {
            enemy.TakeDamageDOT(poisonDamagePerTick);
            yield return new WaitForSeconds(poisonTickInterval);
            elapsedTime += poisonTickInterval;
        }
    }

    private IEnumerator ApplyPoisonDamageOverTime(LussuriaHealth lussuria)
    {
        float elapsedTime = 0f;
        while (elapsedTime < poisonDuration)
        {
            lussuria.TakeDamageDOTLU(poisonDamagePerTick);
            yield return new WaitForSeconds(poisonTickInterval);
            elapsedTime += poisonTickInterval;
        }
    }

    private void HideBullet()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
        }
    }

    public void SetDamage(int poisonDamage)
    {
        poisonDamagePerTick = poisonDamage;
    }

    /// <summary>
    /// Applies visual poison effects to the enemy:
    /// - Instantiates a poison VFX prefab (if assigned).
    /// - Applies a green overlay using the EnemyPoisonEffect component.
    /// </summary>
    private void ApplyPoisonVisualEffects(Transform enemyTransform)
    {
        // Instantiate poison VFX prefab on the enemy.
        if (poisonVfxPrefab != null)
        {
            GameObject vfx = Instantiate(poisonVfxPrefab, enemyTransform.position, Quaternion.identity, enemyTransform);
            Destroy(vfx, poisonDuration);
        }
        
        // Get or add the EnemyPoisonEffect component and apply the poison effect.
        SpriteRenderer enemyRenderer = enemyTransform.GetComponent<SpriteRenderer>();
        if (enemyRenderer != null)
        {
            EnemyPoisonEffect poisonEffect = enemyTransform.GetComponent<EnemyPoisonEffect>();
            if (poisonEffect == null)
            {
                poisonEffect = enemyTransform.gameObject.AddComponent<EnemyPoisonEffect>();
            }
            poisonEffect.ApplyPoisonEffect(poisonOverlayColor, poisonTintFadeDuration, poisonDuration);
        }
    }
}

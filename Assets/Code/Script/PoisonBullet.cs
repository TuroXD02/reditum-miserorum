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
    [SerializeField] private float poisonDuration;
    [SerializeField] private float poisonTickInterval;

    private Transform target;
    private bool hasHitTarget = false;
    private int poisonDamagePerTick;

    [Header("Poison Visual Effect Settings")]
    [SerializeField] private GameObject poisonVfxPrefab;
    [SerializeField] private Color poisonOverlayColor = new Color(0.7f, 1f, 0.7f, 1f);
    [SerializeField] private float poisonTintFadeDuration = 0.5f;

    public void SetTarget(Transform _target) => target = _target;

    private void Start() => poisonDamagePerTick = 50;

    private void Update()
    {
        if (!hasHitTarget) transform.Rotate(0, 0, -720 * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (target == null || hasHitTarget) return;

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            OnHitTarget();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform == target) OnHitTarget();
    }

    private void OnHitTarget()
    {
        hasHitTarget = true;
        rb.velocity = Vector2.zero;

        if (target.TryGetComponent(out EnemyHealth enemy))
        {
            StartCoroutine(ApplyPoisonDamageOverTime(enemy));
            ApplyPoisonVisualEffects(target);
        }
        else if (target.TryGetComponent(out LussuriaHealth lussuria))
        {
            StartCoroutine(ApplyPoisonDamageOverTimeLussuria(lussuria));
            ApplyPoisonVisualEffects(target);
        }

        HideBullet();
        Destroy(gameObject, poisonDuration);
    }

    private IEnumerator ApplyPoisonDamageOverTime(EnemyHealth enemy)
    {
        float elapsed = 0f;
        while (elapsed < poisonDuration && enemy != null && !enemy.IsDestroyed)
        {
            enemy.TakeDamageDOT(poisonDamagePerTick);
            yield return new WaitForSeconds(poisonTickInterval);
            elapsed += poisonTickInterval;
        }
    }

    private IEnumerator ApplyPoisonDamageOverTimeLussuria(LussuriaHealth target)
    {
        float elapsed = 0f;
        while (elapsed < poisonDuration && target != null && !target.IsDestroyed)
        {
            target.TakeDamageDOTLU(poisonDamagePerTick);
            yield return new WaitForSeconds(poisonTickInterval);
            elapsed += poisonTickInterval;
        }
    }

    private void HideBullet()
    {
        if (spriteRenderer) spriteRenderer.enabled = false;
        if (bulletCollider) bulletCollider.enabled = false;
    }

    public void SetDamage(int damage) => poisonDamagePerTick = damage;

    private void ApplyPoisonVisualEffects(Transform enemyTransform)
    {
        if (poisonVfxPrefab)
        {
            GameObject vfx = Instantiate(poisonVfxPrefab, enemyTransform.position, Quaternion.identity, enemyTransform);
            Destroy(vfx, poisonDuration);
        }

        if (enemyTransform.TryGetComponent(out SpriteRenderer renderer))
        {
            var effect = enemyTransform.GetComponent<EnemyPoisonEffect>() ?? enemyTransform.gameObject.AddComponent<EnemyPoisonEffect>();
            effect.ApplyPoisonEffect(poisonOverlayColor, poisonTintFadeDuration, poisonDuration);
        }
    }
}

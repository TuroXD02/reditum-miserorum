using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AreaDamageBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float bulletSpeed = 5f;
    private int damage;
    private float aoeRadius;
    private Transform target;

    [Header("Explosion Animation")]
    [SerializeField] private RuntimeAnimatorController explosionAnimatorController;
    [SerializeField] private float explosionDuration = 1f;

    [Header("Explosion Visuals")]
    [SerializeField] private Color explosionOutlineColor = Color.white;
    [SerializeField] private float explosionOutlineWidth = 0.03f;
    private const int circleSegments = 50;
    private LineRenderer explosionLR;
    private bool hasExploded = false;

    [Header("Explosion Sound")]
    [SerializeField] private AudioClip explosionSound;

    // Setters
    public void SetDamage(int dmg) => damage = dmg;
    public void SetAOERadius(float radius) => aoeRadius = radius;
    public void SetTarget(Transform targetTransform) => target = targetTransform;

    private void FixedUpdate()
    {
        if (hasExploded) return;

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
        if (hasExploded) return;
        hasExploded = true;

        // Play explosion sound
        AudioSource audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
        if (explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Apply AoE Damage
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (Collider2D col in hitColliders)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // Disable bullet visuals and physics
        if (TryGetComponent(out SpriteRenderer sr)) sr.enabled = false;
        if (TryGetComponent(out Collider2D col2D)) col2D.enabled = false;
        if (TryGetComponent(out Rigidbody2D rb)) rb.velocity = Vector2.zero;

        // Create explosion outline
        CreateExplosionOutline();

        // Create explosion animation
        CreateExplosionAnimation(sr);

        // Cleanup
        StartCoroutine(EndExplosionEffect());
    }

    private void CreateExplosionOutline()
    {
        explosionLR = gameObject.AddComponent<LineRenderer>();
        explosionLR.positionCount = circleSegments + 1;
        explosionLR.loop = true;
        explosionLR.useWorldSpace = true;
        explosionLR.widthMultiplier = explosionOutlineWidth;
        explosionLR.material = new Material(Shader.Find("Sprites/Default"));
        explosionLR.startColor = explosionOutlineColor;
        explosionLR.endColor = explosionOutlineColor;

        float angleStep = 360f / circleSegments;
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * aoeRadius;
            float y = Mathf.Sin(angle) * aoeRadius;
            explosionLR.SetPosition(i, transform.position + new Vector3(x, y, 0f));
        }
    }

    private void CreateExplosionAnimation(SpriteRenderer originalSR)
    {
        GameObject explosionObj = new GameObject("ExplosionAnimation");
        explosionObj.transform.parent = transform;
        explosionObj.transform.localPosition = Vector3.zero;

        var explosionSR = explosionObj.AddComponent<SpriteRenderer>();
        if (originalSR != null)
        {
            explosionSR.sortingLayerName = originalSR.sortingLayerName;
            explosionSR.sortingOrder = originalSR.sortingOrder + 1;
        }

        var animator = explosionObj.AddComponent<Animator>();
        animator.runtimeAnimatorController = explosionAnimatorController;
    }

    private IEnumerator EndExplosionEffect()
    {
        float waitTime = explosionDuration * 0.05f;
        yield return new WaitForSeconds(waitTime);

        float fadeDuration = explosionDuration - waitTime;
        float elapsed = 0f;
        Color initialColor = explosionLR.startColor;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            Color newColor = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(initialColor.a, 0f, t));
            explosionLR.startColor = newColor;
            explosionLR.endColor = newColor;
            elapsed += Time.deltaTime;
            yield return null;
        }

        explosionLR.startColor = explosionLR.endColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        yield return new WaitForSeconds(0.05f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}

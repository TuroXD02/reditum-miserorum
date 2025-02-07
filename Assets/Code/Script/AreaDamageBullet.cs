using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaDamageBullet : MonoBehaviour
{
    // ---------------------------
    // Bullet Settings
    // ---------------------------
    [SerializeField] private float bulletSpeed;  // Speed at which the bullet travels.
    private int damage;                          // Damage dealt by the bullet.
    private float aoeRadius;                     // Radius within which the bullet deals area damage.
    private Transform target;                    // Target that the bullet is homing toward.

    // ---------------------------
    // Explosion Outline (Optional)
    // ---------------------------
    // Number of segments to form the explosion circle.
    private const int circleSegments = 50;
    // LineRenderer used to display an outline around the explosion.
    private LineRenderer explosionLR;
    
    // Flag to ensure the explosion effect is triggered only once.
    private bool hasExploded = false;

    // ---------------------------
    // Explosion Animation Settings
    // ---------------------------
    [Header("Explosion Animation Settings")]
    // RuntimeAnimatorController for your explosion animation (set this via the Inspector).
    [SerializeField] private RuntimeAnimatorController explosionAnimatorController;
    // Duration of the explosion animation (in seconds). Adjust this to match your animation clip.
    [SerializeField] private float explosionDuration = 1f;

    // ---------------------------
    // Public Methods to Set Parameters
    // ---------------------------
    
    // Set the damage for this bullet.
    public void SetDamage(int dmg)
    {
        damage = dmg;
    }

    // Set the area-of-effect radius.
    public void SetAOERadius(float radius)
    {
        aoeRadius = radius;
    }

    // Set the homing target for the bullet.
    public void SetTarget(Transform targetTransform)
    {
        target = targetTransform;
    }

    // ---------------------------
    // FixedUpdate: Bullet Movement
    // ---------------------------
    private void FixedUpdate()
    {
        // If the explosion effect has started, stop moving.
        if (hasExploded)
            return;

        // If a target is set, calculate direction and move toward it.
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
        }
        else
        {
            // No target available: destroy the bullet.
            Destroy(gameObject);
        }
    }

    // ---------------------------
    // OnCollisionEnter2D: Trigger Explosion
    // ---------------------------
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Prevent the explosion from triggering multiple times.
        if (hasExploded)
            return;
        hasExploded = true;

        // 1. Apply Area Damage:
        // Find all colliders within the explosion radius and apply damage to any enemies.
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (Collider2D col in hitColliders)
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
        }

        // 2. Disable Bullet Visuals & Physics:
        // Hide the bullet's sprite.
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = false;
        // Disable the collider to avoid further interactions.
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null)
            coll.enabled = false;
        // Stop bullet movement.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.velocity = Vector2.zero;

        // 3. (Optional) Create an Outline for the Explosion:
        // This creates a simple circle outline using a LineRenderer.
        explosionLR = gameObject.AddComponent<LineRenderer>();
        explosionLR.positionCount = circleSegments + 1;
        explosionLR.loop = true;
        explosionLR.useWorldSpace = true;
        explosionLR.widthMultiplier = 0.03f;
        explosionLR.material = new Material(Shader.Find("Sprites/Default"));
        explosionLR.startColor = Color.white;
        explosionLR.endColor = Color.white;
        // Set positions for the circle outline.
        float angleStep = 360f / circleSegments;
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * aoeRadius;
            float y = Mathf.Sin(angle) * aoeRadius;
            Vector3 pos = transform.position + new Vector3(x, y, 0f);
            explosionLR.SetPosition(i, pos);
        }

        // 4. Create a Child GameObject for the Explosion Animation:
        // This child will display your animated explosion effect.
        GameObject explosionObj = new GameObject("ExplosionAnimation");
        explosionObj.transform.parent = transform;     // Make it a child of the bullet.
        explosionObj.transform.localPosition = Vector3.zero; // Center it on the bullet.

        // Add a SpriteRenderer to display the animation frames.
        SpriteRenderer explosionSR = explosionObj.AddComponent<SpriteRenderer>();
        // Set the sorting layer and order so the explosion appears above the bullet.
        if (sr != null)
        {
            explosionSR.sortingLayerName = sr.sortingLayerName;
            explosionSR.sortingOrder = sr.sortingOrder + 1;
        }

        // Add an Animator to control the explosion animation.
        Animator explosionAnimator = explosionObj.AddComponent<Animator>();
        // Assign the Animator Controller that you created for the explosion animation.
        explosionAnimator.runtimeAnimatorController = explosionAnimatorController;

        // 5. Start a Coroutine to End the Explosion Effect with a Fade Out:
        StartCoroutine(EndExplosionEffect());
    }

    // ---------------------------
    // EndExplosionEffect Coroutine:
    // Waits for the explosion animation to complete and gradually fades out the outline, then destroys the bullet.
    // ---------------------------
    private IEnumerator EndExplosionEffect()
    {
        // Wait for half of the explosion duration before starting to fade the outline.
        float waitTime = explosionDuration * 0.05f;
        yield return new WaitForSeconds(waitTime);

        // Fade out the outline over the remaining duration.
        float fadeDuration = explosionDuration - waitTime;
        float elapsed = 0f;
        Color initialColor = explosionLR.startColor;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            // Lerp the alpha from the initial value to 0.
            Color newColor = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(initialColor.a, 0f, t));
            explosionLR.startColor = newColor;
            explosionLR.endColor = newColor;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the outline is fully transparent.
        Color transparent = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        explosionLR.startColor = transparent;
        explosionLR.endColor = transparent;

        // Wait a brief moment (optional) before destroying the bullet.
        yield return new WaitForSeconds(0.005f);
        Destroy(gameObject);
    }

    // ---------------------------
    // OnDrawGizmosSelected (Optional):
    // Visualizes the explosion radius in the Unity Editor.
    // ---------------------------
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}

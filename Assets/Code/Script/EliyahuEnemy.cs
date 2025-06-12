using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Rigidbody2D))]
public class EliyahuEnemy : EnemyHealth
{
    [Header("Respawn Settings")]
    [SerializeField] private int maxRespawns = 7;
    [SerializeField] private float respawnDelay = 1f;

    [Header("Health & Speed")]
    [SerializeField] private int fullHealth;
    [SerializeField] private float speedMultiplier = 1.2f;

    [Header("Appearance")]
    [SerializeField] private Sprite[] respawnSprites;

    [Header("Revive Effect")]
    [SerializeField] private GameObject reviveEffectPrefab;
    [SerializeField] private float reviveEffectDuration = 2f;

    [Header("Layer Control")]
    [SerializeField] private string aliveLayer = "Enemy";
    [SerializeField] private string deadLayer = "DeadEnemy";
    [SerializeField] private string respawnLayer = "Respawn";

    [Header("Projectile Cleanup")]
    [SerializeField] private float projectileClearRadius = 0.5f;
    [SerializeField] private LayerMask projectileLayer;

    private int currentRespawnCount = 0;
    private SpriteRenderer spriteRenderer;
    private EnemyMovement enemyMovement;
    private Collider2D col;
    private Rigidbody2D rb;

    private void Start()
    {
        hitPoints = fullHealth;

        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyMovement = GetComponent<EnemyMovement>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (!spriteRenderer || !enemyMovement || !col || !rb)
            Debug.LogError("Missing required components on EliyahuEnemy.");
    }

    protected override void EnemyDestroyed()
    {
        if (currentRespawnCount < maxRespawns)
        {
            currentRespawnCount++;
            Debug.Log($"{name} preparing to respawn ({currentRespawnCount}/{maxRespawns})");
            StartCoroutine(RespawnSequence());
        }
        else
        {
            base.EnemyDestroyed(); // permanently destroyed
        }
    }

    private IEnumerator RespawnSequence()
    {
        // === DEATH STATE ===

        // Stop movement
        if (enemyMovement != null)
            enemyMovement.enabled = false;

        // Remove velocity and freeze physics
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
        }

        // Disable collider so turrets/projectiles ignore it
        if (col != null)
            col.enabled = false;

        // Hide sprite
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        // Change to dead layer
        gameObject.layer = LayerMask.NameToLayer(deadLayer);
        gameObject.tag = respawnLayer;

        // Play revive effect
        if (reviveEffectPrefab != null)
        {
            GameObject fx = Instantiate(reviveEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, reviveEffectDuration);
        }

        // Start projectile cleanup coroutine
        StartCoroutine(CleanupProjectiles(reviveEffectDuration));

        // Wait before respawn
        yield return new WaitForSeconds(respawnDelay);

        // === RESPAWN STATE ===

        hitPoints = fullHealth;

        // Speed up movement
        if (enemyMovement != null)
        {
            enemyMovement.UpdateSpeed(enemyMovement.moveSpeed * speedMultiplier);
            enemyMovement.enabled = true;
        }

        // Restore collider and physics
        if (col != null)
            col.enabled = true;

        if (rb != null)
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Kinematic; // Still no physics reactions
        }

        // Restore visuals
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;

            if (respawnSprites != null && respawnSprites.Length > 0)
            {
                int index = (currentRespawnCount - 1) % respawnSprites.Length;
                spriteRenderer.sprite = respawnSprites[index];
            }
        }

        // Back to enemy layer
        gameObject.layer = LayerMask.NameToLayer(aliveLayer);
        gameObject.tag = "Enemy";

        Debug.Log($"{name} has respawned.");
    }

    private IEnumerator CleanupProjectiles(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, projectileClearRadius, projectileLayer);
            foreach (var hit in hits)
            {
                Destroy(hit.gameObject);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}

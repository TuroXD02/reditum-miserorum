using System.Collections;
using UnityEngine;

public class EliyahuEnemy : EnemyHealth
{
    [Header("Respawn Settings")]
    [SerializeField, Tooltip("Number of times Eliyahu can respawn before permanent death.")]
    private int maxRespawns = 7;
    private int currentRespawnCount = 0;

    [Header("Health & Speed Settings")]
    [SerializeField, Tooltip("Health restored upon each respawn.")]
    private int fullHealth = 500;
    [SerializeField, Tooltip("Multiplier applied to the current speed upon each respawn (e.g., 1.2 means 20% faster).")]
    private float speedMultiplier = 1.2f;

    [Header("Sprite Settings")]
    [SerializeField, Tooltip("Array of sprites to cycle through upon each respawn.")]
    private Sprite[] respawnSprites;

    private SpriteRenderer spriteRenderer;
    private EnemyMovement enemyMovement;

    private void Start()
    {
        // Initialize health and get required components.
        hitPoints = fullHealth;
        enemyMovement = GetComponent<EnemyMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (enemyMovement == null)
        {
            Debug.LogError($"{gameObject.name}: Missing EnemyMovement component!");
        }
        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: Missing SpriteRenderer component!");
        }
    }

    /// <summary>
    /// Overrides the base EnemyDestroyed method.
    /// Instead of permanently dying, Eliyahu respawns (with increased speed and a new sprite)
    /// up to maxRespawns times. After that, it dies permanently.
    /// </summary>
    protected override void EnemyDestroyed()
    {
        if (currentRespawnCount < maxRespawns)
        {
            currentRespawnCount++;
            Debug.Log($"{gameObject.name} respawning ({currentRespawnCount}/{maxRespawns})");

            // Increase movement speed.
            if (enemyMovement != null)
            {
                float oldSpeed = enemyMovement.moveSpeed;
                float newSpeed = oldSpeed * speedMultiplier;
                enemyMovement.UpdateSpeed(newSpeed);
                Debug.Log($"{gameObject.name} speed increased from {oldSpeed} to {newSpeed}");
            }

            // Cycle sprite based on respawn count.
            if (spriteRenderer != null && respawnSprites != null && respawnSprites.Length > 0)
            {
                int index = (currentRespawnCount - 1) % respawnSprites.Length;
                spriteRenderer.sprite = respawnSprites[index];
            }

            // Reset health for the respawn.
            hitPoints = fullHealth;
        }
        else
        {
            // No more respawns: proceed with permanent death.
            base.EnemyDestroyed();
        }
    }
}

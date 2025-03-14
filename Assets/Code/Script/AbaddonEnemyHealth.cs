using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbaddonEnemyHealth : EnemyHealth
{
    [Header("Abaddon Speed Reduction Settings")]
    [SerializeField, Tooltip("Percentage of current speed to reduce each time Abaddon takes damage (e.g., 0.05 = 5%).")]
    private float speedReductionPercentage = 0.05f;

    [Header("Sprite Switching Settings")]
    [SerializeField, Tooltip("Sprites to cycle through each time a set number of hits are received.")]
    private Sprite[] hitSprites;
    [SerializeField, Tooltip("Number of hits required to switch sprite.")]
    private int hitsPerSpriteSwitch = 20;
    private int totalHitCount = 0;

    private EnemyMovement enemyMovement;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement == null)
        {
            Debug.LogError($"{gameObject.name}: Missing EnemyMovement component!");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: Missing SpriteRenderer component!");
        }
    }

    public override void TakeDamage(int dmg)
    {
        if (isDestroyed) return;
        
        // First, reduce speed.
        if (enemyMovement != null)
        {
            float oldSpeed = enemyMovement.moveSpeed;
            float newSpeed = oldSpeed * (1f - speedReductionPercentage);
            enemyMovement.UpdateSpeed(newSpeed);
            Debug.Log($"{gameObject.name} speed reduced from {oldSpeed} to {newSpeed} due to damage.");
        }
        
        // Increment hit count and check for sprite switch.
        totalHitCount++;
        CheckAndSwitchSprite();
        
        // Then, apply damage.
        base.TakeDamage(dmg);
    }

    public override void TakeDamageDOT(int dmg)
    {
        if (isDestroyed) return;

        // First, reduce speed.
        if (enemyMovement != null)
        {
            float oldSpeed = enemyMovement.moveSpeed;
            float newSpeed = oldSpeed * (1f - speedReductionPercentage);
            enemyMovement.UpdateSpeed(newSpeed);
            Debug.Log($"{gameObject.name} speed reduced from {oldSpeed} to {newSpeed} due to DOT damage.");
        }
        
        // Increment hit count and check for sprite switch.
        totalHitCount++;
        CheckAndSwitchSprite();
        
        // Then, apply DOT damage.
        base.TakeDamageDOT(dmg);
    }

    /// <summary>
    /// Checks if the total hit count is a multiple of hitsPerSpriteSwitch.
    /// If so, cycles to the next sprite in the hitSprites array.
    /// </summary>
    private void CheckAndSwitchSprite()
    {
        if (hitSprites != null && hitSprites.Length > 0 && totalHitCount % hitsPerSpriteSwitch == 0)
        {
            int spriteIndex = ((totalHitCount / hitsPerSpriteSwitch) - 1) % hitSprites.Length;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = hitSprites[spriteIndex];
                Debug.Log($"{gameObject.name} switched sprite to index {spriteIndex} after {totalHitCount} hits.");
            }
        }
    }
}

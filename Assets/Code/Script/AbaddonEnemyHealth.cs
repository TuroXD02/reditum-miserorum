using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbaddonEnemyHealth : EnemyHealth
{
    [Header("Abaddon Speed Reduction Settings")]
    [SerializeField, Tooltip("Percentage of current speed to reduce each time Abaddon takes damage (e.g., 0.05 = 5%).")]
    private float speedReductionPercentage = 0.05f;

    private EnemyMovement enemyMovement;

    private void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement == null)
        {
            Debug.LogError($"{gameObject.name}: Missing EnemyMovement component!");
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
        
        // Then, apply DOT damage.
        base.TakeDamageDOT(dmg);
    }
}
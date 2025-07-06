using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyGroupSpeedBoost : MonoBehaviour
{
    [Header("Group Speed Settings")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private int requiredNearbyCount = 3;
    [SerializeField] private float speedMultiplier = 1.25f;
    [SerializeField] private float checkInterval = 0.5f;

    private EnemyMovement movement;
    private float baseSpeed;
    private bool boosted = false;

    private void Start()
    {
        movement = GetComponent<EnemyMovement>();
        baseSpeed = movement.BaseSpeed;

        StartCoroutine(CheckNearbyEnemiesRoutine());
    }

    private IEnumerator CheckNearbyEnemiesRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            CheckAndApplySpeedBoost();
        }
    }

    private void CheckAndApplySpeedBoost()
    {
        int sameTypeCount = 0;

        // Find all nearby colliders within detection radius
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        foreach (Collider2D col in nearby)
        {
            if (col.gameObject == gameObject) continue; // Ignore self

            // Check if same prefab type by comparing component or tag
            EnemyGroupSpeedBoost other = col.GetComponent<EnemyGroupSpeedBoost>();
            if (other != null && other.GetType() == this.GetType())
            {
                sameTypeCount++;
            }
        }

        if (sameTypeCount >= requiredNearbyCount)
        {
            if (!boosted)
            {
                movement.UpdateSpeed(baseSpeed * speedMultiplier);
                boosted = true;
            }
        }
        else
        {
            if (boosted)
            {
                movement.UpdateSpeed(baseSpeed);
                boosted = false;
            }
        }
    }

    // Optional: visualize detection radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

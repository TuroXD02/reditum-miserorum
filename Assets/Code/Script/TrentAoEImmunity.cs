using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrentAoEImmunity : MonoBehaviour
{
    [Header("AoE Settings")]
    [SerializeField, Tooltip("Radius of the AoE that grants slow immunity to other enemies.")]
    private float aoeRadius = 3.5f;  // Set AoE radius to 3.5 units

    [Header("Animated Visual Settings")]
    [SerializeField, Tooltip("Prefab for the animated AoE effect (should include an Animator component).")]
    private GameObject aoeVisualPrefab;
    [SerializeField, Tooltip("Scaling factor for the visual effect. Use values less than 1 to make it smaller.")]
    private float vfxScaleFactor = 0.7f;  // Adjust this value to scale down the VFX

    [Header("Layer Settings")]
    [SerializeField, Tooltip("Layer mask for enemies to be granted slow immunity.")]
    private LayerMask enemyMask;

    private GameObject aoeVisualInstance;

    private void Start()
    {
        if (aoeVisualPrefab != null)
        {
            // Instantiate the animated AoE effect as a child of Trent.
            aoeVisualInstance = Instantiate(aoeVisualPrefab, transform.position, Quaternion.identity, transform);
            // Scale it so that its diameter equals 2 * aoeRadius, then apply the vfx scale factor.
            float scale = aoeRadius * 2f * vfxScaleFactor;
            aoeVisualInstance.transform.localScale = new Vector3(scale, scale, 1f);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No AoE visual prefab assigned!");
        }
    }

    private void Update()
    {
        // Keep the AoE visual centered on Trent.
        if (aoeVisualInstance != null)
        {
            aoeVisualInstance.transform.position = transform.position;
        }
        
        // Get all enemy colliders within the AoE.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, enemyMask);
        HashSet<Transform> enemiesInRange = new HashSet<Transform>();

        foreach (Collider2D hit in hits)
        {
            if (hit.transform == transform)
                continue;
            enemiesInRange.Add(hit.transform);

            // Add SlowImmunity marker if not already present.
            if (hit.GetComponent<SlowImmunity>() == null)
            {
                hit.gameObject.AddComponent<SlowImmunity>();
                Debug.Log($"Added SlowImmunity to {hit.transform.name}");
            }
        }
        
        // For each enemy in range, always reset its speed to its BaseSpeed.
        foreach (Transform enemyTransform in enemiesInRange)
        {
            EnemyMovement em = enemyTransform.GetComponent<EnemyMovement>();
            if (em != null)
            {
                em.ResetSpeed();
                Debug.Log($"Reset speed for {enemyTransform.name} because they are in Trent's AoE.");
            }
        }
        
        // Remove immunity from enemies that have left the AoE.
        RemoveImmunityFromEnemiesOutside(enemiesInRange);
    }

    /// <summary>
    /// Removes the SlowImmunity marker from any enemy that is no longer within the AoE.
    /// </summary>
    /// <param name="enemiesInRange">Set of enemy transforms currently in range.</param>
    private void RemoveImmunityFromEnemiesOutside(HashSet<Transform> enemiesInRange)
    {
        // Find all SlowImmunity markers in the scene.
        SlowImmunity[] allImmunities = FindObjectsOfType<SlowImmunity>();
        foreach (SlowImmunity immunity in allImmunities)
        {
            if (immunity == null || immunity.gameObject == gameObject)
                continue;
            // If this enemy is not in the set, remove its immunity.
            if (!enemiesInRange.Contains(immunity.transform))
            {
                Debug.Log($"Removed SlowImmunity from {immunity.transform.name} (outside AoE)");
                Destroy(immunity);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a full orange circle with 60% transparency in the Scene view.
        Color gizmoColor = new Color(1f, 0.5f, 0f, 0.6f);
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}

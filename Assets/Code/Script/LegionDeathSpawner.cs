using UnityEngine;

public class LegionDeathSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject spawnPrefab; // Prefab to spawn upon Legion's death.
    [SerializeField] private int spawnCount = 10;    // Number of enemies to spawn.

    private void OnDisable()
    {
        if (!Application.isPlaying) return;

        Vector3 deathPosition = transform.position;

        // Get Legion's movement progress if it has an EnemyMovement script
        EnemyMovement legionMovement = GetComponent<EnemyMovement>();
        float legionProgress = legionMovement != null ? legionMovement.GetProgress() : 0f;

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject spawnedEnemy = Instantiate(spawnPrefab, deathPosition, Quaternion.identity);

            // Transfer movement progress to the spawned minions
            EnemyMovement minionMovement = spawnedEnemy.GetComponent<EnemyMovement>();
            if (minionMovement != null)
            {
                minionMovement.SetProgress(legionProgress);
            }
        }
    }
}
using UnityEngine;

public class LegionDeathSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject spawnPrefab; // Prefab to spawn upon Legion's death.
    [SerializeField] private int spawnCount = 10;    // Number of enemies to spawn.

    private void OnDisable()
    {
        if (LoseScreen.IsSceneUnloading()) return;
        if (!Application.isPlaying) return;

        Vector3 deathPosition = transform.position;

        EnemyMovement legionMovement = GetComponent<EnemyMovement>();
        float legionProgress = legionMovement != null ? legionMovement.GetProgress() : 0f;

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject spawnedEnemy = Instantiate(spawnPrefab, deathPosition, Quaternion.identity);

            EnemyMovement minionMovement = spawnedEnemy.GetComponent<EnemyMovement>();
            if (minionMovement != null)
            {
                minionMovement.SetProgress(legionProgress);
            }
        }
    }
}
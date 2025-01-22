using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] enemyPrefabs; // Array of enemy prefabs to spawn.

    [Header("Attributes")]
    [SerializeField] private int baseEnemies; // Base number of enemies in the first wave.
    [SerializeField] private float enemiesPerSecond; // Initial spawn rate of enemies per second.
    [SerializeField] private float timeBetweenWaves; // Time gap between waves.
    [SerializeField] private float difficultyScalingFactor; // Factor to scale difficulty with each wave.
    [SerializeField] private float enemiesPerSecondCap; // Maximum spawn rate limit.
    [SerializeField] private float waveTimeout; // Maximum time allowed for a wave before it's forced to end.

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent(); // Event triggered when an enemy is destroyed.

    // State variables to track wave progression.
    private int currentWave = 1; // Current wave number.
    private float timeSinceLastSpawn; // Time elapsed since the last enemy spawn.
    private int enemiesAlive; // Number of enemies currently alive.
    private int enemiesLeftToSpawn; // Number of enemies remaining to spawn in the current wave.
    private float eps; // Calculated enemies per second for the current wave.
    private bool isSpawning = false; // Flag to indicate if enemies are currently being spawned.

    private Coroutine waveTimeoutCoroutine; // Reference to the timeout coroutine for cancelation.

    private void Awake()
    {
        // Register the EnemyDestroyed method to the onEnemyDestroy event.
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    private void Start()
    {
        // Start the first wave.
        StartCoroutine(StartWave());
    }

    private void Update()
    {
        // Skip if spawning is not active.
        if (!isSpawning) return;

        // Increment the time since the last spawn.
        timeSinceLastSpawn += Time.deltaTime;

        // Check if it's time to spawn a new enemy and there are still enemies left to spawn.
        if (timeSinceLastSpawn >= (1f / eps) && enemiesLeftToSpawn > 0)
        {
            SpawnEnemy(); // Spawn an enemy.
            enemiesLeftToSpawn--; // Decrease the remaining spawn count.
            enemiesAlive++; // Increase the count of alive enemies.
            timeSinceLastSpawn = 0f; // Reset the spawn timer.
        }

        // End the wave if there are no more enemies to spawn and no enemies are alive.
        if (enemiesAlive <= 0 && enemiesLeftToSpawn <= 0 && isSpawning)
        {
            EndWave();
        }
    }

    private void EnemyDestroyed()
    {
        // Decrease the count of alive enemies, ensuring it doesn't go below zero.
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        // Debug.Log($"Enemy destroyed. Enemies Alive: {enemiesAlive}");
    }

    private void EndWave()
    {
        // Stop spawning and reset the spawn timer.
        isSpawning = false;
        timeSinceLastSpawn = 0f;

        // Stop the timeout coroutine if it is running.
        if (waveTimeoutCoroutine != null)
        {
            StopCoroutine(waveTimeoutCoroutine);
        }

        // Move to the next wave.
        currentWave++;
        Debug.Log($"Wave {currentWave - 1} ended. Preparing for Wave {currentWave}...");
        StartCoroutine(StartWave());
    }

    private IEnumerator StartWave()
    {
        // Wait for the specified time between waves.
        Debug.Log($"Starting Wave {currentWave}. Waiting {timeBetweenWaves} seconds before spawning...");
        yield return new WaitForSeconds(timeBetweenWaves);

        // Enable spawning for the new wave.
        isSpawning = true;
        enemiesLeftToSpawn = EnemiesPerWave(); // Calculate the number of enemies to spawn in this wave.
        eps = EnemiesPerSecond(); // Calculate the spawn rate for this wave.

        Debug.Log($"Wave {currentWave} started: {enemiesLeftToSpawn} enemies to spawn at {eps} EPS.");

        // Start the timeout coroutine for this wave.
        waveTimeoutCoroutine = StartCoroutine(WaveTimeout());
    }

    private IEnumerator WaveTimeout()
    {
        // Wait for the timeout duration
        yield return new WaitForSeconds(waveTimeout);

        // Force the wave to end if it hasn't ended naturally.
        if (isSpawning)
        {
            
            EndWave();
        }
    }

    private void SpawnEnemy()
    {
        // Randomly select an enemy prefab to spawn.
        int index = Random.Range(0, enemyPrefabs.Length);
        GameObject prefabToSpawn = enemyPrefabs[index];

        // Spawn the enemy at a random position near the start point.
        Vector3 position = new Vector3(
            Random.Range(-1, +1),
            Random.Range(LevelManager.main.startPoint.position.y - 1, LevelManager.main.startPoint.position.y + 1),
            0
        );

        Instantiate(prefabToSpawn, position, Quaternion.identity);
    }

    private int EnemiesPerWave()
    {
        // Calculate the number of enemies to spawn in the current wave.
        // This calculation considers:
        // - `baseEnemies`: The base number of enemies for the first wave.
        // - `currentWave`: The current wave number, increasing as the game progresses.
        // - `difficultyScalingFactor`: A multiplier that determines how quickly the enemy count increases with each wave.

        // `Mathf.Pow(currentWave, difficultyScalingFactor)` exponentially scales the enemy count based on the wave number.
        // For example, if `currentWave` is 2 and `difficultyScalingFactor` is 1.5:
        // The result will be `baseEnemies * Mathf.Pow(2, 1.5)`.

        int calculatedEnemies = Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultyScalingFactor));

        // The result is rounded to the nearest integer using Mathf.RoundToInt, ensuring a whole number of enemies.
        // This ensures no fractional enemies are calculated, which wouldn't make sense in the game.

        return calculatedEnemies; // Return the calculated enemy count for this wave.
    }

    private float EnemiesPerSecond()
    {
        // Calculate the number of enemies spawned per second (EPS) for the current wave.
        // This calculation takes into account:
        // - `enemiesPerSecond`: The base spawn rate of enemies in the first wave.
        // - `currentWave`: The current wave number, which increases as the game progresses.
        // - `difficultyScalingFactor`: A multiplier to increase the spawn rate as waves progress.
    
        // `Mathf.Pow(currentWave, difficultyScalingFactor)` exponentially scales the spawn rate based on the wave number.
        // For example, if `currentWave` is 3 and `difficultyScalingFactor` is 1.2:
        // The spawn rate will increase based on `enemiesPerSecond * Mathf.Pow(3, 1.2)`.

        float calculatedEps = Mathf.Clamp(
            enemiesPerSecond * Mathf.Pow(currentWave, difficultyScalingFactor), // Calculate the spawn rate
            0f,                                                               // Minimum EPS (0 - no spawning)
            enemiesPerSecondCap                                               // Maximum EPS (cap on how fast enemies can spawn)
        );

        // `Mathf.Clamp` ensures that the spawn rate stays between the defined minimum (0) and maximum (`enemiesPerSecondCap`).

        return calculatedEps; // Return the calculated enemies-per-second rate for this wave.
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public Transform startPoint; // Must be assigned in the Inspector.
    [SerializeField] private GameObject[] enemyPrefabs; // Base enemy prefabs.
    [SerializeField] private GameObject[] extraEnemyPrefabs; // Additional enemy prefabs to add starting from wave 7.
    [SerializeField] private GameObject[] moreEnemyPrefabs;  // Additional enemy prefabs to add starting from wave 12.

    [Header("Attributes")]
    [SerializeField] private int baseEnemies = 5; // Base number of enemies in the first wave.
    [SerializeField] private float enemiesPerSecond = 1f; // Base spawn rate.
    [SerializeField] private float timeBetweenWaves = 5f; // Time between waves.
    [SerializeField] private float difficultyScalingFactor = 1.2f; // How quickly difficulty scales.
    [SerializeField] private float enemiesPerSecondCap = 5f; // Maximum EPS.
    [SerializeField] private float waveTimeout = 30f; // Maximum time allowed for a wave.

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    // State variables.
    private int currentWave = 1; 
    private float timeSinceLastSpawn = 0f;
    private int enemiesAlive = 0;
    private int enemiesLeftToSpawn = 0;
    private float eps = 0f;
    private bool isSpawning = false;

    private Coroutine waveTimeoutCoroutine;

    private void Awake()
    {
        // Register enemy destroyed event.
        onEnemyDestroy.AddListener(EnemyDestroyed);
    }

    private void Start()
    {       
        StartCoroutine(StartWave());
    }

    private void Update()
    {
        if (!isSpawning) return;

        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= (1f / eps) && enemiesLeftToSpawn > 0)
        {
            SpawnEnemy();
            enemiesLeftToSpawn--;
            enemiesAlive++;
            timeSinceLastSpawn = 0f;
        }

        if (enemiesAlive <= 0 && enemiesLeftToSpawn <= 0 && isSpawning)
        {
            EndWave();
        }
    }

    private void EnemyDestroyed()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
    }

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;

        if (waveTimeoutCoroutine != null)
        {
            StopCoroutine(waveTimeoutCoroutine);
        }

        Debug.Log($"[EnemySpawner] Wave {currentWave} ended.");
        currentWave++;
        
        StartCoroutine(StartWave());
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        isSpawning = true;
        enemiesLeftToSpawn = EnemiesPerWave();
        eps = EnemiesPerSecond();

        waveTimeoutCoroutine = StartCoroutine(WaveTimeout());
    }

    private IEnumerator WaveTimeout()
    {
        yield return new WaitForSeconds(waveTimeout);
        if (isSpawning)
        {
            EndWave();
        }
    }

    private void SpawnEnemy()
    {
        GameObject[] spawnPool;

        // If currentWave is 12 or higher and there are entries in moreEnemyPrefabs, combine all pools.
        if (currentWave >= 12 && moreEnemyPrefabs.Length > 0)
        {
            List<GameObject> combined = new List<GameObject>(enemyPrefabs);
            if (extraEnemyPrefabs.Length > 0)
            {
                combined.AddRange(extraEnemyPrefabs);
            }
            combined.AddRange(moreEnemyPrefabs);
            spawnPool = combined.ToArray();
            Debug.Log("[EnemySpawner] Using combined enemy spawn pool for wave 12+.");
        }
        // Else if currentWave is 7 or higher, use enemyPrefabs + extraEnemyPrefabs.
        else if (currentWave >= 7 && extraEnemyPrefabs.Length > 0)
        {
            List<GameObject> combined = new List<GameObject>(enemyPrefabs);
            combined.AddRange(extraEnemyPrefabs);
            spawnPool = combined.ToArray();
            Debug.Log("[EnemySpawner] Using combined enemy spawn pool for wave 7+.");
        }
        else
        {
            spawnPool = enemyPrefabs;
        }

        int index = Random.Range(0, spawnPool.Length);
        GameObject prefabToSpawn = spawnPool[index];

        if (startPoint == null)
        {
            Debug.LogError("[EnemySpawner] startPoint is not assigned!");
            return;
        }

        Vector3 position = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(startPoint.position.y - 1f, startPoint.position.y + 1f),
            0
        );

        Instantiate(prefabToSpawn, position, Quaternion.identity);
    }

    private int EnemiesPerWave()
    {
        int calculatedEnemies = Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultyScalingFactor));
        return calculatedEnemies;
    }

    private float EnemiesPerSecond()
    {
        float calculatedEps = Mathf.Clamp(enemiesPerSecond * Mathf.Pow(currentWave, difficultyScalingFactor), 0f, enemiesPerSecondCap);
        return calculatedEps;
    }
}

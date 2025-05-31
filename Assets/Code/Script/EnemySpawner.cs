

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public Transform startPoint;

    [SerializeField] private GameObject[] waveEnemies1;
    [SerializeField] private GameObject[] waveEnemies2;
    [SerializeField] private GameObject[] waveEnemies3;
    [SerializeField] private GameObject[] waveEnemies4;
    [SerializeField] private GameObject[] waveEnemies5;
    [SerializeField] private GameObject[] waveEnemies6;
    [SerializeField] private GameObject[] waveEnemies7;
    [SerializeField] private GameObject[] waveEnemies8;
    [SerializeField] private GameObject[] waveEnemies9;
    [SerializeField] private GameObject[] waveEnemies10;
    [SerializeField] private GameObject[] waveEnemies11;
    [SerializeField] private GameObject[] waveEnemies12;

    [Header("Attributes")]
    [SerializeField] private int baseEnemies = 5;
    [SerializeField] private float enemiesPerSecond = 1f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficultyScalingFactor = 1.2f;
    [SerializeField] private float enemiesPerSecondCap = 5f;
    [SerializeField] private float waveTimeout = 30f;

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private int currentWave = 1;
    private float timeSinceLastSpawn = 0f;
    private int enemiesAlive = 0;
    private int enemiesLeftToSpawn = 0;
    private float eps = 0f;
    private bool isSpawning = false;
    private Coroutine waveTimeoutCoroutine;
    private bool[] waveIntroduced;
    private List<GameObject> currentWaveSpawnPool;

    private void Awake()
    {
        onEnemyDestroy.AddListener(EnemyDestroyed);
        waveIntroduced = new bool[12];
        currentWaveSpawnPool = new List<GameObject>();
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

        // Set up spawn pool for this wave
        SetupWaveSpawnPool();

        waveTimeoutCoroutine = StartCoroutine(WaveTimeout());
    }

    private void SetupWaveSpawnPool()
    {
        currentWaveSpawnPool.Clear();
        int tier = Mathf.Clamp((currentWave - 1) / 3 + 1, 1, 12);

        if (!waveIntroduced[tier - 1])
        {
            // If this tier is being introduced, only use enemies from this tier
            waveIntroduced[tier - 1] = true;
            GameObject[] newEnemies = GetWaveEnemiesByIndex(tier);
            if (newEnemies != null && newEnemies.Length > 0)
            {
                currentWaveSpawnPool.AddRange(newEnemies);
            }
        }
        else
        {
            // If no new tier is being introduced, use all previously introduced enemies
            for (int i = 1; i <= tier; i++)
            {
                if (waveIntroduced[i - 1])
                {
                    GameObject[] pool = GetWaveEnemiesByIndex(i);
                    if (pool != null && pool.Length > 0)
                    {
                        currentWaveSpawnPool.AddRange(pool);
                    }
                }
            }
        }

        if (currentWaveSpawnPool.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemies available for the current wave!");
        }
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
        if (currentWaveSpawnPool.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] No enemies to spawn!");
            return;
        }

        if (startPoint == null)
        {
            Debug.LogError("[EnemySpawner] startPoint is not assigned!");
            return;
        }

        int index = Random.Range(0, currentWaveSpawnPool.Count);
        GameObject prefabToSpawn = currentWaveSpawnPool[index];

        Vector3 position = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(startPoint.position.y - 1f, startPoint.position.y + 1f),
            0
        );

        Instantiate(prefabToSpawn, position, Quaternion.identity);
    }

    private GameObject[] GetWaveEnemiesByIndex(int index)
    {
        return index switch
        {
            1 => waveEnemies1,
            2 => waveEnemies2,
            3 => waveEnemies3,
            4 => waveEnemies4,
            5 => waveEnemies5,
            6 => waveEnemies6,
            7 => waveEnemies7,
            8 => waveEnemies8,
            9 => waveEnemies9,
            10 => waveEnemies10,
            11 => waveEnemies11,
            12 => waveEnemies12,
            _ => null
        };
    }

    private int EnemiesPerWave()
    {
        return Mathf.RoundToInt(baseEnemies * Mathf.Pow(currentWave, difficultyScalingFactor));
    }

    private float EnemiesPerSecond()
    {
        return Mathf.Clamp(enemiesPerSecond * Mathf.Pow(currentWave, difficultyScalingFactor), 0f, enemiesPerSecondCap);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class WeightedEnemy
{
    public GameObject enemyPrefab;
    public float weight = 1f;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    public Transform startPoint;

    [Header("Enemy Waves")]
    [SerializeField] private WeightedEnemy[] waveEnemies1;
    [SerializeField] private WeightedEnemy[] waveEnemies2;
    [SerializeField] private WeightedEnemy[] waveEnemies3;
    [SerializeField] private WeightedEnemy[] waveEnemies4;
    [SerializeField] private WeightedEnemy[] waveEnemies5;
    [SerializeField] private WeightedEnemy[] waveEnemies6;
    [SerializeField] private WeightedEnemy[] waveEnemies7;
    [SerializeField] private WeightedEnemy[] waveEnemies8;
    [SerializeField] private WeightedEnemy[] waveEnemies9;
    [SerializeField] private WeightedEnemy[] waveEnemies10;
    [SerializeField] private WeightedEnemy[] waveEnemies11;
    [SerializeField] private WeightedEnemy[] waveEnemies12;

    [Header("Attributes")]
    [SerializeField] private float baseWaveWeight = 5f;
    [SerializeField] private float enemiesPerSecond = 1f;
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private float difficultyScalingFactor = 1.2f;
    [SerializeField] private float enemiesPerSecondCap = 5f;
    [SerializeField] private float waveTimeout = 30f;

    [Header("Events")]
    public static UnityEvent onEnemyDestroy = new UnityEvent();

    private int currentWave = 1;
    private float timeSinceLastSpawn = 0f;
    private float eps = 0f;
    private bool isSpawning = false;
    private Coroutine waveTimeoutCoroutine;
    private bool[] waveIntroduced;
    private List<WeightedEnemy> currentWaveSpawnPool;
    private float currentWaveWeight;
    private float currentWaveWeightUsed;

    private void Awake()
    {
        waveIntroduced = new bool[12];
        currentWaveSpawnPool = new List<WeightedEnemy>();

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

        if (timeSinceLastSpawn >= (1f / eps))
        {
            TrySpawnEnemy();
            timeSinceLastSpawn = 0f;
        }

        if (currentWaveWeightUsed >= currentWaveWeight)
        {
            EndWave();
        }
    }

    private void EnemyDestroyed()
    {
        // Optional: Count deaths, update UI, etc.
        Debug.Log("[EnemySpawner] An enemy has been destroyed.");
    }

    private void EndWave()
    {
        isSpawning = false;
        timeSinceLastSpawn = 0f;

        if (waveTimeoutCoroutine != null)
            StopCoroutine(waveTimeoutCoroutine);

        Debug.Log($"[EnemySpawner] Wave {currentWave} ended.");
        currentWave++;

        StartCoroutine(StartWave());
    }

    private IEnumerator StartWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        isSpawning = true;
        eps = EnemiesPerSecond();
        currentWaveWeight = GetWaveWeight();
        currentWaveWeightUsed = 0f;

        Debug.Log($"[EnemySpawner] Starting Wave {currentWave} | Weight Budget: {currentWaveWeight:F2}");

        SetupWaveSpawnPool();

        waveTimeoutCoroutine = StartCoroutine(WaveTimeout());
    }

    private void SetupWaveSpawnPool()
    {
        currentWaveSpawnPool.Clear();
        int tier = Mathf.Clamp((currentWave - 1) / 3 + 1, 1, 12);

        if (!waveIntroduced[tier - 1])
        {
            waveIntroduced[tier - 1] = true;
            var newEnemies = GetWaveEnemiesByIndex(tier);
            if (newEnemies != null) currentWaveSpawnPool.AddRange(newEnemies);
        }
        else
        {
            for (int i = 1; i <= tier; i++)
            {
                if (waveIntroduced[i - 1])
                {
                    var pool = GetWaveEnemiesByIndex(i);
                    if (pool != null) currentWaveSpawnPool.AddRange(pool);
                }
            }
        }

        if (currentWaveSpawnPool.Count == 0)
            Debug.LogWarning("[EnemySpawner] No enemies available for this wave!");
    }

    private IEnumerator WaveTimeout()
    {
        yield return new WaitForSeconds(waveTimeout);
        if (isSpawning) EndWave();
    }

    private void TrySpawnEnemy()
    {
        if (currentWaveSpawnPool.Count == 0) return;

        WeightedEnemy candidate;
        int attempts = 0;
        do
        {
            candidate = currentWaveSpawnPool[Random.Range(0, currentWaveSpawnPool.Count)];
            attempts++;
        }
        while (candidate.weight + currentWaveWeightUsed > currentWaveWeight && attempts < 10);

        if (candidate.weight + currentWaveWeightUsed <= currentWaveWeight)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(startPoint.position.y - 1f, startPoint.position.y + 1f),
                0f
            );

            Instantiate(candidate.enemyPrefab, spawnPos, Quaternion.identity);
            currentWaveWeightUsed += candidate.weight;

            Debug.Log($"[EnemySpawner] Spawned {candidate.enemyPrefab.name} | Used: {candidate.weight:F2} | Remaining: {currentWaveWeight - currentWaveWeightUsed:F2}");
        }
    }

    private WeightedEnemy[] GetWaveEnemiesByIndex(int index)
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

    private float GetWaveWeight()
    {
        return baseWaveWeight * Mathf.Pow(currentWave, difficultyScalingFactor);
    }

    private float EnemiesPerSecond()
    {
        return Mathf.Clamp(enemiesPerSecond * Mathf.Pow(currentWave, difficultyScalingFactor), 0f, enemiesPerSecondCap);
    }
}

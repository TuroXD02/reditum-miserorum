using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] public float moveSpeed = 1f; // Use Inspector value as initial base speed
    private float baseSpeed;
    public float BaseSpeed => baseSpeed;

    private Transform target;
    private int pathIndex = 0;

    private float zeroSpeedTimer = 0f;
    private const float resetDelay = 5f;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        baseSpeed = moveSpeed; // Cache the true original speed for reset purposes
    }

    private void Start()
    {
        if (LevelManager.main == null || LevelManager.main.path == null || LevelManager.main.path.Length == 0)
        {
            Debug.LogError("LevelManager or path not initialized!");
            enabled = false;
            return;
        }

        target = LevelManager.main.path[pathIndex];

        if (baseSpeed <= 0f)
        {
            Debug.LogWarning($"{gameObject.name} spawned with baseSpeed = {baseSpeed}. Enemy may not move.");
        }
    }

    private void Update()
    {
        HandleSpeedResetTimer();
        HandleWaypointProgression();
    }

    private void FixedUpdate()
    {
        if (moveSpeed > 0f)
        {
            Vector2 direction = (GetRandomizedTarget() - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void HandleSpeedResetTimer()
    {
        if (moveSpeed == 0f)
        {
            zeroSpeedTimer += Time.deltaTime;
            if (zeroSpeedTimer >= resetDelay)
            {
                ResetSpeed();
                zeroSpeedTimer = 0f;
            }
        }
        else
        {
            zeroSpeedTimer = 0f;
        }
    }

    private void HandleWaypointProgression()
    {
        if (Vector2.Distance(transform.position, target.position) <= 0.2f)
        {
            pathIndex++;
            if (pathIndex >= LevelManager.main.path.Length)
            {
                EnemySpawner.onEnemyDestroy?.Invoke();
                Destroy(gameObject);
            }
            else
            {
                target = LevelManager.main.path[pathIndex];
            }
        }
    }

    private Vector3 GetRandomizedTarget()
    {
        return new Vector3(
            Random.Range(target.position.x - 0.1f, target.position.x + 0.1f),
            Random.Range(target.position.y - 0.2f, target.position.y + 0.2f),
            0);
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed; // Resets to the original speed of the prefab
    }

    public float GetProgress()
    {
        if (pathIndex == 0) return 0f;

        Transform prev = LevelManager.main.path[pathIndex - 1];
        Transform curr = LevelManager.main.path[pathIndex];

        float segmentLength = Vector2.Distance(prev.position, curr.position);
        float distTravelled = Vector2.Distance(prev.position, transform.position);
        float fraction = (segmentLength > 0f) ? Mathf.Clamp01(distTravelled / segmentLength) : 0f;

        return (pathIndex - 1) + fraction;
    }

    public void SetProgress(float progress)
    {
        int index = Mathf.FloorToInt(progress);
        float fraction = progress - index;

        index = Mathf.Clamp(index, 0, LevelManager.main.path.Length - 2);
        pathIndex = index + 1;

        Transform prev = LevelManager.main.path[index];
        Transform curr = LevelManager.main.path[pathIndex];

        transform.position = Vector3.Lerp(prev.position, curr.position, fraction);
        target = curr;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] public float moveSpeed;         // Current speed.
    private float baseSpeed;                         // The original speed.
    public float BaseSpeed { get { return baseSpeed; } } // Exposed base speed.

    private Transform target;                        // Current waypoint target.
    private int pathIndex = 0;                       // Index in LevelManager.main.path.

    private void Start()
    {
        baseSpeed = moveSpeed;
        target = LevelManager.main.path[pathIndex];  
    }

    private void Update()
    {
        // If the enemy is near the current waypoint, advance to the next.
        if (Vector2.Distance(target.position, transform.position) <= 0.2f)
        {
            pathIndex++;
            if (pathIndex == LevelManager.main.path.Length)
            {
                EnemySpawner.onEnemyDestroy.Invoke();
                Destroy(gameObject);
                return;
            }
            else
            {
                target = LevelManager.main.path[pathIndex];
            }
        }
    }

    private void FixedUpdate()
    {
        // Slight randomization for natural movement.
        Vector3 RandomTarget = new Vector3(
            Random.Range(target.position.x - 0.4f, target.position.x + 0.4f),
            Random.Range(target.position.y - 1, target.position.y + 1),
            0);
        Vector2 direction = (RandomTarget - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed;
    }

    /// <summary>
    /// Returns the enemy's progress along the path as a float.
    /// For example, if the enemy has completed 2 segments and is 50% through the 3rd,
    /// this method returns 2.5.
    /// </summary>
    public float GetProgress()
    {
        if (pathIndex == 0)
            return 0f;
        Transform prev = LevelManager.main.path[pathIndex - 1];
        Transform curr = LevelManager.main.path[pathIndex];
        float segmentLength = Vector2.Distance(prev.position, curr.position);
        float distTravelled = Vector2.Distance(prev.position, transform.position);
        float fraction = (segmentLength > 0) ? Mathf.Clamp01(distTravelled / segmentLength) : 0f;
        return (pathIndex - 1) + fraction;
    }

    /// <summary>
    /// Sets the enemy's position along the path based on the provided progress value.
    /// 'progress' should be between 0 and (number of segments).
    /// For example, a progress value of 2.5 will place the enemy halfway between waypoint 2 and 3.
    /// </summary>
    public void SetProgress(float progress)
    {
        int index = Mathf.FloorToInt(progress);
        float fraction = progress - index;
        // Clamp index to ensure valid range. There must be at least two waypoints.
        index = Mathf.Clamp(index, 0, LevelManager.main.path.Length - 2);
        pathIndex = index + 1;
        Transform prev = LevelManager.main.path[index];
        Transform curr = LevelManager.main.path[pathIndex];
        transform.position = Vector3.Lerp(prev.position, curr.position, fraction);
        target = curr;
    }
}

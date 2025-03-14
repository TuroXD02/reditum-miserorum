using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrentEnemyMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Attributes")]
    [SerializeField] public float moveSpeed;  // Current speed.
    private float baseSpeed;                  // Base (normal) speed.
    public float BaseSpeed { get { return baseSpeed; } }  // Expose base speed.

    private Transform target;
    private int pathIndex = 0;

    private void Start()
    {
        baseSpeed = moveSpeed;
        target = LevelManager.main.path[pathIndex];
    }

    private void Update()
    {
        if (Vector2.Distance(transform.position, target.position) <= 0.2f)
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
        Vector3 randomTarget = new Vector3(
            Random.Range(target.position.x - 0.4f, target.position.x + 0.4f),
            Random.Range(target.position.y - 1, target.position.y + 1),
            0);
        Vector2 direction = (randomTarget - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    /// <summary>
    /// Updates the speed only if newSpeed is greater than or equal to baseSpeed.
    /// This prevents slowing effects from reducing Trent's speed.
    /// </summary>
    public void UpdateSpeed(float newSpeed)
    {
        if (newSpeed < baseSpeed)
        {
            // Ignore slow commands.
            Debug.Log($"{gameObject.name}: Ignoring slow effect. Current speed remains {moveSpeed}");
            return;
        }
        else
        {
            moveSpeed = newSpeed;
        }
    }

    public void ResetSpeed()
    {
        moveSpeed = baseSpeed;
    }
}

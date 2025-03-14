using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Korah : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private float boostInterval = 5f;       // How often (in seconds) to boost enemy speed.
    [SerializeField] private float minBoostPercent = 0.05f;    // Minimum boost: 5%
    [SerializeField] private float maxBoostPercent = 0.10f;    // Maximum boost: 10%
    [SerializeField] private float effectDuration = 5f;        // Duration that the speed boost (and tint) lasts.

    [Header("Buff Effect Settings")]
    [SerializeField] private GameObject buffEffectPrefab;      // Prefab for the visual buff effect.
    [SerializeField] private float visualEffectDuration = 2f;    // How long the visual effect stays visible.

    [Header("Tint Settings")]
    [SerializeField] private Color targetTint = new Color(1f, 0.7f, 0.7f, 1f); // Subtle red tint.
    [SerializeField] private float tintLerpDuration = 0.5f;      // Time to fade in/out the tint.

    private void Start()
    {
        StartCoroutine(BoostEnemySpeed());
    }

    private IEnumerator BoostEnemySpeed()
    {
        while (true)
        {
            yield return new WaitForSeconds(boostInterval);

            // Generate a random boost factor.
            float boostFactor = Random.Range(minBoostPercent, maxBoostPercent);

            // Find all enemies with an EnemyMovement component.
            EnemyMovement[] enemies = FindObjectsOfType<EnemyMovement>();
            foreach (EnemyMovement enemy in enemies)
            {
                // Skip self.
                if (enemy.gameObject == this.gameObject)
                    continue;

                // If the enemy is slowed (current speed is below BaseSpeed), reset its speed.
                if (enemy.moveSpeed < enemy.BaseSpeed)
                {
                    enemy.moveSpeed = enemy.BaseSpeed;
                    Debug.Log($"{enemy.gameObject.name} was slowed; resetting speed to base: {enemy.BaseSpeed}");
                }
                
                // Apply boost.
                enemy.moveSpeed *= (1f + boostFactor);
                Debug.Log($"{enemy.gameObject.name} boosted to {enemy.moveSpeed} (boost factor: {boostFactor * 100f}%)");

                // Start a coroutine to reset enemy speed after effectDuration seconds.
                StartCoroutine(ResetEnemySpeed(enemy, effectDuration));

                // Instantiate the buff effect on the enemy.
                if (buffEffectPrefab != null)
                {
                    GameObject effect = Instantiate(buffEffectPrefab, enemy.transform.position, Quaternion.identity, enemy.transform);
                    float effectScale = 1f; // Adjust as needed.
                    effect.transform.localScale = new Vector3(effectScale, effectScale, 1f);
                    Destroy(effect, visualEffectDuration);
                }

                // Apply the tint effect.
                SpriteRenderer enemyRenderer = enemy.GetComponent<SpriteRenderer>();
                if (enemyRenderer != null)
                {
                    StartCoroutine(ApplyTemporaryColor(enemyRenderer, targetTint, effectDuration, tintLerpDuration));
                }
            }

            Debug.Log("Korah boosted enemy speed by " + (boostFactor * 100f) + "% for " + effectDuration + " seconds (visual effect lasts " + visualEffectDuration + " seconds).");
        }
    }

    private IEnumerator ResetEnemySpeed(EnemyMovement enemy, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (enemy != null)
        {
            enemy.ResetSpeed();
            Debug.Log($"{enemy.gameObject.name} speed reset to base speed: {enemy.BaseSpeed}");
        }
    }

    /// <summary>
    /// Applies a temporary color tint to the enemy's sprite.
    /// Fades in the tint over fadeDuration seconds, holds for (duration - fadeDuration), then fades out.
    /// </summary>
    private IEnumerator ApplyTemporaryColor(SpriteRenderer renderer, Color newColor, float duration, float fadeDuration)
    {
        if (renderer == null)
            yield break;

        Color originalColor = renderer.color;
        float t = 0f;
        // Fade in.
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (renderer == null)
                yield break;
            renderer.color = Color.Lerp(originalColor, newColor, t / fadeDuration);
            yield return null;
        }
        if (renderer != null)
            renderer.color = newColor;
        
        yield return new WaitForSeconds(duration - fadeDuration);
        
        // Fade out.
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (renderer == null)
                yield break;
            renderer.color = Color.Lerp(newColor, originalColor, t / fadeDuration);
            yield return null;
        }
        if (renderer != null)
            renderer.color = originalColor;
    }
}

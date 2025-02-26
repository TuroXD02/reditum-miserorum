using System.Collections;
using UnityEngine;

public class EnemyPoisonEffect : MonoBehaviour
{
    private SpriteRenderer enemyRenderer;
    private Coroutine poisonCoroutine;
    private Color originalColor;
    private float effectEndTime;

    private void Awake()
    {
        enemyRenderer = GetComponent<SpriteRenderer>();
        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.color;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No SpriteRenderer found for poison effect.");
        }
    }
    
    /// <summary>
    /// Applies the poison effect by tinting the enemy's sprite to the specified poison color.
    /// If the effect is already active, refreshes the hold duration.
    /// </summary>
    /// <param name="poisonColor">The greenish poison tint color.</param>
    /// <param name="fadeDuration">Time for fading in/out.</param>
    /// <param name="holdDuration">Total time to hold the tint.</param>
    public void ApplyPoisonEffect(Color poisonColor, float fadeDuration, float holdDuration)
    {
        if (poisonCoroutine != null)
        {
            // Refresh the hold duration if already active.
            effectEndTime = Time.time + holdDuration;
            return;
        }
        poisonCoroutine = StartCoroutine(PoisonEffectRoutine(poisonColor, fadeDuration, holdDuration));
    }
    
    private IEnumerator PoisonEffectRoutine(Color poisonColor, float fadeDuration, float holdDuration)
    {
        float t = 0f;
        // Fade in to poison color.
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (enemyRenderer != null)
            {
                enemyRenderer.color = Color.Lerp(originalColor, poisonColor, t / fadeDuration);
            }
            yield return null;
        }
        if (enemyRenderer != null)
        {
            enemyRenderer.color = poisonColor;
        }
        effectEndTime = Time.time + holdDuration;
        while (Time.time < effectEndTime)
        {
            yield return null;
        }
        // Fade out back to original color.
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (enemyRenderer != null)
            {
                enemyRenderer.color = Color.Lerp(poisonColor, originalColor, t / fadeDuration);
            }
            yield return null;
        }
        if (enemyRenderer != null)
        {
            enemyRenderer.color = originalColor;
        }
        poisonCoroutine = null;
    }
}

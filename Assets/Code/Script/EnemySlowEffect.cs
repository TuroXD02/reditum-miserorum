using System.Collections;
using UnityEngine;

public class EnemySlowEffect : MonoBehaviour
{
    private SpriteRenderer enemyRenderer;
    private Coroutine slowCoroutine;
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
            Debug.LogWarning($"{gameObject.name}: No SpriteRenderer found for slow effect.");
        }
    }
    
    /// <summary>
    /// Applies the slow effect by tinting the enemy's sprite to the specified tint.
    /// If the effect is already active, the enemy's color is immediately set to the tint,
    /// and the hold duration is refreshed.
    /// </summary>
    /// <param name="tint">The blue tint color to apply.</param>
    /// <param name="fadeDuration">Time to fade in/out the tint.</param>
    /// <param name="holdDuration">Duration for which the tint is held.</param>
    public void ApplySlowEffect(Color tint, float fadeDuration, float holdDuration)
    {
        if (slowCoroutine != null)
        {
            // Refresh the hold duration.
            effectEndTime = Time.time + holdDuration;
            // Force the color to the full tint immediately.
            if (enemyRenderer != null)
            {
                enemyRenderer.color = tint;
            }
            return;
        }
        slowCoroutine = StartCoroutine(SlowEffectRoutine(tint, fadeDuration, holdDuration));
    }
    
    private IEnumerator SlowEffectRoutine(Color tint, float fadeDuration, float holdDuration)
    {
        float t = 0f;
        // Fade in to the tint.
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (enemyRenderer != null)
            {
                enemyRenderer.color = Color.Lerp(originalColor, tint, t / fadeDuration);
            }
            yield return null;
        }
        if (enemyRenderer != null)
        {
            enemyRenderer.color = tint;
        }
        // Set the effect end time.
        effectEndTime = Time.time + holdDuration;
        // Wait until the effect duration has elapsed.
        while (Time.time < effectEndTime)
        {
            yield return null;
        }
        // Fade out back to the original color.
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (enemyRenderer != null)
            {
                enemyRenderer.color = Color.Lerp(tint, originalColor, t / fadeDuration);
            }
            yield return null;
        }
        if (enemyRenderer != null)
        {
            enemyRenderer.color = originalColor;
        }
        slowCoroutine = null;
    }
}

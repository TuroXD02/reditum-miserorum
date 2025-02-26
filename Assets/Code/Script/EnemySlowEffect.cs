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
    /// Applies the slow effect by tinting the enemy's sprite to the given tint.
    /// If already active, refreshes the hold duration.
    /// </summary>
    public void ApplySlowEffect(Color tint, float fadeDuration, float holdDuration)
    {
        if (slowCoroutine != null)
        {
            // Refresh the hold duration.
            effectEndTime = Time.time + holdDuration;
            return;
        }
        slowCoroutine = StartCoroutine(SlowEffectRoutine(tint, fadeDuration, holdDuration));
    }
    
    private IEnumerator SlowEffectRoutine(Color tint, float fadeDuration, float holdDuration)
    {
        float t = 0f;
        // Fade in.
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
        effectEndTime = Time.time + holdDuration;
        while (Time.time < effectEndTime)
        {
            yield return null;
        }
        // Fade out.
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

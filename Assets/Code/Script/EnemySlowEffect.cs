using System.Collections;
using UnityEngine;

public class EnemySlowEffect : MonoBehaviour
{
    private SpriteRenderer enemyRenderer;
    private Coroutine effectCoroutine;
    private Color originalColor;
    private Color currentTargetTint;
    private float currentHoldDuration;

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
    /// Applies (or refreshes) the slow effect by tinting the enemy's sprite.
    /// If the effect is already active, it restarts the routine to refresh the hold duration.
    /// </summary>
    /// <param name="tint">The blue tint to apply.</param>
    /// <param name="fadeDuration">Time to fade in/out the tint.</param>
    /// <param name="holdDuration">Duration to hold the tint after fade in.</param>
    public void ApplySlowEffect(Color tint, float fadeDuration, float holdDuration)
    {
        currentTargetTint = tint;
        currentHoldDuration = holdDuration;
        // If already active, restart the coroutine to refresh the effect.
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        effectCoroutine = StartCoroutine(SlowEffectRoutine(fadeDuration));
    }

    private IEnumerator SlowEffectRoutine(float fadeDuration)
    {
        float t = 0f;
        // Fade in from originalColor to currentTargetTint.
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (enemyRenderer != null)
            {
                enemyRenderer.color = Color.Lerp(originalColor, currentTargetTint, t / fadeDuration);
            }
            yield return null;
        }
        if (enemyRenderer != null)
        {
            enemyRenderer.color = currentTargetTint;
        }

        // Hold the tint for the full duration.
        float holdTimer = 0f;
        while (holdTimer < currentHoldDuration)
        {
            holdTimer += Time.deltaTime;
            yield return null;
        }

        // Fade out from the tint back to originalColor.
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (enemyRenderer != null)
            {
                enemyRenderer.color = Color.Lerp(currentTargetTint, originalColor, t / fadeDuration);
            }
            yield return null;
        }
        if (enemyRenderer != null)
        {
            enemyRenderer.color = originalColor;
        }
        effectCoroutine = null;
    }
}

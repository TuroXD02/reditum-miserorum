using System.Collections;
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

            // Generate a random boost factor (e.g., 0.07 means 7% boost).
            float boostFactor = Random.Range(minBoostPercent, maxBoostPercent);

            // Find all enemies with an EnemyMovement component.
            EnemyMovement[] enemies = FindObjectsOfType<EnemyMovement>();
            foreach (EnemyMovement enemy in enemies)
            {
                if (enemy.gameObject == this.gameObject)
                    continue;

                // Permanently boost enemy speed.
                enemy.moveSpeed *= (1f + boostFactor);

                // Start a coroutine to reset enemy speed after effectDuration seconds.
                StartCoroutine(ResetEnemySpeed(enemy, effectDuration));

                // Instantiate the buff effect on the enemy.
                if (buffEffectPrefab != null)
                {
                    GameObject effect = Instantiate(buffEffectPrefab, enemy.transform.position, Quaternion.identity, enemy.transform);
                    // Optionally adjust effect scale.
                    float effectScale = 1f; // Adjust as needed.
                    effect.transform.localScale = new Vector3(effectScale, effectScale, 1f);
                    Destroy(effect, visualEffectDuration);
                }

                // Start the tint coroutine on the enemy's sprite.
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
        }
    }

    /// <summary>
    /// Applies a temporary color tint to the enemy's sprite.
    /// Fades in the tint over fadeDuration seconds, holds it for (duration - fadeDuration), then fades back.
    /// Checks for null before updating to avoid errors if the enemy is destroyed.
    /// </summary>
    private IEnumerator ApplyTemporaryColor(SpriteRenderer renderer, Color newColor, float duration, float fadeDuration)
    {
        if (renderer == null)
            yield break;
            
        Color originalColor = renderer.color;
        
        // Fade in.
        float t = 0f;
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
        
        // Hold for the remainder of the duration.
        yield return new WaitForSeconds(duration - fadeDuration);
        
        // Fade back out.
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

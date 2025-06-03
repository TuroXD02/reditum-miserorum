using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Korah : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private float boostInterval = 5f;
    [SerializeField] private float minBoostPercent = 0.05f;
    [SerializeField] private float maxBoostPercent = 0.10f;
    [SerializeField] private float effectDuration = 5f;

    [Header("Visual Link Settings")]
    [SerializeField] private GameObject linkPrefab;
    [SerializeField] private float linkLifetime = 0.6f;
    [SerializeField] private float beamThickness = 3f;

    [Header("Buff Effect Settings")]
    [SerializeField] private GameObject buffEffectPrefab;
    [SerializeField] private float visualEffectDuration = 2f;

    [Header("Tint Settings")]
    [SerializeField] private Color targetTint = new Color(1f, 0.7f, 0.7f, 1f);
    [SerializeField] private float tintLerpDuration = 0.5f;

    private void Start()
    {
        StartCoroutine(BoostEnemySpeed());
    }

    private IEnumerator BoostEnemySpeed()
    {
        while (true)
        {
            yield return new WaitForSeconds(boostInterval);

            List<EnemyMovement> allEnemies = new List<EnemyMovement>(FindObjectsOfType<EnemyMovement>());
            allEnemies.RemoveAll(e => e == null || e.gameObject == this.gameObject);

            allEnemies.Sort((a, b) =>
                (transform.position - a.transform.position).sqrMagnitude
                .CompareTo((transform.position - b.transform.position).sqrMagnitude)
            );

            int maxAffected = Mathf.Min(3, allEnemies.Count);
            float boostFactor = Random.Range(minBoostPercent, maxBoostPercent);

            Vector3 originCenter = GetSpriteCenter(transform);

            for (int i = 0; i < maxAffected; i++)
            {
                EnemyMovement enemy = allEnemies[i];
                if (enemy == null) continue;

                if (enemy.moveSpeed < enemy.BaseSpeed)
                {
                    enemy.moveSpeed = enemy.BaseSpeed;
                }

                enemy.moveSpeed *= (1f + boostFactor);
                StartCoroutine(ResetEnemySpeed(enemy, effectDuration));

                if (buffEffectPrefab != null)
                {
                    GameObject effect = Instantiate(buffEffectPrefab, enemy.transform.position, Quaternion.identity, enemy.transform);
                    Destroy(effect, visualEffectDuration);
                }

                SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    StartCoroutine(ApplyTemporaryColor(sr, targetTint, effectDuration, tintLerpDuration));
                }

                if (linkPrefab != null)
                {
                    GameObject beam = Instantiate(linkPrefab);
                    StartCoroutine(AnimateLink(beam, transform, enemy.transform, linkLifetime));
                    Debug.Log($"Beam position: {beam.transform.position}");
                    Debug.Log($"Beam scale: {beam.transform.localScale}");


                }
            }
        }
    }

    private Vector3 GetSpriteCenter(Transform objTransform)
    {
        SpriteRenderer sr = objTransform.GetComponent<SpriteRenderer>();
        if (sr != null)
            return sr.bounds.center;

        BoxCollider2D col = objTransform.GetComponent<BoxCollider2D>();
        if (col != null)
            return col.bounds.center;

        return objTransform.position;
    }


    private IEnumerator AnimateLink(GameObject beam, Transform source, Transform target, float duration)
    {
        if (beam == null || source == null || target == null) yield break;

        SpriteRenderer sr = beam.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = beam.GetComponentInChildren<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogError("No SpriteRenderer found in beam or its children");
                yield break;
            }
        }

        float elapsed = 0f;
        float scaleMultiplier = 4f; // Adjust this value to increase the final size

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            Vector3 start = GetSpriteCenter(source);
            Vector3 end = GetSpriteCenter(target);
            Vector3 dir = end - start;
            float distance = dir.magnitude;

            // Position the beam at the midpoint
            beam.transform.position = start + (dir * 0.5f);

            // Rotate to point from start to end
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            beam.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Scale to match distance, applying the scale multiplier
            float widthScale = distance * scaleMultiplier; // Removed baseWidth
            beam.transform.localScale = new Vector3(widthScale, beamThickness, 1f);

            yield return null;
        }

        // Fade out
        float fadeDuration = 0.2f;
        float fadeElapsed = 0f;
        Color startColor = sr.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            sr.color = Color.Lerp(startColor, endColor, fadeElapsed / fadeDuration);
            yield return null;
        }

        Destroy(beam);
    }












    private IEnumerator ResetEnemySpeed(EnemyMovement enemy, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (enemy != null)
        {
            enemy.ResetSpeed();
        }
    }

    private IEnumerator ApplyTemporaryColor(SpriteRenderer renderer, Color newColor, float duration, float fadeDuration)
    {
        if (renderer == null) yield break;

        Color originalColor = renderer.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (renderer == null) yield break;
            renderer.color = Color.Lerp(originalColor, newColor, t / fadeDuration);
            yield return null;
        }

        if (renderer != null) renderer.color = newColor;
        yield return new WaitForSeconds(duration - fadeDuration);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            if (renderer == null) yield break;
            renderer.color = Color.Lerp(newColor, originalColor, t / fadeDuration);
            yield return null;
        }

        if (renderer != null) renderer.color = originalColor;
    }
}

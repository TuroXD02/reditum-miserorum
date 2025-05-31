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
    [SerializeField] private GameObject linkPrefab; // Prefab with SpriteRenderer only
    [SerializeField] private float linkAnimationDuration = 0.5f;

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

            int maxAffected = Mathf.Min(4, allEnemies.Count);
            float boostFactor = Random.Range(minBoostPercent, maxBoostPercent);

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

                // Stretching link from Korah to enemy
                if (linkPrefab != null)
                {
                    GameObject beam = Instantiate(linkPrefab, transform.position, Quaternion.identity);
                    StartCoroutine(AnimateLink(beam, transform.position, enemy.transform));
                }
            }

            // Optional: link between two enemies
            if (allEnemies.Count >= 2)
            {
                ConnectEnemies(allEnemies[0].transform, allEnemies[1].transform);
            }
        }
    }

    private void ConnectEnemies(Transform fromEnemy, Transform toEnemy)
    {
        if (linkPrefab == null || fromEnemy == null || toEnemy == null) return;

        GameObject beam = Instantiate(linkPrefab, fromEnemy.position, Quaternion.identity);
        StartCoroutine(AnimateLink(beam, fromEnemy.position, toEnemy));
    }

    private IEnumerator AnimateLink(GameObject beam, Vector3 origin, Transform target)
    {
        if (beam == null || target == null) yield break;

        Transform beamTransform = beam.transform;
        SpriteRenderer sr = beam.GetComponent<SpriteRenderer>();

        beamTransform.localScale = new Vector3(0f, 1f, 1f);

        float growDuration = linkAnimationDuration;
        float elapsed = 0f;

        while (elapsed < growDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / growDuration;

            if (target == null || beam == null) yield break;

            Vector3 targetPos = target.position;
            Vector3 dir = targetPos - origin;
            float distance = dir.magnitude;
            Vector3 direction = dir.normalized;

            // Calculate rotation
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            beamTransform.rotation = Quaternion.Euler(0, 0, angle);

            // Scale beam along X axis
            float length = Mathf.Lerp(0f, distance, t);
            beamTransform.localScale = new Vector3(length, 1f, 1f);

            // Offset beam so it stretches from origin to target
            beamTransform.position = origin + direction * (length / 2f);

            yield return null;
        }

        // Optional fade-out logic (same as before)
        yield return new WaitForSeconds(0.1f);

        if (sr != null)
        {
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

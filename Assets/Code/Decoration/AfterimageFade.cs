using UnityEngine;

public class AfterimageFade : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.3f;

    private SpriteRenderer sr;
    private Color originalColor;
    private float timer;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
        }
        else
        {
            Debug.LogWarning("AfterimageFade: No SpriteRenderer found.");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / fadeDuration;

        // Fade alpha
        float alpha = Mathf.Lerp(1f, 0f, progress);
        if (sr != null)
        {
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        }

        // Shrink scale
        float scale = Mathf.Lerp(1f, 0f, progress);
        transform.localScale = new Vector3(scale, scale, 1f);

        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }

}
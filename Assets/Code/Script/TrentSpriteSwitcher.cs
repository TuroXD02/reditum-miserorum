using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrentSpriteSwitcher : MonoBehaviour
{
    [Header("Sprite Switching Settings")]
    [SerializeField, Tooltip("Threshold for first sprite switch as a fraction of full health (e.g., 0.66 means below 66% health).")]
    private float threshold1 = 0.66f;
    [SerializeField, Tooltip("Threshold for second sprite switch as a fraction of full health (e.g., 0.33 means below 33% health).")]
    private float threshold2 = 0.33f;
    [SerializeField, Tooltip("Array of sprites to use when health drops. Element 0 is used when HP falls below threshold1, and element 1 when below threshold2.")]
    private Sprite[] spriteArray;

    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;
    private int fullHealth;
    private Sprite originalSprite;

    private void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogError($"{gameObject.name}: TrentSpriteSwitcher requires an EnemyHealth component!");
            return;
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: TrentSpriteSwitcher requires a SpriteRenderer component!");
            return;
        }
        fullHealth = enemyHealth.hitPoints; // Assume initial hitPoints represent full health.
        originalSprite = spriteRenderer.sprite;
    }

    private void Update()
    {
        if (enemyHealth == null) return;
        float currentFraction = (float)enemyHealth.hitPoints / fullHealth;

        // If health is below the second threshold, use the second sprite.
        if (currentFraction <= threshold2 && spriteArray != null && spriteArray.Length > 1)
        {
            if (spriteRenderer.sprite != spriteArray[1])
            {
                spriteRenderer.sprite = spriteArray[1];
                Debug.Log($"{gameObject.name} switched to sprite for threshold2.");
            }
        }
        // Else if health is below the first threshold, use the first sprite.
        else if (currentFraction <= threshold1 && spriteArray != null && spriteArray.Length > 0)
        {
            if (spriteRenderer.sprite != spriteArray[0])
            {
                spriteRenderer.sprite = spriteArray[0];
                Debug.Log($"{gameObject.name} switched to sprite for threshold1.");
            }
        }
        else
        {
            // Health above the first threshold: revert to the original sprite.
            if (spriteRenderer.sprite != originalSprite)
            {
                spriteRenderer.sprite = originalSprite;
                Debug.Log($"{gameObject.name} reverted to original sprite.");
            }
        }
    }
}

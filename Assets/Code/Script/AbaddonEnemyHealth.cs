using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AbaddonEnemyHealth : EnemyHealth
{
    [Header("Abaddon Speed Reduction Settings")]
    [SerializeField, Tooltip("Percentage of current speed to reduce each time Abaddon takes damage (e.g., 0.05 = 5%).")]
    private float speedReductionPercentage = 0.05f;

    [Header("Sprite Switching Settings")]
    [SerializeField, Tooltip("Sprites to cycle through each time a set number of hits are received.")]
    private Sprite[] hitSprites;
    [SerializeField, Tooltip("Number of hits required to switch sprite.")]
    private int hitsPerSpriteSwitch = 20;
    private int totalHitCount = 0;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    private EnemyMovement enemyMovement;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;

    private void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        if (enemyMovement == null)
        {
            Debug.LogError($"{gameObject.name}: Missing EnemyMovement component!");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: Missing SpriteRenderer component!");
        }

        // Setup AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = audioMixerGroup;
    }

    public override void TakeDamage(int dmg)
    {
        if (isDestroyed) return;

        // Play damage sound
        PlayDamageSound();

        // Reduce speed
        if (enemyMovement != null)
        {
            float oldSpeed = enemyMovement.moveSpeed;
            float newSpeed = oldSpeed * (1f - speedReductionPercentage);
            enemyMovement.UpdateSpeed(newSpeed);
        }

        // Handle sprite switching
        totalHitCount++;
        CheckAndSwitchSprite();

        // Apply damage
        base.TakeDamage(dmg);
    }

    public override void TakeDamageDOT(int dmg)
    {
        if (isDestroyed) return;

        // Play damage sound
        PlayDamageSound();

        // Reduce speed
        if (enemyMovement != null)
        {
            float oldSpeed = enemyMovement.moveSpeed;
            float newSpeed = oldSpeed * (1f - speedReductionPercentage);
            enemyMovement.UpdateSpeed(newSpeed);
            Debug.Log($"{gameObject.name} speed reduced from {oldSpeed} to {newSpeed} due to DOT damage.");
        }

        // Handle sprite switching
        totalHitCount++;
        CheckAndSwitchSprite();

        // Apply DOT damage
        base.TakeDamageDOT(dmg);
    }

    private void CheckAndSwitchSprite()
    {
        if (hitSprites != null && hitSprites.Length > 0 && totalHitCount % hitsPerSpriteSwitch == 0)
        {
            int spriteIndex = ((totalHitCount / hitsPerSpriteSwitch) - 1) % hitSprites.Length;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = hitSprites[spriteIndex];
                Debug.Log($"{gameObject.name} switched sprite to index {spriteIndex} after {totalHitCount} hits.");
            }
        }
    }

    private void PlayDamageSound()
    {
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }
}

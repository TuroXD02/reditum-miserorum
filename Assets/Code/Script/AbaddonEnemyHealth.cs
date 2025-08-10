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

    public override bool TakeDamage(int dmg, Turret damageSource = null)
    {
        if (isDestroyed) return false;

        PlayDamageSound();
        ApplySpeedReduction();
        totalHitCount++;
        CheckAndSwitchSprite();

        return base.TakeDamage(dmg, damageSource);
    }

    public override void TakeDamageDOT(int dmg, Turret damageSource = null)
    {
        if (isDestroyed) return;

        PlayDamageSound();
        ApplySpeedReduction();
        totalHitCount++;
        CheckAndSwitchSprite();

        base.TakeDamageDOT(dmg, damageSource);
    }

    private void ApplySpeedReduction()
    {
        if (enemyMovement != null)
        {
            float oldSpeed = enemyMovement.moveSpeed;
            float newSpeed = oldSpeed * (1f - speedReductionPercentage);
            enemyMovement.UpdateSpeed(newSpeed);
        }
    }

    private void CheckAndSwitchSprite()
    {
        if (hitSprites != null && hitSprites.Length > 0 && totalHitCount % hitsPerSpriteSwitch == 0)
        {
            int spriteIndex = ((totalHitCount / hitsPerSpriteSwitch) - 1) % hitSprites.Length;
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = hitSprites[spriteIndex];
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

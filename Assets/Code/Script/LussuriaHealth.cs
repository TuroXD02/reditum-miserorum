using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LussuriaHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private int hitPoints;
    [SerializeField] private int currencyWorth;
    [SerializeField] private float baseArmor = 100f;
    [SerializeField] private float armor;

    [Header("Armor Sprite Settings")]
    [SerializeField] private Sprite highEffectiveArmorSprite;
    [SerializeField] private Sprite lowEffectiveArmorSprite;
    [SerializeField] private Sprite armorZeroSprite;

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip damageSoundLowArmor;
    [SerializeField] private AudioClip noArmor;
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup audioMixerGroup;

    [Header("Death Prefab")]
    [SerializeField] private GameObject deathPrefab;

    public bool IsDestroyed => isDestroyed;
    public int HitPoints => hitPoints;

    private bool isDestroyed = false;
    private float previousEffectiveArmor;
    private EnemyMovement enemyMovement;
    private SpriteRenderer sr;
    private Sprite originalSprite;
    private AudioSource audioSource;

    private void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        sr = GetComponent<SpriteRenderer>();
        originalSprite = sr != null ? sr.sprite : null;
        armor = baseArmor;
        previousEffectiveArmor = armor;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (audioMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = audioMixerGroup;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: AudioMixerGroup not assigned in Inspector.");
        }
    }

    private void Update()
    {
        UpdateArmorBasedOnSpeed();
    }

    private void UpdateArmorBasedOnSpeed()
    {
        if (enemyMovement == null) return;

        float speedRatio = enemyMovement.BaseSpeed > 0 ? enemyMovement.moveSpeed / enemyMovement.BaseSpeed : 0f;
        float effectiveArmor = Mathf.Clamp(baseArmor * speedRatio, 0, baseArmor);

        if (!Mathf.Approximately(effectiveArmor, previousEffectiveArmor))
        {
            bool armorIncreased = effectiveArmor > previousEffectiveArmor;
            LevelManager.main?.PlayArmorChangeEffect(transform, armorIncreased, !armorIncreased && effectiveArmor == 0);
        }

        armor = previousEffectiveArmor = effectiveArmor;
        CheckArmorSprite();
    }

    private void CheckArmorSprite()
    {
        if (sr == null) return;

        if (armor <= 0 && armorZeroSprite != null)
        {
            sr.sprite = armorZeroSprite;
        }
        else if (armor <= 50 && lowEffectiveArmorSprite != null)
        {
            sr.sprite = lowEffectiveArmorSprite;
        }
        else if (highEffectiveArmorSprite != null)
        {
            sr.sprite = highEffectiveArmorSprite;
        }
        else if (originalSprite != null)
        {
            sr.sprite = originalSprite;
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        if (isDestroyed) return;

        PlayDamageSound();

        int finalDamage = Mathf.CeilToInt(dmg * (1f - armor / 100f));
        hitPoints -= finalDamage;

        CheckArmorSprite();

        if (hitPoints <= 0)
        {
            Kill();
        }
    }

    public virtual void TakeDamageDOTLU(int dmg)
    {
        if (isDestroyed) return;

        PlayDamageSound();

        hitPoints -= dmg;

        CheckArmorSprite();

        if (hitPoints <= 0)
        {
            Kill();
        }
    }

    public virtual void ReduceArmour(int amount)
    {
        if (baseArmor <= 0) return;

        baseArmor = Mathf.Max(baseArmor - amount, 0);
        UpdateArmorBasedOnSpeed();
    }

    private void Kill()
    {
        if (isDestroyed) return;

        isDestroyed = true;

        EnemySpawner.onEnemyDestroy?.Invoke();
        LevelManager.main?.IncreaseCurrency(currencyWorth);

        if (deathPrefab != null)
        {
            Instantiate(deathPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    private void PlayDamageSound()
    {
        if (audioSource == null) return;

        if (armor > 96f && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
        else if (armor <= 96f && armor > 50f && damageSoundLowArmor != null)
        {
            audioSource.PlayOneShot(damageSoundLowArmor);
        }
        else if (armor <= 50f && noArmor != null)
        {
            audioSource.PlayOneShot(noArmor);
        }
    }
}

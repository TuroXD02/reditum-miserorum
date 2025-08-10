using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnemyHealth : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] public int hitPoints;
    [SerializeField] private int currencyWorth;
    [SerializeField] private float armor;

    [Header("Health Sprite Settings")]
    [SerializeField] private Sprite[] healthSprites;

    [Header("Audio")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    [Header("Death Prefab")]
    [SerializeField] private GameObject deathPrefab;

    protected bool isDestroyed = false;
    protected int fullHealth;
    private SpriteRenderer sr;
    private Sprite originalSprite;
    private AudioSource audioSource;

    // Reference to turret that last dealt damage
    protected Turret lastDamageSource;

    public virtual int HitPoints => hitPoints;
    public bool IsDestroyed => isDestroyed;

    protected virtual void Start()
    {
        fullHealth = hitPoints;
        sr = GetComponent<SpriteRenderer>();
        originalSprite = sr != null ? sr.sprite : null;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (audioMixerGroup == null)
        {
            AudioMixer mixer = Resources.Load<AudioMixer>("Audio/MainMixer");
            if (mixer != null)
            {
                AudioMixerGroup[] groups = mixer.FindMatchingGroups("SFX");
                if (groups.Length > 0)
                {
                    audioMixerGroup = groups[0];
                }
            }
        }

        audioSource.outputAudioMixerGroup = audioMixerGroup;
    }

    /// <summary>
    /// Overload for turret-damage tracking. Returns true if enemy was killed.
    /// </summary>
    public virtual bool TakeDamage(int dmg, Turret damageSource = null)
    {
        if (isDestroyed) return false;

        PlayDamageSound();

        float damageMultiplier = 1f - (armor / 100f);
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);

        hitPoints -= finalDamage;

        // Only update lastDamageSource if a turret is provided.
        if (damageSource != null)
        {
            lastDamageSource = damageSource;
            damageSource.RecordDamage(finalDamage);
            Debug.Log($"[EnemyHealth] {name} took {finalDamage} dmg from turret '{damageSource.name}'. Remaining HP={hitPoints}");
        }
        else
        {
            Debug.Log($"[EnemyHealth] {name} took {finalDamage} dmg from NON-TURRET source. Remaining HP={hitPoints}");
        }

        CheckHealthSprite();

        if (hitPoints <= 0)
        {
            EnemyDestroyed();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Base version of TakeDamage. Can be overridden by subclasses.
    /// NOTE: this overload does NOT change lastDamageSource.
    /// </summary>
    public virtual void TakeDamage(int dmg)
    {
        if (isDestroyed) return;

        PlayDamageSound();

        float damageMultiplier = 1f - (armor / 100f);
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);
        hitPoints -= finalDamage;

        Debug.Log($"[EnemyHealth] {name} took {finalDamage} dmg (no source). Remaining HP={hitPoints}");

        CheckHealthSprite();

        if (hitPoints <= 0)
        {
            EnemyDestroyed();
        }
    }

    /// <summary>
    /// DOT overload now accepts optional Turret owner so we can credit damage/kills properly.
    /// </summary>
    public virtual void TakeDamageDOT(int dmg, Turret damageSource = null)
    {
        if (isDestroyed) return;

        PlayDamageSound();

        hitPoints -= dmg;

        if (damageSource != null)
        {
            lastDamageSource = damageSource;
            damageSource.RecordDamage(dmg);
            Debug.Log($"[EnemyHealth DOT] {name} took {dmg} DOT from turret '{damageSource.name}'. Remaining HP={hitPoints}");
        }
        else
        {
            Debug.Log($"[EnemyHealth DOT] {name} took {dmg} DOT (no turret source). Remaining HP={hitPoints}");
        }

        CheckHealthSprite();

        if (hitPoints <= 0)
        {
            EnemyDestroyed();
        }
    }

    public virtual void ReduceArmour(int amount)
    {
        if (armor <= 0) return;
        armor = Mathf.Max(armor - amount, 0);
        LevelManager.main?.PlayArmorChangeEffect(transform, false, armor == 0);
    }

    protected virtual void EnemyDestroyed()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        // Let the turret know it got a kill
        if (lastDamageSource != null)
        {
            Debug.Log($"[EnemyHealth] {name} destroyed. Crediting kill to '{lastDamageSource.name}'.");
            lastDamageSource.RecordKill();
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {name} destroyed but lastDamageSource is null (killed by environment or non-turret).");
        }

        EnemySpawner.onEnemyDestroy?.Invoke();
        LevelManager.main?.IncreaseCurrency(currencyWorth);

        if (deathPrefab != null)
        {
            Instantiate(deathPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    protected virtual void CheckHealthSprite()
    {
        if (sr == null || fullHealth == 0) return;

        float fraction = (float)hitPoints / fullHealth;

        if (fraction > 0.7f)
            sr.sprite = originalSprite;
        else if (fraction > 0.4f && healthSprites.Length >= 1)
            sr.sprite = healthSprites[0];
        else if (fraction > 0.1f && healthSprites.Length >= 2)
            sr.sprite = healthSprites[1];
        else if (healthSprites.Length >= 3)
            sr.sprite = healthSprites[2];
    }

    private void PlayDamageSound()
    {
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }
}

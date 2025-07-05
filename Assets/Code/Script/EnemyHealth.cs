using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    [Header("Death Prefab")]
    [SerializeField] private GameObject deathPrefab;

    protected bool isDestroyed = false;
    protected int fullHealth;
    private SpriteRenderer sr;
    private Sprite originalSprite;

    private AudioSource audioSource;

    public virtual int HitPoints => hitPoints;
    public bool IsDestroyed => isDestroyed;

    protected virtual void Start()
    {
        fullHealth = hitPoints;
        sr = GetComponent<SpriteRenderer>();
        originalSprite = sr != null ? sr.sprite : null;

        // Setup AudioSource for playing sounds
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    public virtual void TakeDamage(int dmg)
    {
        if (isDestroyed) return;

        PlayDamageSound();

        float damageMultiplier = 1f - (armor / 100f);
        int finalDamage = Mathf.CeilToInt(dmg * damageMultiplier);
        hitPoints -= finalDamage;
        CheckHealthSprite();

        if (hitPoints <= 0) EnemyDestroyed();
    }

    public virtual void TakeDamageDOT(int dmg)
    {
        if (isDestroyed) return;

        PlayDamageSound();

        hitPoints -= dmg;
        CheckHealthSprite();

        if (hitPoints <= 0) EnemyDestroyed();
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

        EnemySpawner.onEnemyDestroy.Invoke();
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
        if (fraction > 0.7f) sr.sprite = originalSprite;
        else if (fraction > 0.4f && healthSprites.Length >= 1) sr.sprite = healthSprites[0];
        else if (fraction > 0.1f && healthSprites.Length >= 2) sr.sprite = healthSprites[1];
        else if (healthSprites.Length >= 3) sr.sprite = healthSprites[2];
    }

    private void PlayDamageSound()
    {
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }


}

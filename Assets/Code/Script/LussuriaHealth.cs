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

    public bool IsDestroyed => isDestroyed;
    private bool isDestroyed = false;
    private float previousEffectiveArmor;
    private EnemyMovement enemyMovement;
    public int HitPoints => hitPoints;

    [Header("Armor Sprite Settings")]
    [SerializeField] private Sprite highEffectiveArmorSprite;
    [SerializeField] private Sprite lowEffectiveArmorSprite;
    [SerializeField] private Sprite armorZeroSprite;

    private SpriteRenderer sr;
    private Sprite originalSprite;

    private void Start()
    {
        enemyMovement = GetComponent<EnemyMovement>();
        sr = GetComponent<SpriteRenderer>();
        originalSprite = sr != null ? sr.sprite : null;
        armor = baseArmor;
        previousEffectiveArmor = armor;
    }

    private void Update() => UpdateArmorBasedOnSpeed();

    private void UpdateArmorBasedOnSpeed()
    {
        if (enemyMovement == null) return;
        float speedRatio = enemyMovement.BaseSpeed > 0 ? enemyMovement.moveSpeed / enemyMovement.BaseSpeed : 0f;
        float effectiveArmor = Mathf.Clamp(baseArmor * speedRatio, 0, baseArmor);

        if (effectiveArmor != previousEffectiveArmor)
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
        if (armor <= 0 && armorZeroSprite != null) sr.sprite = armorZeroSprite;
        else if (armor <= 50 && lowEffectiveArmorSprite != null) sr.sprite = lowEffectiveArmorSprite;
        else if (highEffectiveArmorSprite != null) sr.sprite = highEffectiveArmorSprite;
        else sr.sprite = originalSprite;
    }

    public virtual void TakeDamage(int dmg)
    {
        int finalDamage = Mathf.CeilToInt(dmg * (1f - armor / 100f));
        hitPoints -= finalDamage;
        CheckArmorSprite();
        if (hitPoints <= 0) Kill();
    }

    public virtual void TakeDamageDOTLU(int dmg)
    {
        if (isDestroyed) return;
        hitPoints -= dmg;
        CheckArmorSprite();
        if (hitPoints <= 0) Kill();
    }

    public virtual void ReduceArmour(int amount)
    {
        if (baseArmor <= 0) return;
        baseArmor = Mathf.Max(baseArmor - amount, 0);
        UpdateArmorBasedOnSpeed();
    }

    private void Kill()
    {
        isDestroyed = true;
        EnemySpawner.onEnemyDestroy.Invoke();
        LevelManager.main?.IncreaseCurrency(currencyWorth);
        Destroy(gameObject);
    }
}


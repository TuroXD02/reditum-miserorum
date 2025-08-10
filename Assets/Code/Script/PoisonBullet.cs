using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class PoisonBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D bulletCollider;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 8f;

    [Header("Poison Damage Settings")]
    [SerializeField] private float poisonDuration = 3f;
    [SerializeField] private float poisonTickInterval = 1f;

    private Transform target;
    private bool hasHitTarget = false;
    private int poisonDamagePerTick;

    [Header("VFX/SFX")]
    [SerializeField] private GameObject poisonVfxPrefab;
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    // owner turret (base type) so any turret subclass can be passed in
    private Turret ownerTurret;

    public void SetTarget(Transform _target) => target = _target;
    public void SetDamage(int damage) => poisonDamagePerTick = damage;
    public void SetOwner(Turret owner) => ownerTurret = owner;

    private void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (bulletCollider == null) bulletCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!hasHitTarget)
        {
            transform.Rotate(0f, 0f, -720f * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (target == null || hasHitTarget) return;

        Vector2 direction = (target.position - transform.position).normalized;
        if (rb != null)
            rb.velocity = direction * bulletSpeed;

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            OnHitTarget();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform == target) OnHitTarget();
    }

    private void OnHitTarget()
    {
        if (hasHitTarget) return;
        hasHitTarget = true;

        if (rb != null)
            rb.velocity = Vector2.zero;

        PlayHitSound();

        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (target.TryGetComponent(out EnemyHealth enemy))
        {
            // Pass ownerTurret into the DOT so enemy can record damage/kills to the turret.
            StartCoroutine(ApplyPoisonDamageOverTime(enemy));
            ApplyPoisonVisualEffects(target);
        }
        else if (target.TryGetComponent(out LussuriaHealth boss))
        {
            StartCoroutine(ApplyPoisonDamageOverTimeLussuria(boss));
            ApplyPoisonVisualEffects(target);
        }

        HideBullet();
        Destroy(gameObject, poisonDuration + 0.1f);
    }

    private void PlayHitSound()
    {
        if (impactSound == null) return;
        GameObject audioObj = new GameObject("TempPoisonHitSound");
        audioObj.transform.position = transform.position;
        var source = audioObj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxMixerGroup;
        source.spatialBlend = 0f;
        source.volume = soundVolume;
        source.PlayOneShot(impactSound);
        Destroy(audioObj, impactSound.length + 0.1f);
    }

    private IEnumerator ApplyPoisonDamageOverTime(EnemyHealth enemy)
    {
        float elapsed = 0f;
        while (elapsed < poisonDuration && enemy != null && !enemy.IsDestroyed)
        {
            // Pass ownerTurret into enemy; enemy will record damage for turret.
            enemy.TakeDamageDOT(poisonDamagePerTick, ownerTurret);

            yield return new WaitForSeconds(poisonTickInterval);
            elapsed += poisonTickInterval;
        }
    }

    private IEnumerator ApplyPoisonDamageOverTimeLussuria(LussuriaHealth boss)
    {
        float elapsed = 0f;
        while (elapsed < poisonDuration && boss != null && !boss.IsDestroyed)
        {
            boss.TakeDamageDOTLU(poisonDamagePerTick, ownerTurret);
            yield return new WaitForSeconds(poisonTickInterval);
            elapsed += poisonTickInterval;
        }
    }

    private void HideBullet()
    {
        if (spriteRenderer) spriteRenderer.enabled = false;
        if (bulletCollider) bulletCollider.enabled = false;
        if (rb) rb.simulated = false;
    }

    private void ApplyPoisonVisualEffects(Transform enemyTransform)
    {
        if (poisonVfxPrefab)
        {
            GameObject vfx = Instantiate(poisonVfxPrefab, enemyTransform.position, Quaternion.identity, enemyTransform);
            Destroy(vfx, poisonDuration);
        }

        if (enemyTransform.TryGetComponent(out SpriteRenderer renderer))
        {
            var effect = enemyTransform.GetComponent<EnemyPoisonEffect>() ?? enemyTransform.gameObject.AddComponent<EnemyPoisonEffect>();
            effect.ApplyPoisonEffect(new Color(0.7f, 1f, 0.7f, 1f), 0.5f, poisonDuration);
        }
    }
}

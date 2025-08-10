using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Animator))]
public class ArmourBreakerBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float bulletDuration = 0.5f;

    private int damage;
    private int armourReduction;
    private Transform target;
    private Vector3 startPosition;

    [Header("Animation & Scaling")]
    [SerializeField] private RuntimeAnimatorController swipeAnimatorController;
    [SerializeField] private float baseBulletSize = 1f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private Animator anim;
    private SpriteRenderer sr;
    private float spriteWidth = 1f;

    private float timer = 0f;
    private bool hasHit = false;

    // Reference back to the turret so the enemy can credit the turret via TakeDamage(..., sourceTurret)
    private TurretArmourBreaker sourceTurret;

    private AudioSource audioSource;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim != null && swipeAnimatorController != null)
            anim.runtimeAnimatorController = swipeAnimatorController;

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            spriteWidth = Mathf.Max(0.0001f, sr.sprite.bounds.size.x);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
    }

    private void Start()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        startPosition = transform.position;
        timer = 0f;
        AdjustBullet();
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= bulletDuration)
        {
            HitTarget();
        }
    }

    private void AdjustBullet()
    {
        Vector3 direction = (target.position - startPosition).normalized;
        float distance = Vector2.Distance(startPosition, target.position);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + 180);

        if (spriteWidth > 0f)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = (distance / spriteWidth) * baseBulletSize;
            newScale.y = baseBulletSize;
            transform.localScale = newScale;
        }

        transform.position = startPosition + direction * (distance / 2f);
    }

    // Animation event friendly
    public void PlayImpactSound()
    {
        if (impactSound == null) return;

        GameObject tempAudio = new GameObject("TempImpactSound");
        tempAudio.transform.position = transform.position;

        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = impactSound;
        tempSource.volume = soundVolume;
        tempSource.outputAudioMixerGroup = sfxMixerGroup;
        tempSource.spatialBlend = 0f;

        tempSource.Play();
        Destroy(tempAudio, impactSound.length + 0.1f);
    }

    private void HitTarget()
    {
        if (hasHit) return;
        hasHit = true;

        if (target != null)
        {
            // Use the turret-aware overload so EnemyHealth/LussuriaHealth credit the turret internally.
            if (target.TryGetComponent(out EnemyHealth enemyHealth))
            {
                bool killed = enemyHealth.TakeDamage(damage, sourceTurret);
                enemyHealth.ReduceArmour(armourReduction);

                // Do NOT call sourceTurret.RegisterKill() here — enemy will call RecordKill internally.
                // Do NOT call sourceTurret.RegisterDamage(...) here — EnemyHealth already calls RecordDamage(...)
            }

            if (target.TryGetComponent(out LussuriaHealth bossHealth))
            {
                bool killedBoss = bossHealth.TakeDamage(damage, sourceTurret);
                bossHealth.ReduceArmour(armourReduction);
            }
        }

        // allow tiny delay so animation/sound can play if needed
        Destroy(gameObject, 0.1f);
    }

    // Public setters
    public void SetDamage(int dmg) => damage = dmg;
    public void SetArmourReduction(int reduction) => armourReduction = reduction;
    public void SetTarget(Transform targetTransform) => target = targetTransform;
    public void SetSourceTurret(TurretArmourBreaker turret) => sourceTurret = turret;
}

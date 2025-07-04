using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField] private AudioMixerGroup sfxMixerGroup; // 🔊 Route to SFX mixer

    private Animator anim;
    private SpriteRenderer sr;
    private float spriteWidth;

    private float timer = 0f;
    private bool hasHit = false;
    private AudioSource audioSource;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (anim != null && swipeAnimatorController != null)
        {
            anim.runtimeAnimatorController = swipeAnimatorController;
        }

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            spriteWidth = sr.sprite.bounds.size.x;
            if (spriteWidth <= 0f)
                Debug.LogWarning("ArmourBreakerBullet: Sprite width is invalid.");
        }
        else
        {
            Debug.LogWarning("ArmourBreakerBullet: No SpriteRenderer found.");
        }

        // Setup audio source
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

        if (spriteWidth > 0)
        {
            Vector3 newScale = transform.localScale;
            newScale.x = (distance / spriteWidth) * baseBulletSize;
            newScale.y = baseBulletSize;
            transform.localScale = newScale;
        }

        transform.position = startPosition + direction * (distance / 2);
    }

    // 👇 Call this from an Animation Event at the right frame
    public void PlayImpactSound()
    {
        if (impactSound == null || sfxMixerGroup == null) return;

        // Create a temporary GameObject at the impact position
        GameObject tempAudio = new GameObject("TempImpactSound");
        tempAudio.transform.position = transform.position;

        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = impactSound;
        tempSource.volume = soundVolume;
        tempSource.outputAudioMixerGroup = sfxMixerGroup;
        tempSource.spatialBlend = 0f; // Set to 1f if 3D audio is needed

        tempSource.Play();
        Destroy(tempAudio, impactSound.length);
    }

    private void HitTarget()
    {
        if (hasHit) return;
        hasHit = true;

        if (target != null)
        {
            // Damage to regular enemies
            if (target.TryGetComponent(out EnemyHealth enemyHealth))
            {
                enemyHealth.TakeDamage(damage);
                enemyHealth.ReduceArmour(armourReduction);
            }

            // Damage to bosses
            if (target.TryGetComponent(out LussuriaHealth bossHealth))
            {
                bossHealth.TakeDamage(damage);
                bossHealth.ReduceArmour(armourReduction);
            }
        }

        // Allow time for animation and sound to finish
        Destroy(gameObject, 0.1f);
    }

    // Public setters
    public void SetDamage(int dmg) => damage = dmg;
    public void SetArmourReduction(int reduction) => armourReduction = reduction;
    public void SetTarget(Transform targetTransform) => target = targetTransform;
}

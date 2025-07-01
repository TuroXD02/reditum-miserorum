using System.Collections;
using UnityEngine;

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

    private Animator anim;
    private SpriteRenderer sr;
    private float spriteWidth;

    private float timer = 0f;
    private bool hasHit = false;

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
            {
                Debug.LogWarning("ArmourBreakerBullet: Sprite width is invalid.");
            }
        }
        else
        {
            Debug.LogWarning("ArmourBreakerBullet: No SpriteRenderer found.");
        }
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

    private void PlayImpactSound()
    {
        if (impactSound == null) return;

        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        audioSource.PlayOneShot(impactSound, soundVolume);
    }

    private void HitTarget()
    {
        if (hasHit) return;
        hasHit = true;

        if (target != null)
        {
            // Play impact sound
            PlayImpactSound();

            // Apply to EnemyHealth
            EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                enemyHealth.ReduceArmour(armourReduction);
            }

            // Apply to LussuriaHealth
            LussuriaHealth lussuriaHealth = target.GetComponent<LussuriaHealth>();
            if (lussuriaHealth != null)
            {
                lussuriaHealth.TakeDamage(damage);
                lussuriaHealth.ReduceArmour(armourReduction);
            }
        }

        // Give time for the audio to play before destroy (optional delay)
        Destroy(gameObject, 0.05f);
    }

    // Public Setters
    public void SetDamage(int dmg) => damage = dmg;
    public void SetArmourReduction(int reduction) => armourReduction = reduction;
    public void SetTarget(Transform targetTransform) => target = targetTransform;
}

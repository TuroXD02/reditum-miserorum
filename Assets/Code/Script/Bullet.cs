using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject pivot;

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed = 10f;

    [Header("Impact Effects")]
    [SerializeField] private List<GameObject> impactEffects;
    [SerializeField] private float effectDuration = 1.5f;
    [SerializeField] private float rotationOffset = -90f;

    [Header("Impact Audio")]
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float impactVolume = 1f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private static int lastEffectIndex = -1;

    private int bulletDamage = 1;
    private Turret ownerTurret;
    public Transform target;

    public void SetDamage(int damage) => bulletDamage = damage;
    public void SetTarget(Transform _target) => target = _target;
    public void SetOwner(Turret owner) => ownerTurret = owner;

    private void Start()
    {
        Destroy(gameObject, 12f); // Bullet timeout
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, -720f * Time.deltaTime); // Spin bullet
    }

    private void FixedUpdate()
    {
        if (!target) return;

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool hitSomething = false;

        // 1. Check for normal enemies
        var enemy = collision.gameObject.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            if (ownerTurret != null)
                ownerTurret.RecordDamage(bulletDamage);

            bool wasKilled = enemy.TakeDamage(bulletDamage, ownerTurret);

            if (wasKilled && ownerTurret != null)
                ownerTurret.RecordKill();

            hitSomething = true;
        }
        else
        {
            // 2. Check for Lussuria enemy type
            var lussuria = collision.gameObject.GetComponentInParent<LussuriaHealth>();
            if (lussuria != null)
            {
                if (ownerTurret != null)
                    ownerTurret.RecordDamage(bulletDamage);

                bool wasKilled = lussuria.TakeDamage(bulletDamage, ownerTurret);

                if (wasKilled && ownerTurret != null)
                    ownerTurret.RecordKill();

                hitSomething = true;
            }
        }

        if (hitSomething)
        {
            PlayImpactEffect();
            Destroy(gameObject);
        }
    }


    private void PlayImpactEffect()
    {
        // Visual Effect
        if (impactEffects != null && impactEffects.Count > 0)
        {
            int index;
            do
            {
                index = Random.Range(0, impactEffects.Count);
            } while (impactEffects.Count > 1 && index == lastEffectIndex);

            lastEffectIndex = index;

            Vector2 bulletDir = rb.velocity.normalized;
            Vector2 oppositeDir = -bulletDir;

            float angle = Mathf.Atan2(oppositeDir.y, oppositeDir.x) * Mathf.Rad2Deg + rotationOffset;
            Quaternion rot = Quaternion.Euler(0f, 0f, angle);

            GameObject impact = Instantiate(impactEffects[index], transform.position, rot);
            Destroy(impact, effectDuration);
        }

        // Audio Effect
        if (impactSound != null && sfxMixerGroup != null)
        {
            GameObject tempAudio = new GameObject("TempImpactSound");
            tempAudio.transform.position = transform.position;

            AudioSource src = tempAudio.AddComponent<AudioSource>();
            src.clip = impactSound;
            src.outputAudioMixerGroup = sfxMixerGroup;
            src.volume = impactVolume;
            src.Play();

            Destroy(tempAudio, impactSound.length + 0.1f);
        }
    }
}

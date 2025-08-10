using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(Rigidbody2D))]
public class AreaDamageBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float bulletSpeed = 5f;
    private int damage;
    private float aoeRadius;
    private Transform target;

    [Header("Explosion Animation")]
    [SerializeField] private RuntimeAnimatorController explosionAnimatorController;
    [SerializeField] private float explosionDuration = 1f;

    [Header("Explosion Visuals")]
    [SerializeField] private Color explosionOutlineColor = Color.white;
    [SerializeField] private float explosionOutlineWidth = 0.03f;
    private const int circleSegments = 50;

    [Header("Explosion Sound")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionVolume = 1f;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Header("Lifetime")]
    [SerializeField] private float selfDestructTime = 6f;

    private bool hasExploded = false;
    private Rigidbody2D rb;

    // Source turret reference
    private TurretAreaDamage sourceTurret;

    // Visual container used for fading the outline
    private GameObject explosionVisualsObj;
    private LineRenderer explosionLR;

    // Setters
    public void SetSourceTurret(TurretAreaDamage turret) => sourceTurret = turret;
    public void SetDamage(int dmg) => damage = dmg;
    public void SetAOERadius(float radius) => aoeRadius = radius;
    public void SetTarget(Transform targetTransform) => target = targetTransform;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(SelfDestructTimer());
    }

    private void FixedUpdate()
    {
        if (hasExploded || target == null) return;
        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * bulletSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;

        if (collision.collider.GetComponent<EnemyHealth>() != null ||
            collision.collider.GetComponent<LussuriaHealth>() != null)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        rb.velocity = Vector2.zero;

        if (TryGetComponent(out SpriteRenderer sr)) sr.enabled = false;
        if (TryGetComponent(out Collider2D col2D)) col2D.enabled = false;

        // damage enemies in AoE
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
        foreach (Collider2D col in hitColliders)
        {
            if (col.TryGetComponent(out EnemyHealth enemy))
            {
                // original signature returning bool isKilled
                bool isKilled = enemy.TakeDamage(damage, sourceTurret);

                if (sourceTurret != null)
                {
                    // currently registering attempted damage; for accurate damage use an API that returns actual damage amount
                    sourceTurret.RegisterDamage(damage);
                    if (isKilled)
                        sourceTurret.RegisterKill();
                }
            }
            else if (col.TryGetComponent(out LussuriaHealth lussuria))
            {
                lussuria.TakeDamage(damage);
            }
        }

        PlayExplosionSound();
        CreateExplosionOutline();
        CreateExplosionAnimation();

        StartCoroutine(EndExplosionEffect());
    }

    private IEnumerator EndExplosionEffect()
    {
        if (explosionLR == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float waitTime = explosionDuration * 0.05f;
        yield return new WaitForSeconds(waitTime);

        float fadeDuration = Mathf.Max(0.001f, explosionDuration - waitTime);
        float elapsed = 0f;
        Color initialColor = explosionLR.startColor;

        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            Color faded = new Color(initialColor.r, initialColor.g, initialColor.b, Mathf.Lerp(initialColor.a, 0f, t));
            explosionLR.startColor = faded;
            explosionLR.endColor = faded;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator SelfDestructTimer()
    {
        yield return new WaitForSeconds(selfDestructTime);
        if (!hasExploded)
        {
            Explode();
        }
    }

    private void PlayExplosionSound()
    {
        if (explosionSound == null) return;

        GameObject tempAudioObj = new GameObject("TempExplosionSound");
        tempAudioObj.transform.position = transform.position;

        AudioSource audioSource = tempAudioObj.AddComponent<AudioSource>();
        audioSource.clip = explosionSound;
        audioSource.volume = explosionVolume;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;
        audioSource.spatialBlend = 0f;
        audioSource.Play();

        Destroy(tempAudioObj, explosionSound.length + 0.1f);
    }

    private void CreateExplosionOutline()
    {
        // Put visuals on a child object so we can keep the bullet root for logic and destroy after fade
        explosionVisualsObj = new GameObject("ExplosionOutline");
        explosionVisualsObj.transform.SetParent(transform, worldPositionStays: true);
        explosionVisualsObj.transform.position = transform.position;

        explosionLR = explosionVisualsObj.AddComponent<LineRenderer>();
        explosionLR.positionCount = circleSegments + 1;
        explosionLR.loop = true;
        explosionLR.useWorldSpace = true;
        explosionLR.widthMultiplier = explosionOutlineWidth;
        explosionLR.material = new Material(Shader.Find("Sprites/Default"));
        explosionLR.startColor = explosionOutlineColor;
        explosionLR.endColor = explosionOutlineColor;
        explosionLR.numCapVertices = 5;
        explosionLR.numCornerVertices = 5;

        float angleStep = 360f / circleSegments;
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * aoeRadius;
            float y = Mathf.Sin(angle) * aoeRadius;
            explosionLR.SetPosition(i, transform.position + new Vector3(x, y, 0f));
        }
    }

    private void CreateExplosionAnimation()
    {
        if (explosionAnimatorController == null) return;

        GameObject explosionObj = new GameObject("ExplosionAnimation");
        explosionObj.transform.parent = transform;
        explosionObj.transform.localPosition = Vector3.zero;

        var sr = explosionObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 99;

        var animator = explosionObj.AddComponent<Animator>();
        animator.runtimeAnimatorController = explosionAnimatorController;

        Destroy(explosionObj, explosionDuration);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}

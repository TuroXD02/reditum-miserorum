using System.Collections;
using UnityEngine;

public class PoisonBullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;  // Riferimento al Rigidbody2D per gestire il movimento del proiettile
    [SerializeField] private SpriteRenderer spriteRenderer; // Riferimento al SpriteRenderer per gestire l'aspetto visivo del proiettile
    [SerializeField] private Collider2D bulletCollider;    // Riferimento al Collider2D del proiettile per gestire le collisioni

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed;  // Velocita del proiettile

    [Header("Poison Damage Settings")]
    [Tooltip("Durata dell'effetto veleno sul target.")]
    [SerializeField] private float poisonDuration;   // Durata totale dell'effetto veleno
    [Tooltip("Intervallo di tempo in secondi per l'applicazione del danno da veleno.")]
    [SerializeField] private float poisonTickInterval;  // Intervallo di tempo per ogni applicazione di danno

    private Transform target;  // Riferimento al target
    private bool hasHitTarget = false;  // Flag per controllare se il proiettile ha colpito il target
    private int poisonDamagePerTick;  // Danno applicato per ogni tick di veleno
    
    // Metodo per impostare il target del proiettile
    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private void Start()
    {
        // Calcola quanti tick di veleno vengono applicati durante la durata del veleno
        int numberOfTicks = Mathf.CeilToInt(poisonDuration / poisonTickInterval);
        if (numberOfTicks == 0)
        {
            numberOfTicks = 1; // Prevenire divisione per zero
        }

        poisonDamagePerTick = 50; // Imposta il danno per ogni tick
    }

    private void Update()
    {
        if (!hasHitTarget)
        {
            // Ruota il proiettile per un effetto visivo
            transform.Rotate(0, 0, -720 * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        // Se il target non � impostato, non fa nulla
        if (!target) return;

        // Muove il proiettile verso il target se non ha ancora colpito
        if (!hasHitTarget)
        {
            Vector2 direction = (target.position - transform.position).normalized;  // Calcola la direzione verso il target
            rb.velocity = direction * bulletSpeed;  // Imposta la velocit� del proiettile

            // Se il proiettile raggiunge il target, chiama il metodo OnHitTarget
            if (Vector2.Distance(transform.position, target.position) < 0.1f)
            {
                OnHitTarget();
            }
        }
    }

    private void OnHitTarget()
    {
        // Segna che il target � stato colpito
        hasHitTarget = true;

        // Ferma il movimento del proiettile
        rb.velocity = Vector2.zero;

        // Cerca il componente EnemyHealth o LussuriaHealth sul target
        EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
        LussuriaHealth lussuriaHealth = target.GetComponent<LussuriaHealth>();

        // Applica danno da veleno nel tempo se il target ha EnemyHealth
        if (enemyHealth != null)
        {
            StartCoroutine(ApplyPoisonDamageOverTime(enemyHealth));
        }

        // Applica danno da veleno nel tempo se il target ha LussuriaHealth
        if (lussuriaHealth != null)
        {
            StartCoroutine(ApplyPoisonDamageOverTime(lussuriaHealth));
        }

        // Rende il proiettile invisibile e non collidibile
        HideBullet();

        // Distrugge il proiettile dopo che l'effetto veleno � terminato
        Destroy(gameObject, poisonDuration);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Controlla se l'oggetto con cui ha colliso � il target
        if (other.transform == target)
        {
            OnHitTarget();
        }
    }

    // Coroutine per applicare danno da veleno nel tempo a EnemyHealth
    private IEnumerator ApplyPoisonDamageOverTime(EnemyHealth enemy)
    {
        float elapsedTime = 0f;

        while (elapsedTime < poisonDuration)
        {
            enemy.TakeDamageDOT(poisonDamagePerTick);  // Applica danno per tick
            

            yield return new WaitForSeconds(poisonTickInterval);  // Aspetta per il prossimo tick
            elapsedTime += poisonTickInterval;
        }

        
    }

    // Coroutine per applicare danno da veleno nel tempo a LussuriaHealth
    private IEnumerator ApplyPoisonDamageOverTime(LussuriaHealth lussuria)
    {
        float elapsedTime = 0f;

        while (elapsedTime < poisonDuration)
        {
            lussuria.TakeDamageDOTLU(poisonDamagePerTick);  // Updated method name
            

            yield return new WaitForSeconds(poisonTickInterval);  // Wait for the next tick
            elapsedTime += poisonTickInterval;
        }
    }

    // Rende il proiettile invisibile e disabilita il suo collider
    private void HideBullet()
    {
        // Disabilita il renderer del proiettile per renderlo invisibile
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Disabilita il collider del proiettile per evitare ulteriori interazioni
        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
        }
    }

    // Metodo per impostare il danno del veleno per tick
    public void SetDamage(int poisonDamage)
    {
        poisonDamagePerTick = poisonDamage;
    }
}

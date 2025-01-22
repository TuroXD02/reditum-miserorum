using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;  // Riferimento al Rigidbody2D del proiettile per gestire il movimento
    [SerializeField] private GameObject pivot;  // Riferimento al pivot (opzionale, non utilizzato in questo codice)

    [Header("Attributes")]
    [SerializeField] private float bulletSpeed;  // Velocità del proiettile


    private int bulletDamage = 1;  // Danno inflitto dal proiettile, valore di default
    public Transform target;  // Riferimento al target che il proiettile deve colpire

    // Metodo per impostare il danno del proiettile
    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    // Metodo per impostare il target del proiettile
    public void SetTarget(Transform _target)
    {
        target = _target;
    }

    private void Update()
    {
        // Ruota il proiettile per un effetto visivo
        transform.Rotate(0, 0, -720 * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Se non è stato impostato un target, esce dal metodo
        if (!target) return;

        // Se il target è stato distrutto, distrugge il proiettile
        if (!target)
        {
            Destroy(gameObject);
            return;
        }

        // Muove il proiettile verso il target
        Vector2 direction = (target.position - transform.position).normalized;  // Calcola la direzione verso il target
        rb.velocity = direction * bulletSpeed;  // Imposta la velocità del proiettile
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // Prova a ottenere il componente EnemyHealth dall'oggetto con cui il proiettile ha colliso
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();

        // Prova a ottenere il componente LussuriaHealth dall'oggetto con cui il proiettile ha colliso
        LussuriaHealth lussuriaHealth = other.gameObject.GetComponent<LussuriaHealth>();

        // Se l'oggetto con cui ha colliso ha un componente EnemyHealth, infligge danno
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(bulletDamage);  // Infligge danno all'oggetto nemico
        }

        // Se l'oggetto con cui ha colliso ha un componente LussuriaHealth, infligge danno
        if (lussuriaHealth != null)
        {
            lussuriaHealth.TakeDamage(bulletDamage);  // Infligge danno all'oggetto Lussuria
        }

        // Distrugge il proiettile dopo la collisione
        Destroy(gameObject);
    }
}

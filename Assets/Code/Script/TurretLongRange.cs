using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TurretLongRange : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint; // Punto di rotazione della torretta
    [SerializeField] private LayerMask enemyMask; // Layer che contiene i nemici
    [SerializeField] private GameObject bulletPrefab; // Prefab del proiettile
    [SerializeField] private Transform firingPoint; // Punto da cui vengono sparati i proiettili
    [SerializeField] private GameObject upgradeUI; // Interfaccia utente per l'aggiornamento della torretta
    [SerializeField] private Button upgradeButton; // Bottone per effettuare l'aggiornamento

    [Header("Attributes")]
    [SerializeField] public float targetingRange; // Raggio di rilevamento dei nemici
    [SerializeField] private float rotationSpeed; // Velocit� di rotazione della torretta
    [SerializeField] private float bps; // Bullet per second 
    [SerializeField] public int baseUpgradeCost; // Costo base per l'aggiornamento
    [SerializeField] private int bulletDamage; // Danno base del proiettile

    private float bpsBase; // Valore base dei proiettili al secondo
    private float targetingRangeBase; // Valore base del raggio di targeting
    private int bulletDamageBase; // Danno base del proiettile per i calcoli dell'upgrade

    private Transform target; // Riferimento al target attuale
    private float timeUntilFire; // Timer per gestire la cadenza di fuoco

    private int level = 1; // Livello corrente della torretta

    private void Start()
    {
        // Salvataggio dei valori base per poter calcolare le modifiche agli upgrade
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;

        // Assegna il listener al bottone per gestire l'upgrade della torretta
        upgradeButton.onClick.AddListener(Upgrade);
    }

    private void Update()
    {
        // Se non c'� un target, cerca un nemico
        if (target == null)
        {
            FindTarget();
            return;
        }

        // Ruota la torretta verso il target
        RotateTowardsTarget();

        // Controlla se il target � ancora nel raggio della torretta
        if (!CheckTargetIsInRange())
        {
            target = null; // Se il target esce dal raggio, resetta il target
        }
        else
        {
            // Gestisci il timer per il fuoco
            timeUntilFire += Time.deltaTime;

            // Se � passato abbastanza tempo, spara un proiettile
            if (timeUntilFire >= 1f / bps)
            {
                Shoot();
                timeUntilFire = 0f; // Reset del timer
            }
        }
    }

    private void Shoot()
    {
        // Create (instantiate) a new bullet at the firing point's position.
        // This uses the bulletPrefab as a blueprint for the new bullet.
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

        // Attach the LongRangeBullet script from the new bullet object (this script controls the bullet's behavior).
        LongRangeBullet bulletScript = bulletObj.GetComponent<LongRangeBullet>();

        // Use the bulletScript (remote control) to set the bullet's target.
        bulletScript.SetTarget(target); // The target is usually the enemy.

        // Use the bulletScript to set the damage the bullet can deal.
        // This is important for handling damage calculation when the bullet hits something.
        bulletScript.SetDamage(bulletDamage);
    }

    private void FindTarget()
    {
        // Usa un CircleCast per trovare i nemici nel raggio di targeting
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)
            transform.position, 0f, enemyMask);

        // Se trova dei nemici, imposta il primo come target
        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    private bool CheckTargetIsInRange()
    {
        // Controlla se il target � ancora all'interno del raggio di targeting
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    private void RotateTowardsTarget()
    {
        // Calcola l'angolo in radianti tra la posizione della torretta e il target
        // Atan2 restituisce l'arcotangente del quoziente delle coordinate (y, x), utile per ottenere l'angolo corretto
        // tra due punti nel piano 2D. Questo fornisce l'angolo in radianti.
        float angle = Mathf.Atan2(
            target.position.y - transform.position.y, // Differenza nella coordinata y tra il target e la torretta
            target.position.x - transform.position.x  // Differenza nella coordinata x tra il target e la torretta
        ) * Mathf.Rad2Deg - 90f; // Converti l'angolo da radianti a gradi e sottrai 90� per allineare l'angolo alla direzione della torretta.

        // Crea una rotazione target in base all'angolo calcolato.
        // Quaternion.Euler crea una rotazione (Quaternion) utilizzando angoli di Eulero (x, y, z),
        // in questo caso, stiamo solo ruotando attorno all'asse z (piano 2D).
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0f, 0f, angle));

        // Ruota la torretta verso il target in modo fluido utilizzando Quaternion.RotateTowards.
        // RotateTowards prende l'orientamento corrente della torretta e la ruota gradualmente
        // verso l'orientamento desiderato (targetRotation) a una velocit� specificata.
        turretRotationPoint.rotation = Quaternion.RotateTowards(
            turretRotationPoint.rotation,  // Rotazione attuale della torretta
            targetRotation,                // Rotazione desiderata verso cui muoversi
            rotationSpeed * Time.deltaTime // Velocit� della rotazione. Time.deltaTime garantisce che la rotazione sia fluida
                                           // e proporzionale al tempo trascorso tra un frame e l'altro (indipendente dal frame rate).
        );
    }


    public void OpenUpgradeUI()
    {
        // Mostra l'interfaccia di aggiornamento
        upgradeUI.SetActive(true);
    }

    public void CloseUpgradeUI()
    {
        // Nasconde l'interfaccia di aggiornamento e resetta lo stato del mouse
        upgradeUI.SetActive(false);
        UiManager.main.SetHoveringState(false);
    }

    public void Upgrade()
    {
        // Se non hai abbastanza denaro per l'upgrade, esci dal metodo
        if (CalculateCost() > LevelManager.main.currency) return;

        // Spendi la valuta necessaria per l'upgrade
        LevelManager.main.SpendCurrency(CalculateCost());

        // Aumenta il livello della torretta
        level++;

        // Aggiorna gli attributi della torretta in base al livello
        bps = CalculateBPS();
        targetingRange = CalculateRange();
        bulletDamage = CalculateBulletDamage(); // Aggiorna il danno del proiettile

        // Chiude l'interfaccia di aggiornamento
        CloseUpgradeUI();
    }

    public int CalculateCost()
    {
        // Calcola il costo dell'upgrade basato sul livello attuale
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }

    private float CalculateBPS()
    {
        // Aumenta la frequenza di fuoco in base al livello
        return bpsBase * Mathf.Pow(level, 0.4f);
    }

    private float CalculateRange()
    {
        // Aumenta il raggio di targeting in base al livello
        return targetingRangeBase * Mathf.Pow(level, 0.1f);
    }

    private int CalculateBulletDamage()
    {
        // Aumenta il danno del proiettile in base al livello
        return Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 0.5f));
    }


}

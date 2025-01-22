using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class TurretPoison : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint; // Punto di rotazione della torretta
    [SerializeField] private LayerMask enemyMask; // Layer Mask per identificare i nemici
    [SerializeField] private GameObject poisonBulletPrefab; // Prefab del proiettile velenoso
    [SerializeField] private Transform firingPoint; // Punto da cui vengono sparati i proiettili
    [SerializeField] private GameObject upgradeUI; // UI per mostrare le opzioni di aggiornamento
    [SerializeField] private Button upgradeButton; // Bottone per confermare l'aggiornamento

    [Header("Attributes")]
    [SerializeField] public float targetingRange; // Raggio di targeting della torretta
    [SerializeField] private float rotationSpeed; // Velocit� di rotazione della torretta
    [SerializeField] private float bps; // Bullet per second, ovvero la frequenza di fuoco
    [SerializeField] public int baseUpgradeCost; // Costo base dell'aggiornamento
    [SerializeField] private int bulletDamage; // Danno del proiettile

    private float bpsBase; // Valore base del bps per l'aggiornamento
    private float targetingRangeBase; // Valore base del raggio di targeting
    private int bulletDamageBase; // Valore base del danno del proiettile
    private LineRenderer lineRenderer; // LineRenderer per disegnare il raggio di targeting
    public int circleSegments = 50; // Numero di segmenti per il cerchio del raggio di targeting

    private Transform target; // Riferimento al nemico bersaglio
    private float timeUntilFire; // Tempo fino al prossimo colpo

    private int level = 1; // Livello corrente della torretta

    private void Start()
    {
        // Salva i valori base per calcoli futuri sugli upgrade
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;

        // Assegna il listener per l'upgrade alla torretta
        upgradeButton.onClick.AddListener(Upgrade);
        
        //printmessage("set correctly");
    }

    private void Update()
    {
        // Se non c'� un target, cerca un nuovo bersaglio
        if (target == null)
        {
            FindTarget();
            return;
        }

        // Ruota la torretta verso il bersaglio
        Vector3 direction = target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        turretRotationPoint.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90)); // Regola in base all'orientamento del modello della torretta

        // Se il bersaglio � fuori dal raggio, azzera il bersaglio
        if (!CheckTargetIsInRange())
        {
            target = null;
        }
        else
        {
            // Gestisce il tempo tra un colpo e l'altro
            timeUntilFire += Time.deltaTime;

            // Se � passato abbastanza tempo dall'ultimo colpo, spara
            if (timeUntilFire >= 1f / bps)
            {
                Shoot();
                timeUntilFire = 0f;
            }
        }
    }

    // Metodo per sparare un proiettile velenoso
    private void Shoot()
    {
        // Create (instantiate) a new poison bullet at the firing point's position.
        // The poisonBulletPrefab is used as a blueprint for the new bullet.
        GameObject bulletObj = Instantiate(poisonBulletPrefab, firingPoint.position, Quaternion.identity);

        // Attach the PoisonBullet script from the new bullet object (this script controls the bullet's behavior).
        PoisonBullet bulletScript = bulletObj.GetComponent<PoisonBullet>();

        // Use the bulletScript (remote control) to set the bullet's target.
        bulletScript.SetTarget(target); // The target is usually the enemy.

        // Use the bulletScript to set the poison damage the bullet can deal.
        bulletScript.SetDamage(bulletDamage);
    }

    // Metodo per trovare il bersaglio pi� vicino all'interno del raggio di targeting
    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, (Vector2)
            transform.position, 0f, enemyMask);

        // Se trova un bersaglio, lo imposta come target
        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    // Controlla se il bersaglio � ancora entro il raggio di targeting
    private bool CheckTargetIsInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    // Mostra l'interfaccia di aggiornamento e disegna il cerchio del raggio di targeting
    public void OpenUpgradeUI()
    {
        upgradeUI.SetActive(true); // Attiva l'UI di aggiornamento
        DrawCircle(); // Disegna il cerchio del raggio di targeting
                      // Mostra il cerchio
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
        }
    }

    // Chiude l'interfaccia di aggiornamento e nasconde il cerchio del raggio di targeting
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false); // Disattiva l'UI di aggiornamento
        UiManager.main.SetHoveringState(false); // Imposta lo stato UI su "non hovering"
        lineRenderer.enabled = false; // Nasconde il cerchio del raggio di targeting
    }

    // Metodo per gestire l'upgrade della torretta
    public void Upgrade()
    {
        // Se non ci sono abbastanza risorse, non fare nulla
        if (CalculateCost() > LevelManager.main.currency) return;

        // Spendi la valuta necessaria per l'upgrade
        LevelManager.main.SpendCurrency(CalculateCost());

        level++; // Aumenta il livello della torretta

        // Calcola e aggiorna i nuovi valori di bps, targetingRange e bulletDamage
        bps = CalculateBPS();
        targetingRange = CalculateRange();
        bulletDamage = CalculateBulletDamage(); // Aggiorna il danno del proiettile

        CloseUpgradeUI(); // Chiudi l'interfaccia di upgrade

    }

    // Metodo per calcolare il costo dell'upgrade
    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }

    // Calcola la nuova frequenza di fuoco (bps) in base al livello
    private float CalculateBPS()
    {
        return bpsBase * Mathf.Pow(level, 1f);
    }

    // Calcola il nuovo raggio di targeting in base al livello
    private float CalculateRange()
    {
        return targetingRangeBase * Mathf.Pow(level, 0.55f); // Mantiene il raggio base corretto
    }

    // Calcola il nuovo danno del proiettile in base al livello
    private int CalculateBulletDamage()
    {
        return Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 2f));
    }

    // Disegna il raggio di targeting come un cerchio usando il LineRenderer
    private void DrawCircle()
    {
        float angleStep = 360f / circleSegments; // Suddivide il cerchio in segmenti

        // Calcola e imposta i punti per il cerchio
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * targetingRange;
            float y = Mathf.Sin(angle) * targetingRange;

            // Imposta la posizione del segmento del cerchio
            Vector3 position = new Vector3(x, y, 0f) + transform.position;
            
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(i, position);
            }
        }

        
    }

    //public void printmessage(string message)
    //{
    //   Debug.Log(message);
    //}

   

    // Metodo per disegnare il raggio di targeting nella modalit� Editor (Gizmos)

}

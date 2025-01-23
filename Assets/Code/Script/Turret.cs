using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class Turret : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform turretRotationPoint; // Punto di rotazione della torretta
    [SerializeField] private LayerMask enemyMask; // Layer Mask per identificare i nemici
    [SerializeField] private GameObject bulletPrefab; // Prefab del proiettile che viene sparato dalla torretta
    [SerializeField] private Transform firingPoint; // Punto da cui vengono sparati i proiettili
    [SerializeField] private GameObject upgradeUI; // UI per gli aggiornamenti della torretta
    [SerializeField] private Button upgradeButton; // Bottone per effettuare l'upgrade
    [SerializeField] private SpriteRenderer turretSpriteRenderer; // Sprite Renderer for the tower
    [SerializeField] private Sprite[] towerStates; // Array of sprites representing tower states

    [Header("Attributes")]
    [SerializeField] public float targetingRange; // Raggio entro cui la torretta può colpire i nemici
    [SerializeField] private float rotationSpeed; // Velocità di rotazione della torretta
    [SerializeField] private float bps; // Bullet per second, frequenza di fuoco
    [SerializeField] public int baseUpgradeCost; // Costo base dell'upgrade
    [SerializeField] private int bulletDamage; // Danno inflitto dai proiettili della torretta

    // Variabili private per memorizzare i valori base (utilizzati per calcoli degli upgrade)
    private float bpsBase;
    private float targetingRangeBase;
    private int bulletDamageBase;

    private Transform target; // Riferimento al bersaglio attuale della torretta
    private float timeUntilFire; // Timer per gestire il tempo tra un colpo e l'altro

    private int level = 1; // Livello corrente della torretta

    private void Start()
    {
        // Memorizza i valori base per l'upgrade
        bpsBase = bps;
        targetingRangeBase = targetingRange;
        bulletDamageBase = bulletDamage;

        // Assegna la funzione Upgrade al bottone
        upgradeButton.onClick.AddListener(Upgrade);

        // Set the initial sprite for the tower
        UpdateSprite();
    }

    private void Update()
    {
        // Se non c'è nessun bersaglio, cerca uno nuovo
        if (target == null)
        {
            FindTarget();
            return;
        }

        // Controlla se il bersaglio è ancora nel raggio di targeting
        if (!CheckTargetIsInRange())
        {
            target = null; // Resetta il bersaglio se è fuori portata
        }
        else
        {
            // Aggiorna il timer per il prossimo colpo
            timeUntilFire += Time.deltaTime;

            // Se il timer supera il tempo tra un colpo e l'altro, spara
            if (timeUntilFire >= 1f / bps)
            {
                Shoot();
                timeUntilFire = 0f; // Resetta il timer
            }
        }
    }

    // Metodo che gestisce il fuoco della torretta
    private void Shoot()
    {
        // Create (instantiate) a new generic bullet at the firing point's position.
        // This uses the bulletPrefab as a blueprint for the new bullet.
        GameObject bulletObj = Instantiate(bulletPrefab, firingPoint.position, Quaternion.identity);

        // Attach the Bullet script from the new bullet object (this script controls the bullet's behavior).
        Bullet bulletScript = bulletObj.GetComponent<Bullet>();

        // Use the bulletScript (remote control) to set the bullet's target.
        bulletScript.SetTarget(target); // The target is usually the enemy.

        // Use the bulletScript to set the damage the bullet can deal.
        bulletScript.SetDamage(bulletDamage);
    }

    // Cerca un nuovo bersaglio all'interno del raggio di targeting della torretta
    private void FindTarget()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, targetingRange, Vector2.zero, 0f, enemyMask);

        // Se ci sono nemici nel raggio, seleziona il primo come bersaglio
        if (hits.Length > 0)
        {
            target = hits[0].transform;
        }
    }

    // Controlla se il bersaglio è ancora nel raggio di targeting
    private bool CheckTargetIsInRange()
    {
        return Vector2.Distance(target.position, transform.position) <= targetingRange;
    }

    // Apre l'interfaccia di upgrade della torretta
    public void OpenUpgradeUI()
    {
        upgradeUI.SetActive(true); // Attiva l'UI per gli upgrade
    }

    // Chiude l'interfaccia di upgrade della torretta
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false); // Disattiva l'UI
        UiManager.main.SetHoveringState(false); // Aggiorna lo stato del cursore
    }

    // Metodo per gestire l'upgrade della torretta
    public void Upgrade()
    {
        // Controlla se ci sono abbastanza risorse per effettuare l'upgrade
        if (CalculateCost() > LevelManager.main.currency) return;

        // Deduce la valuta necessaria per l'upgrade
        LevelManager.main.SpendCurrency(CalculateCost());

        // Aumenta il livello della torretta
        level++;

        // Aggiorna i valori della torretta in base al nuovo livello
        bps = CalculateBPS(); // Frequenza di fuoco
        targetingRange = CalculateRange(); // Raggio di targeting
        bulletDamage = CalculateBulletDamage(); // Danno del proiettile

        // Cambia il sprite in base al livello attuale
        UpdateSprite();

        // Chiude l'interfaccia di upgrade dopo l'aggiornamento
        CloseUpgradeUI();

        // Debugging per vedere i nuovi valori aggiornati
        Debug.Log("nuovo bps:" + bps);
        Debug.Log("nuovo targetingRange:" + targetingRange);
        Debug.Log("nuovo bulletDamage:" + bulletDamage);
        Debug.Log("nuovo cost:" + CalculateCost());
    }

    // Aggiorna il sprite della torretta in base al livello
    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && level - 1 < towerStates.Length)
        {
            turretSpriteRenderer.sprite = towerStates[level - 1]; // Usa lo sprite corrispondente al livello
        }
        else
        {
            Debug.LogWarning("Sprite not updated: Check sprite array or level.");
        }
    }

    // Metodo per calcolare il costo dell'upgrade basato sul livello
    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 0.8f));
    }

    // Calcola la nuova frequenza di fuoco in base al livello
    private float CalculateBPS()
    {
        return bpsBase * Mathf.Pow(level, 0.5f);
    }

    // Calcola il nuovo raggio di targeting in base al livello
    private float CalculateRange()
    {
        return targetingRangeBase * Mathf.Pow(level, 0.1f);
    }

    // Calcola il nuovo danno del proiettile in base al livello
    private int CalculateBulletDamage()
    {
        return Mathf.RoundToInt(bulletDamageBase * Mathf.Pow(level, 0.4f));
    }
}

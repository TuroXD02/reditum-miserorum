using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;


public class TurretSlow : MonoBehaviour
{ 
    
    [Header("References")]
    [SerializeField] private LayerMask enemyMask; // Mask per identificare i nemici
    [SerializeField] private GameObject upgradeUI; // UI per gli upgrade della torretta
    [SerializeField] private Button upgradeButton; // Bottone per effettuare l'upgrade

    [Header("Attributes")]
    [SerializeField] public float targetingRange; // Raggio entro cui la torretta pu� colpire i nemici
    [SerializeField] private float aps; // Attacchi per secondo (Attacks per second)
    [SerializeField] private float freezeTime; // Durata dell'effetto di rallentamento
    [SerializeField] public int baseUpgradeCost; // Costo base dell'upgrade

    private float apsBase; // Valore di base di attacchi per secondo, usato per i calcoli degli upgrade
    private float timeUntilFire; // Timer per gestire il tempo tra un attacco e l'altro
     // Valore di base del raggio di targeting

    private int level = 1; // Livello corrente della torretta

    // LineRenderer per visualizzare il raggio di targeting (se necessario per debugging o estetica)
    private LineRenderer lineRenderer;
    public int circleSegments; // Segmenti per il cerchio del raggio di targeting

    private void Start()
    {
        // Memorizza i valori base per gli upgrade
        apsBase = aps;
        

        // Assegna la funzione di upgrade al bottone
        upgradeButton.onClick.AddListener(Upgrade);
    }

    private void Update()
    {
        // Aggiorna il timer per il prossimo attacco
        timeUntilFire += Time.deltaTime;

        // Se il timer supera il tempo tra un attacco e l'altro, applica il rallentamento (Freeze)
        if (timeUntilFire >= 1f / aps)
        {
            Freeze();
            timeUntilFire = 0f; // Resetta il timer
        }
    }

    // Funzione che gestisce l'effetto di rallentamento (Freeze) sui nemici
    private void Freeze()
    {
        // Casts a circle to find all enemies in range and slows them down
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position, // The center of the circle (the turret's position)
            targetingRange,     // The radius of the circle
            Vector2.zero,       // No movement, just detect in place
            0f,                 // The circle does not move
            enemyMask           // Only detect objects on the "enemy" layer
        );
        
// Check if any enemies were detected by the circle cast
        if (hits.Length > 0)
        {
            
            // Step 1: Iterate through each object detected by the circle cast
            for (int e = 0; e < hits.Length; e++) // Loops over all hits
            {
                // Step 2: Access the current detected object
                RaycastHit2D hit = hits[e]; // `hits` is an array; `hits[e]` gets one element at a time

                // Step 3: Try to get the "EnemyMovement" script from the detected object
                // The script is used to manage the enemy's movement speed
                EnemyMovement em = hit.transform.GetComponent<EnemyMovement>();

                // Step 4: If the detected object has an "EnemyMovement" script, proceed
                if (em != null)
                {
                    // Step 5: Apply a slow effect by reducing the enemy's speed to 0.1
                    em.UpdateSpeed(0.1f);

                    // Step 6: Start a coroutine (timer) to reset the enemy's speed after the slow effect ends
                    StartCoroutine(ResetEnemySpeed(em));
                }
                else
                {
                    // If no "EnemyMovement" script is found, log a warning
                    Debug.LogWarning("Detected object does not have an EnemyMovement script.");
                }
            }
        }

    }

    // Coroutine per ripristinare la velocit� originale del nemico dopo il tempo di freeze
    private IEnumerator ResetEnemySpeed(EnemyMovement em)
    {
        // Step 1: Pause this coroutine for the duration of the freeze effect
        yield return new WaitForSeconds(freezeTime);

        // Step 2: After the freeze duration ends, restore the enemy's original speed
        em.ResetSpeed();
    }

    // Funzione per aprire l'UI di upgrade
    public void OpenUpgradeUI()
    {
        upgradeUI.SetActive(true); // Mostra l'UI per gli upgrade
    }

    // Funzione per chiudere l'UI di upgrade
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false); // Nasconde l'UI
        UiManager.main.SetHoveringState(false); // Aggiorna lo stato del cursore
    }

    // Funzione per gestire l'upgrade della torretta
    public void Upgrade()
    {
        // Controlla se ci sono abbastanza risorse per effettuare l'upgrade
        if (CalculateCost() > LevelManager.main.currency) return;

        // Deduce la valuta necessaria per l'upgrade
        LevelManager.main.SpendCurrency(CalculateCost());

        // Aumenta il livello della torretta
        level++;

        // Aggiorna i valori della torretta in base al nuovo livello
        aps = CalculateAPS(); // Aumenta gli attacchi per secondo
        targetingRange = CalculateRange(); // Aumenta il raggio di targeting

        // Chiude l'UI di upgrade dopo l'aggiornamento
        CloseUpgradeUI();

    }

    // Funzione per calcolare il costo dell'upgrade basato sul livello
    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 2f)); // Il costo aumenta esponenzialmente con il livello
    }

    // Calcola la nuova frequenza di attacco in base al livello
    private float CalculateAPS()
    {
        return apsBase * Mathf.Pow(level, 0.32f); 
    }

    // Calcola il nuovo raggio di targeting in base al livello
    private float CalculateRange()
    {
        return targetingRange * Mathf.Pow(level, 0.3f); // Aumento del raggio di targeting con l'upgrade
    }

    // Potresti aggiungere qui una funzione per disegnare un cerchio attorno alla torretta per mostrare il raggio di targeting visivamente
    // ad esempio con un LineRenderer, se richiesto per effetti visivi o debugging.

    // Metodo pubblico per nascondere il cerchio di targeting (se il LineRenderer viene utilizzato)
}

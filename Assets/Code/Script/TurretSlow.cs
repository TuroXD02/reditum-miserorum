using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretSlow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayerMask enemyMask; // Mask to identify enemies
    [SerializeField] private GameObject upgradeUI; // UI for turret upgrades
    [SerializeField] private Button upgradeButton; // Button for triggering upgrades
    [SerializeField] private SpriteRenderer turretSpriteRenderer; // Reference to the SpriteRenderer component
    [SerializeField] private Sprite[] upgradeSprites; // Array of sprites representing turret states

    [Header("Attributes")]
    [SerializeField] public float targetingRange; // Range within which the turret can detect enemies
    [SerializeField] private float aps; // Attacks per second
    [SerializeField] private float freezeTime; // Duration of the freeze effect
    [SerializeField] public int baseUpgradeCost; // Base cost of upgrading the turret

    private float apsBase; // Base value for attacks per second, used for upgrade calculations
    private float timeUntilFire; // Timer to control the time between attacks
    private int level = 1; // Current level of the turret

    private void Start()
    {
        apsBase = aps; // Store the base APS for future upgrades
        upgradeButton.onClick.AddListener(Upgrade); // Attach the Upgrade function to the button
    }

    private void Update()
    {
        // Update the timer for the next attack
        timeUntilFire += Time.deltaTime;

        // If enough time has passed, trigger the freeze effect
        if (timeUntilFire >= 1f / aps)
        {
            Freeze();
            timeUntilFire = 0f; // Reset the timer
        }
    }

    // Function to apply the freeze effect to enemies in range
    private void Freeze()
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position, // Center of the detection circle
            targetingRange,     // Radius of the detection circle
            Vector2.zero,       // No movement, detect in place
            0f,                 // Circle does not move
            enemyMask           // Only detect objects on the "enemy" layer
        );

        // If there are enemies in range, apply the freeze effect
        if (hits.Length > 0)
        {
            for (int e = 0; e < hits.Length; e++) // Loop through all detected enemies
            {
                RaycastHit2D hit = hits[e];
                EnemyMovement em = hit.transform.GetComponent<EnemyMovement>(); // Get the enemy's movement component
                if (em != null)
                {
                    em.UpdateSpeed(0.1f); // Slow down the enemy
                    StartCoroutine(ResetEnemySpeed(em)); // Reset the speed after the freeze time
                }
            }
        }
    }

    // Coroutine to reset the enemy's speed after the freeze effect ends
    private IEnumerator ResetEnemySpeed(EnemyMovement em)
    {
        yield return new WaitForSeconds(freezeTime); // Wait for the freeze time
        em.ResetSpeed(); // Restore the enemy's original speed
    }

    // Function to open the turret upgrade UI
    public void OpenUpgradeUI()
    {
        upgradeUI.SetActive(true);
    }

    // Function to close the turret upgrade UI
    public void CloseUpgradeUI()
    {
        upgradeUI.SetActive(false);
        UiManager.main.SetHoveringState(false);
    }

    // Function to handle turret upgrades
    public void Upgrade()
    {
        // Check if there are enough resources for the upgrade
        if (CalculateCost() > LevelManager.main.currency) return;

        LevelManager.main.SpendCurrency(CalculateCost()); // Deduct the upgrade cost
        level++; // Increment the turret level
        aps = CalculateAPS(); // Update APS
        targetingRange = CalculateRange(); // Update targeting range
        UpdateSprite(); // Update the turret's sprite to reflect the upgrade
        CloseUpgradeUI(); // Close the upgrade UI
    }

    // Function to calculate the upgrade cost based on the level
    public int CalculateCost()
    {
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(level, 2f));
    }

    // Calculate the new attack speed (APS) based on the level
    private float CalculateAPS()
    {
        return apsBase * Mathf.Pow(level, 0.32f);
    }

    // Calculate the new targeting range based on the level
    private float CalculateRange()
    {
        return targetingRange * Mathf.Pow(level, 0.3f);
    }

    // Update the turret's sprite to reflect the current level
    private void UpdateSprite()
    {
        if (turretSpriteRenderer != null && upgradeSprites != null && level - 1 < upgradeSprites.Length)
        {
            turretSpriteRenderer.sprite = upgradeSprites[level - 1];
        }
        else
        {
            Debug.LogWarning("Sprite or sprite array is missing!");
        }
    }
}

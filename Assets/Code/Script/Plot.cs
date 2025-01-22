using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    private Color startColor;
    private GameObject towerObj;

    private void Start()
    {
        // Save the starting color of the sprite renderer
        startColor = sr.color;
    }

    private void OnMouseEnter()
    {
        // Change color to hover color when the mouse enters
        sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        // Revert to the starting color when the mouse exits
        sr.color = startColor;
    }

    private void OnMouseDown() // aka click mouse
    {
        if (UiManager.main.IsHoveringUI()) return; // Prevent interaction if the UI is active

        // Check if there is already a tower on this plot
        if (towerObj != null)
        {
            Debug.Log($"Interacting with tower: {towerObj.name}");

           

            // Switch based on the component name to open the appropriate UI
            switch (towerObj.name)
            {
                case "Maddalena(Clone)":
                    towerObj.GetComponent<TurretSlow>()?.OpenUpgradeUI();
                    break;

                case "SanPietro(Clone)":
                    towerObj.GetComponent<Turret>()?.OpenUpgradeUI();
                    break;

                case "Davide(Clone)":
                    towerObj.GetComponent<TurretLongRange>()?.OpenUpgradeUI();
                    break;

                case "Eva(Clone)":
                    towerObj.GetComponent<TurretPoison>()?.OpenUpgradeUI();
                    break;

                default:
                    Debug.LogWarning("Unknown turret type.");
                    break;
            }

            return;
        }

        

        // Get the tower to build or use the last selected tower
        Tower towerToBuild = BuildManager.main.GetSelectedTower();

        // If no tower is selected to build, exit
        if (towerToBuild == null)
        {
            Debug.LogWarning("No tower selected to build.");
            return;
        }

        // Check if the player has enough currency
        if (LevelManager.main.SpendCurrency(towerToBuild.cost))
        {
            // Instantiate the new tower and set it to the towerObj reference
            towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);
            
        }
        
    }
}

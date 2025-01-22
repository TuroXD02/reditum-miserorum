using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This script manages the upgrade UI for turrets in the game.
public class UpgradeUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Singleton reference for global access to this script
    public static UpgradeUIHandler main;

    // UI elements for displaying turret upgrade information
    [SerializeField] private TextMeshProUGUI costUI; 
    [SerializeField] private Button sellButton;

    // Tracks whether the mouse is hovering over the upgrade UI
    public bool mouse_over = false;

    // References to different turret types (only one is active at a time)
    public Turret turretInstance;
    public TurretSlow turretSlowInstance;
    public TurretLongRange TurretLongRangeInstance;
    public TurretPoison TurretPoisonInstance;

    // LineRenderer for visualizing turret range
    private LineRenderer lineRenderer;

    // Parameters for drawing the turret's targeting circle
    public int circleSegments = 50;
    public Color circleColor = Color.cyan;

    private void Awake()
    {
        main = this; // Initialize the singleton instance for easy global access
    }

    private void Start()
    {
        // Ensure at least one turret type is assigned; log a warning if none are found
        if (turretInstance == null && turretSlowInstance == null && TurretLongRangeInstance == null && TurretPoisonInstance == null)
        {
            Debug.LogWarning("No turret instance assigned.");
        }

        // Set up the LineRenderer for drawing targeting circles
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = circleSegments + 1;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = circleColor;
        lineRenderer.endColor = circleColor;
        lineRenderer.enabled = false;

        // Attach the SellTower method to the sell button's click event
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(SellTower);
        }
        else
        {
            Debug.LogWarning("Sell button is not assigned.");
        }
    }

    private void Update()
    {
        UpdateCostUI(); // Update the displayed upgrade cost
    }

    // Updates the cost UI to reflect the currently selected turret
    private void UpdateCostUI()
    {
        if (turretInstance != null && costUI != null)
        {
            costUI.text = turretInstance.CalculateCost().ToString();
        }
        else if (turretSlowInstance != null && costUI != null)
        {
            costUI.text = turretSlowInstance.CalculateCost().ToString();
        }
        else if (TurretLongRangeInstance != null && costUI != null)
        {
            costUI.text = TurretLongRangeInstance.CalculateCost().ToString();
        }
        else if (TurretPoisonInstance != null && costUI != null)
        {
            costUI.text = TurretPoisonInstance.CalculateCost().ToString();
        }
        else
        {
            costUI.text = "No selected tower";
        }
    }

    // Triggered when the mouse enters the UI element
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true; // Indicate that the mouse is hovering over the UI
        if (UiManager.main != null)
        {
            UiManager.main.SetHoveringState(true); // Notify the UI manager
        }
        DrawCircle(); // Draw the turret's targeting range
    }

    // Triggered when the mouse exits the UI element
    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false; // Indicate that the mouse is no longer hovering
        if (UiManager.main != null)
        {
            UiManager.main.SetHoveringState(false); // Notify the UI manager
        }
        HideCircle(); // Hide the targeting circle
        gameObject.SetActive(false); // Deactivate the UI
    }

    // Sets the selected turret instance and updates the UI accordingly
    public void SetTurretInstance(Turret turret)
    {
        turretInstance = turret;
        turretSlowInstance = null;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = null;
        UpdateCostUI();
    }

    public void SetTurretSlowInstance(TurretSlow turretSlow)
    {
        turretInstance = null;
        turretSlowInstance = turretSlow;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = null;
        UpdateCostUI();
    }

    public void SetTurretLongRangeInstance(TurretLongRange TurretLongRange)
    {
        turretInstance = null;
        turretSlowInstance = null;
        TurretLongRangeInstance = TurretLongRange;
        TurretPoisonInstance = null;
        UpdateCostUI();
    }

    public void SetTurretPoison(TurretPoison TurretPoison)
    {
        turretInstance = null;
        turretSlowInstance = null;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = TurretPoison;
        UpdateCostUI();
    }

    // Sells the currently selected turret and refunds some currency
    public void SellTower()
    {
        Debug.Log("Selling the turret...");
        int refundAmount = Mathf.RoundToInt(GetTurretOriginalCost() * 0.5f);
        LevelManager.main.AddCurrency(refundAmount); // Refund half the turret's cost

        // Destroy the turret GameObject based on its type
        if (turretInstance != null)
        {
            Destroy(turretInstance.gameObject);
        }
        else if (turretSlowInstance != null)
        {
            Destroy(turretSlowInstance.gameObject);
        }
        else if (TurretLongRangeInstance != null)
        {
            Destroy(TurretLongRangeInstance.gameObject);
        }
        else if (TurretPoisonInstance != null)
        {
            Destroy(TurretPoisonInstance.gameObject);
        }

        Debug.Log("Turret sold. Currency refunded: " + refundAmount);

        // Reset mouse hover state after selling
        MenuUIHandler.main.mouse_over = false;
        UiManager.main.SetHoveringState(false);
    }

    // Retrieves the original cost of the turret
    private int GetTurretOriginalCost()
    {
        if (turretInstance != null) return turretInstance.baseUpgradeCost;
        if (turretSlowInstance != null) return turretSlowInstance.baseUpgradeCost;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.baseUpgradeCost;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.baseUpgradeCost;
        return 0; // Default cost if no turret is set
    }

    // Draws the targeting range circle around the turret
    private void DrawCircle()
    {
        float targetingRange = GetTurretTargetingRange();
        if (targetingRange <= 0) return;
        float angleStep = 360f / circleSegments;
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * targetingRange;
            float y = Mathf.Sin(angle) * targetingRange;
            Vector3 position = new Vector3(x, y, 0f) + GetTurretPosition();
            lineRenderer.SetPosition(i, position);
        }
        lineRenderer.enabled = true;
    }

    // Hides the targeting range circle
    private void HideCircle()
    {
        lineRenderer.enabled = false;
    }

    // Gets the targeting range of the current turret
    private float GetTurretTargetingRange()
    {
        if (turretInstance != null) return turretInstance.targetingRange;
        if (turretSlowInstance != null) return turretSlowInstance.targetingRange;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.targetingRange;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.targetingRange;
        return 0f; // Default range if no turret is set
    }

    // Gets the position of the current turret
    private Vector3 GetTurretPosition()
    {
        if (turretInstance != null) return turretInstance.transform.position;
        if (turretSlowInstance != null) return turretSlowInstance.transform.position;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.transform.position;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.transform.position;
        return Vector3.zero; // Default position if no turret is set
    }
}

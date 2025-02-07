using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This script manages the upgrade UI for turrets in the game.
public class UpgradeUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Singleton reference for global access to this script.
    public static UpgradeUIHandler main;

    // UI elements for displaying turret upgrade information.
    [SerializeField] private TextMeshProUGUI costUI; 
    [SerializeField] private TextMeshProUGUI sellGainUI; // Displays the sell gain amount.
    [SerializeField] private Button sellButton;

    // Tracks whether the mouse is hovering over the upgrade UI.
    public bool mouse_over = false;

    // References to different turret types (only one is active at a time).
    public Turret turretInstance;
    public TurretSlow turretSlowInstance;
    public TurretLongRange TurretLongRangeInstance;
    public TurretPoison TurretPoisonInstance;
    public TurretAreaDamage TurretAreaDamageInstance;
    public TurretArmourBreaker TurretArmourBreakerInstance; // NEW: Armour Breaker turret

    // LineRenderer for visualizing turret range.
    private LineRenderer lineRenderer;

    // Parameters for drawing the turret's targeting circle.
    public int circleSegments = 50;
    public Color circleColor = Color.cyan;

    private void Awake()
    {
        main = this; // Initialize the singleton instance.
    }

    private void Start()
    {
        // Log a warning if no turret instance is assigned.
        if (turretInstance == null && turretSlowInstance == null && TurretLongRangeInstance == null &&
            TurretPoisonInstance == null && TurretAreaDamageInstance == null && TurretArmourBreakerInstance == null)
        {
            Debug.LogWarning("No turret instance assigned.");
        }

        // Set up the LineRenderer for drawing targeting circles.
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = circleSegments + 1;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = true;
        lineRenderer.widthMultiplier = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = circleColor;
        lineRenderer.endColor = circleColor;
        lineRenderer.enabled = false;

        // Attach the SellTower method to the sell button's click event.
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
        UpdateCostAndSellGainUI(); // Update the displayed upgrade cost and sell gain.
    }

    // Updates the cost and sell gain UI to reflect the currently selected turret.
    private void UpdateCostAndSellGainUI()
    {
        int upgradeCost = 0;
        int sellGain = 0;
        
        if (turretInstance != null && costUI != null)
        {
            upgradeCost = turretInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(turretInstance.baseUpgradeCost * 0.5f);
        }
        else if (turretSlowInstance != null && costUI != null)
        {
            upgradeCost = turretSlowInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(turretSlowInstance.baseUpgradeCost * 0.5f);
        }
        else if (TurretLongRangeInstance != null && costUI != null)
        {
            upgradeCost = TurretLongRangeInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretLongRangeInstance.baseUpgradeCost * 0.5f);
        }
        else if (TurretPoisonInstance != null && costUI != null)
        {
            upgradeCost = TurretPoisonInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretPoisonInstance.baseUpgradeCost * 0.5f);
        }
        else if (TurretAreaDamageInstance != null && costUI != null)
        {
            upgradeCost = TurretAreaDamageInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretAreaDamageInstance.baseUpgradeCost * 0.5f);
        }
        else if (TurretArmourBreakerInstance != null && costUI != null)
        {
            upgradeCost = TurretArmourBreakerInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretArmourBreakerInstance.baseUpgradeCost * 0.5f);
        }
        else
        {
            costUI.text = "No selected tower";
            sellGainUI.text = "";
            return;
        }
        
        costUI.text = upgradeCost.ToString();
        sellGainUI.text = sellGain.ToString();
    }

    // Triggered when the mouse enters the UI element.
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
        if (UiManager.main != null)
        {
            UiManager.main.SetHoveringState(true);
        }
        DrawCircle();
    }

    // Triggered when the mouse exits the UI element.
    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false;
        if (UiManager.main != null)
        {
            UiManager.main.SetHoveringState(false);
        }
        HideCircle();
        gameObject.SetActive(false);
    }

    // Methods to set the selected turret instance and update the UI accordingly.
    public void SetTurretInstance(Turret turret)
    {
        turretInstance = turret;
        turretSlowInstance = null;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = null;
        TurretAreaDamageInstance = null;
        TurretArmourBreakerInstance = null;
        UpdateCostAndSellGainUI();
    }

    public void SetTurretSlowInstance(TurretSlow turretSlow)
    {
        turretInstance = null;
        turretSlowInstance = turretSlow;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = null;
        TurretAreaDamageInstance = null;
        TurretArmourBreakerInstance = null;
        UpdateCostAndSellGainUI();
    }

    public void SetTurretLongRangeInstance(TurretLongRange turretLongRange)
    {
        turretInstance = null;
        turretSlowInstance = null;
        TurretLongRangeInstance = turretLongRange;
        TurretPoisonInstance = null;
        TurretAreaDamageInstance = null;
        TurretArmourBreakerInstance = null;
        UpdateCostAndSellGainUI();
    }

    public void SetTurretPoison(TurretPoison turretPoison)
    {
        turretInstance = null;
        turretSlowInstance = null;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = turretPoison;
        TurretAreaDamageInstance = null;
        TurretArmourBreakerInstance = null;
        UpdateCostAndSellGainUI();
    }
    
    public void SetTurretAreaDamageInstance(TurretAreaDamage turretAreaDamage)
    {
        turretInstance = null;
        turretSlowInstance = null;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = null;
        TurretAreaDamageInstance = turretAreaDamage;
        TurretArmourBreakerInstance = null;
        UpdateCostAndSellGainUI();
    }
    
    public void SetTurretArmourBreakerInstance(TurretArmourBreaker turretArmourBreaker)
    {
        turretInstance = null;
        turretSlowInstance = null;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = null;
        TurretAreaDamageInstance = null;
        TurretArmourBreakerInstance = turretArmourBreaker;
        UpdateCostAndSellGainUI();
    }

    // Sells the currently selected turret and refunds some currency.
    public void SellTower()
    {
        Debug.Log("Selling the turret...");
        int refundAmount = Mathf.RoundToInt(GetTurretOriginalCost() * 0.5f);
        LevelManager.main.AddCurrency(refundAmount);
        
        if (turretInstance != null)
            Destroy(turretInstance.gameObject);
        else if (turretSlowInstance != null)
            Destroy(turretSlowInstance.gameObject);
        else if (TurretLongRangeInstance != null)
            Destroy(TurretLongRangeInstance.gameObject);
        else if (TurretPoisonInstance != null)
            Destroy(TurretPoisonInstance.gameObject);
        else if (TurretAreaDamageInstance != null)
            Destroy(TurretAreaDamageInstance.gameObject);
        else if (TurretArmourBreakerInstance != null)
            Destroy(TurretArmourBreakerInstance.gameObject);
        
        Debug.Log("Turret sold. Currency refunded: " + refundAmount);
        MenuUIHandler.main.mouse_over = false;
        UiManager.main.SetHoveringState(false);
    }

    // Retrieves the original cost of the turret.
    private int GetTurretOriginalCost()
    {
        if (turretInstance != null) return turretInstance.baseUpgradeCost;
        if (turretSlowInstance != null) return turretSlowInstance.baseUpgradeCost;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.baseUpgradeCost;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.baseUpgradeCost;
        if (TurretAreaDamageInstance != null) return TurretAreaDamageInstance.baseUpgradeCost;
        if (TurretArmourBreakerInstance != null) return TurretArmourBreakerInstance.baseUpgradeCost;
        return 0; // Default cost if no turret is set
    }

    // Draws the targeting range circle around the turret.
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

    // Hides the targeting range circle.
    private void HideCircle()
    {
        lineRenderer.enabled = false;
    }

    // Gets the targeting range of the current turret.
    private float GetTurretTargetingRange()
    {
        if (turretInstance != null) return turretInstance.targetingRange;
        if (turretSlowInstance != null) return turretSlowInstance.targetingRange;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.targetingRange;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.targetingRange;
        if (TurretAreaDamageInstance != null) return TurretAreaDamageInstance.targetingRange;
        if (TurretArmourBreakerInstance != null) return TurretArmourBreakerInstance.targetingRange;
        return 0f; // Default range if no turret is set
    }

    // Gets the position of the current turret.
    private Vector3 GetTurretPosition()
    {
        if (turretInstance != null) return turretInstance.transform.position;
        if (turretSlowInstance != null) return turretSlowInstance.transform.position;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.transform.position;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.transform.position;
        if (TurretAreaDamageInstance != null) return TurretAreaDamageInstance.transform.position;
        if (TurretArmourBreakerInstance != null) return TurretArmourBreakerInstance.transform.position;
        return Vector3.zero; // Default position if no turret is set
    }
}

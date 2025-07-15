using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static UpgradeUIHandler main;

    [SerializeField] private TextMeshProUGUI costUI;
    [SerializeField] private TextMeshProUGUI sellGainUI;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI levelUI;

    public bool mouse_over = false;

    public Turret turretInstance;
    public TurretSlow turretSlowInstance;
    public TurretLongRange TurretLongRangeInstance;
    public TurretPoison TurretPoisonInstance;
    public TurretAreaDamage TurretAreaDamageInstance;
    public TurretArmourBreaker TurretArmourBreakerInstance;

    [Header("Targeting Range Circle")]
    public int circleSegments = 50;
    public float lineSpacing = 0.5f;
    public Color innerCircleColor = new Color(1f, 0.8f, 0.8f);
    public Color outerCircleColor = new Color(1f, 0.3f, 0.3f);
    public Material lineMaterial;

    private List<LineRenderer> ringRenderers = new List<LineRenderer>();

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        if (sellButton != null)
            sellButton.onClick.AddListener(SellTower);
        else
            Debug.LogWarning("Sell button is not assigned.");
    }

    private void Update()
    {
        UpdateCostAndSellGainUI();
    }

    private void UpdateCostAndSellGainUI()
    {
        int upgradeCost = 0;
        int sellGain = 0;
        int level = 1;

        if (turretInstance != null)
        {
            upgradeCost = turretInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(turretInstance.baseUpgradeCost * 0.5f);
            level = turretInstance.GetLevel();
        }
        else if (turretSlowInstance != null)
        {
            upgradeCost = turretSlowInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(turretSlowInstance.baseUpgradeCost * 0.5f);
            level = turretSlowInstance.GetLevel();
        }
        else if (TurretLongRangeInstance != null)
        {
            upgradeCost = TurretLongRangeInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretLongRangeInstance.baseUpgradeCost * 0.5f);
            level = TurretLongRangeInstance.GetLevel();
        }
        else if (TurretPoisonInstance != null)
        {
            upgradeCost = TurretPoisonInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretPoisonInstance.baseUpgradeCost * 0.5f);
            level = TurretPoisonInstance.GetLevel();
        }
        else if (TurretAreaDamageInstance != null)
        {
            upgradeCost = TurretAreaDamageInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretAreaDamageInstance.baseUpgradeCost * 0.5f);
            level = TurretAreaDamageInstance.GetLevel();
        }
        else if (TurretArmourBreakerInstance != null)
        {
            upgradeCost = TurretArmourBreakerInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretArmourBreakerInstance.baseUpgradeCost * 0.5f);
            level = TurretArmourBreakerInstance.GetLevel();
        }
        else
        {
            costUI.text = "No selected tower";
            sellGainUI.text = "";
            levelUI.text = "";
            return;
        }

        costUI.text = upgradeCost.ToString();
        sellGainUI.text = sellGain.ToString();
        levelUI.text = $"Level {level}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouse_over = true;
        UiManager.main?.SetHoveringState(true);
        DrawCircle();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouse_over = false;
        UiManager.main?.SetHoveringState(false);
        HideCircle();
        gameObject.SetActive(false);
    }

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

    public void SellTower()
    {
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

        MenuUIHandler.main.mouse_over = false;
        UiManager.main.SetHoveringState(false);
    }

    private int GetTurretOriginalCost()
    {
        if (turretInstance != null) return turretInstance.baseUpgradeCost;
        if (turretSlowInstance != null) return turretSlowInstance.baseUpgradeCost;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.baseUpgradeCost;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.baseUpgradeCost;
        if (TurretAreaDamageInstance != null) return TurretAreaDamageInstance.baseUpgradeCost;
        if (TurretArmourBreakerInstance != null) return TurretArmourBreakerInstance.baseUpgradeCost;
        return 0;
    }

    private void DrawCircle()
    {
        float targetingRange = GetTurretTargetingRange();
        if (targetingRange <= 0f) return;

        ClearRings();
        Vector3 center = GetTurretPosition();
        int ringIndex = 0;

        for (float radius = lineSpacing; radius < targetingRange; radius += lineSpacing)
            DrawRing(radius, center, innerCircleColor, ++ringIndex);

        DrawRing(targetingRange, center, outerCircleColor, ++ringIndex);
    }

    private void DrawRing(float radius, Vector3 center, Color color, int index)
    {
        GameObject ringObj = new GameObject($"Ring_{index}");
        ringObj.transform.SetParent(transform);
        LineRenderer ring = ringObj.AddComponent<LineRenderer>();

        ring.useWorldSpace = true;
        ring.loop = true;
        ring.material = lineMaterial;
        ring.startColor = color;
        ring.endColor = color;
        ring.widthMultiplier = 0.02f;
        ring.positionCount = circleSegments + 1;

        float angleStep = 360f / circleSegments;

        for (int j = 0; j <= circleSegments; j++)
        {
            float angle = j * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            Vector3 pos = new Vector3(x, y, 0f) + center;
            ring.SetPosition(j, pos);
        }

        ringRenderers.Add(ring);
    }

    private void HideCircle()
    {
        ClearRings();
    }

    private void ClearRings()
    {
        foreach (LineRenderer ring in ringRenderers)
        {
            if (ring != null)
                Destroy(ring.gameObject);
        }
        ringRenderers.Clear();
    }

    private float GetTurretTargetingRange()
    {
        if (turretInstance != null) return turretInstance.targetingRange;
        if (turretSlowInstance != null) return turretSlowInstance.targetingRange;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.targetingRange;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.targetingRange;
        if (TurretAreaDamageInstance != null) return TurretAreaDamageInstance.targetingRange;
        if (TurretArmourBreakerInstance != null) return TurretArmourBreakerInstance.targetingRange;
        return 0f;
    }

    private Vector3 GetTurretPosition()
    {
        if (turretInstance != null) return turretInstance.transform.position;
        if (turretSlowInstance != null) return turretSlowInstance.transform.position;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.transform.position;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.transform.position;
        if (TurretAreaDamageInstance != null) return TurretAreaDamageInstance.transform.position;
        if (TurretArmourBreakerInstance != null) return TurretArmourBreakerInstance.transform.position;
        return Vector3.zero;
    }
}

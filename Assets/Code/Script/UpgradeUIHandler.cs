using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static UpgradeUIHandler main;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI costUI;
    [SerializeField] private TextMeshProUGUI sellGainUI;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI levelUI;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TextMeshProUGUI statPreviewUI;
    [SerializeField] private GameObject upgradePanel;

    [Header("Turret References")]
    public Turret turretInstance;
    public TurretSlow turretSlowInstance;
    public TurretLongRange TurretLongRangeInstance;
    public TurretPoison TurretPoisonInstance;
    public TurretAreaDamage TurretAreaDamageInstance;
    public TurretArmourBreaker TurretArmourBreakerInstance;

    [Header("Performance Stats (TMP)")]
    [SerializeField] private TextMeshProUGUI dpsUI;
    [SerializeField] private TextMeshProUGUI killsUI;

    [Header("Range Circle")]
    public int circleSegments = 50;
    public float lineSpacing = 0.5f;
    public Color innerCircleColor = new Color(1f, 0.8f, 0.8f);
    public Color outerCircleColor = new Color(1f, 0.3f, 0.3f);
    public Material lineMaterial;

    private List<LineRenderer> lineRendererPool = new List<LineRenderer>();
    private List<LineRenderer> activeRenderers = new List<LineRenderer>();

    private bool isHoveringUI = false;
    private bool isUpgradeButtonHovered = false;

    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        if (sellButton != null)
            sellButton.onClick.AddListener(SellTower);

        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(UpgradeTower);

            EventTrigger trigger = upgradeButton.GetComponent<EventTrigger>();
            if (trigger == null) trigger = upgradeButton.gameObject.AddComponent<EventTrigger>();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((data) => {
                isUpgradeButtonHovered = true;
                ShowStatPreview();
                TogglePerformanceStats(false);
            });

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((data) => {
                isUpgradeButtonHovered = false;
                HideStatPreview();
                TogglePerformanceStats(true);
            });

            trigger.triggers.Clear();
            trigger.triggers.Add(enter);
            trigger.triggers.Add(exit);
        }

        if (statPreviewUI != null) statPreviewUI.text = "";
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        UpdateCostAndSellGainUI();
        UpdatePerformanceStats();
    }

    public void ShowUpgradePanel()
    {
        gameObject.SetActive(true);
        UiManager.main?.SetHoveringState(true);
        DrawCircle();

        // Subscribe to events for dynamic updates
        if (TurretAreaDamageInstance != null)
            TurretAreaDamageInstance.OnStatsUpdated += UpdatePerformanceStats;
        if (TurretArmourBreakerInstance != null)
            TurretArmourBreakerInstance.OnStatsUpdated += UpdatePerformanceStats;
    }

    public void CloseUpgradePanel()
    {
        UiManager.main?.SetHoveringState(false);
        HideCircle();
        HideStatPreview();
        gameObject.SetActive(false);
        isHoveringUI = false;

        // Unsubscribe from events
        if (TurretAreaDamageInstance != null)
            TurretAreaDamageInstance.OnStatsUpdated -= UpdatePerformanceStats;
        if (TurretArmourBreakerInstance != null)
            TurretArmourBreakerInstance.OnStatsUpdated -= UpdatePerformanceStats;
    }

    // Turret reference setting methods
    public void SetTurret(Turret turret)
    {
        ClearAllTurretReferences();
        turretInstance = turret;
        ShowUpgradePanel();
    }

    public void SetTurret(TurretSlow turret)
    {
        ClearAllTurretReferences();
        turretSlowInstance = turret;
        ShowUpgradePanel();
    }

    public void SetTurret(TurretLongRange turret)
    {
        ClearAllTurretReferences();
        TurretLongRangeInstance = turret;
        ShowUpgradePanel();
    }

    public void SetTurret(TurretPoison turret)
    {
        ClearAllTurretReferences();
        TurretPoisonInstance = turret;
        ShowUpgradePanel();
    }

    public void SetTurret(TurretAreaDamage turret)
    {
        ClearAllTurretReferences();
        TurretAreaDamageInstance = turret;
        ShowUpgradePanel();
    }

    public void SetTurret(TurretArmourBreaker turret)
    {
        ClearAllTurretReferences();
        TurretArmourBreakerInstance = turret;
        ShowUpgradePanel();
    }

    private void ClearAllTurretReferences()
    {
        turretInstance = null;
        turretSlowInstance = null;
        TurretLongRangeInstance = null;
        TurretPoisonInstance = null;
        TurretAreaDamageInstance = null;
        TurretArmourBreakerInstance = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHoveringUI = true;
        UiManager.main?.SetHoveringState(true);
        DrawCircle();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHoveringUI = false;
        UiManager.main?.SetHoveringState(false);

        if (!UiManager.main.IsHoveringUI())
        {
            CloseUpgradePanel();
        }
    }

    private void TogglePerformanceStats(bool visible)
    {
        if (dpsUI != null)
            dpsUI.gameObject.SetActive(visible);
        if (killsUI != null)
            killsUI.gameObject.SetActive(visible);
    }

    public void UpgradeTower()
    {
        bool upgraded = false;

        if (turretInstance != null)
        {
            turretInstance.Upgrade();
            upgraded = true;
        }
        else if (turretSlowInstance != null)
        {
            turretSlowInstance.Upgrade();
            upgraded = true;
        }
        else if (TurretLongRangeInstance != null)
        {
            TurretLongRangeInstance.Upgrade();
            upgraded = true;
        }
        else if (TurretPoisonInstance != null)
        {
            TurretPoisonInstance.Upgrade();
            upgraded = true;
        }
        else if (TurretAreaDamageInstance != null)
        {
            TurretAreaDamageInstance.Upgrade();
            upgraded = true;
        }
        else if (TurretArmourBreakerInstance != null)
        {
            TurretArmourBreakerInstance.Upgrade();
            upgraded = true;
        }

        if (upgraded)
        {
            UpdateCostAndSellGainUI();
            DrawCircle();
            ShowStatPreview();
        }
    }

    public void SellTower()
    {
        int refundAmount = Mathf.RoundToInt(GetTurretOriginalCost() * 0.5f);
        LevelManager.main.AddCurrency(refundAmount);

        if (turretInstance != null) Destroy(turretInstance.gameObject);
        else if (turretSlowInstance != null) Destroy(turretSlowInstance.gameObject);
        else if (TurretLongRangeInstance != null) Destroy(TurretLongRangeInstance.gameObject);
        else if (TurretPoisonInstance != null) Destroy(TurretPoisonInstance.gameObject);
        else if (TurretAreaDamageInstance != null) Destroy(TurretAreaDamageInstance.gameObject);
        else if (TurretArmourBreakerInstance != null) Destroy(TurretArmourBreakerInstance.gameObject);

        MenuUIHandler.main.mouse_over = false;
        CloseUpgradePanel();
    }

    private void UpdateCostAndSellGainUI()
    {
        int upgradeCost = 0, sellGain = 0, level = 1;

        if (turretInstance != null)
        {
            upgradeCost = Mathf.RoundToInt(turretInstance.CalculateCost());
            sellGain = Mathf.RoundToInt(turretInstance.BaseCost * 0.5f);
            level = turretInstance.GetLevel();
        }
        else if (turretSlowInstance != null)
        {
            upgradeCost = Mathf.RoundToInt(turretSlowInstance.CalculateCost());
            sellGain = Mathf.RoundToInt(turretSlowInstance.BaseCost * 0.5f);
            level = turretSlowInstance.GetLevel();
        }
        else if (TurretLongRangeInstance != null)
        {
            upgradeCost = Mathf.RoundToInt(TurretLongRangeInstance.CalculateCost());
            sellGain = Mathf.RoundToInt(TurretLongRangeInstance.BaseCost * 0.5f);
            level = TurretLongRangeInstance.GetLevel();
        }
        else if (TurretPoisonInstance != null)
        {
            upgradeCost = Mathf.RoundToInt(TurretPoisonInstance.CalculateCost());
            sellGain = Mathf.RoundToInt(TurretPoisonInstance.BaseCost * 0.5f);
            level = TurretPoisonInstance.GetLevel();
        }
        else if (TurretAreaDamageInstance != null)
        {
            upgradeCost = Mathf.RoundToInt(TurretAreaDamageInstance.CalculateCost());
            sellGain = Mathf.RoundToInt(TurretAreaDamageInstance.BaseCost * 0.5f);
            level = TurretAreaDamageInstance.GetLevel();
        }
        else if (TurretArmourBreakerInstance != null)
        {
            upgradeCost = Mathf.RoundToInt(TurretArmourBreakerInstance.CalculateCost());
            sellGain = Mathf.RoundToInt(TurretArmourBreakerInstance.BaseCost * 0.5f);
            level = TurretArmourBreakerInstance.GetLevel();
        }

        if (costUI != null) costUI.text = upgradeCost.ToString();
        if (sellGainUI != null) sellGainUI.text = sellGain.ToString();
        if (levelUI != null) levelUI.text = level.ToString();
    }

    private int GetTurretOriginalCost()
    {
        if (turretInstance != null) return turretInstance.BaseCost;
        if (turretSlowInstance != null) return turretSlowInstance.BaseCost;
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.BaseCost;
        if (TurretPoisonInstance != null) return TurretPoisonInstance.BaseCost;
        if (TurretAreaDamageInstance != null) return TurretAreaDamageInstance.BaseCost;
        if (TurretArmourBreakerInstance != null) return TurretArmourBreakerInstance.BaseCost;

        return 0;
    }

    private float GetTurretTargetingRange()
    {
        if (turretInstance != null) return turretInstance.CalculateRange(turretInstance.GetLevel());
        if (turretSlowInstance != null) return turretSlowInstance.CalculateRange(turretSlowInstance.GetLevel());
        if (TurretLongRangeInstance != null) return TurretLongRangeInstance.CalculateRange(TurretLongRangeInstance.GetLevel());
        if (TurretPoisonInstance != null) return TurretPoisonInstance.CalculateRange(TurretPoisonInstance.GetLevel());
        if (TurretAreaDamageInstance != null) return TurretAreaDamageInstance.CalculateRange(TurretAreaDamageInstance.GetLevel());
        if (TurretArmourBreakerInstance != null) return TurretArmourBreakerInstance.CalculateRange(TurretArmourBreakerInstance.GetLevel());
        return 0f;
    }

    private void DrawCircle()
    {
        HideCircle();

        float maxRadius = GetTurretTargetingRange();
        if (maxRadius <= 0) return;

        Vector3 center = transform.position;
        float angleStep = 360f / circleSegments;
        int numberOfCircles = 20;  // More circles for finer spacing near center

        for (int i = 1; i <= numberOfCircles; i++)
        {
            float normalizedIndex = (float)i / numberOfCircles;
            // Quadratic spacing
            float radius = maxRadius * Mathf.Pow(normalizedIndex, 2);

            LineRenderer circleLine = GetLineRendererFromPool();
            circleLine.loop = true;

            float t = normalizedIndex;
            Color circleColor = Color.Lerp(innerCircleColor, outerCircleColor, t);
            circleLine.startColor = circleColor;
            circleLine.endColor = circleColor;

            circleLine.positionCount = circleSegments + 1;

            for (int j = 0; j <= circleSegments; j++)
            {
                float angle = j * angleStep * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                circleLine.SetPosition(j, center + new Vector3(x, y, 0));
            }
        }
    }



    private LineRenderer GetLineRendererFromPool()
    {
        LineRenderer lineRenderer;

        if (lineRendererPool.Count > 0)
        {
            lineRenderer = lineRendererPool[0];
            lineRendererPool.RemoveAt(0);
        }
        else
        {
            GameObject lineObj = new GameObject("RangeCircle");
            lineObj.transform.parent = transform;
            lineRenderer = lineObj.AddComponent<LineRenderer>();

            lineRenderer.material = lineMaterial;
            lineRenderer.widthMultiplier = 0.05f;
            lineRenderer.loop = true;
            lineRenderer.useWorldSpace = true;
            lineRenderer.numCapVertices = 5;
            lineRenderer.numCornerVertices = 5;
        }

        lineRenderer.enabled = true;
        activeRenderers.Add(lineRenderer);
        return lineRenderer;
    }

    private void HideCircle()
    {
        foreach (var line in activeRenderers)
        {
            line.enabled = false;
            lineRendererPool.Add(line);
        }
        activeRenderers.Clear();
    }

    private void ShowStatPreview()
    {
        if (!isUpgradeButtonHovered || statPreviewUI == null) return;

        statPreviewUI.gameObject.SetActive(true);

        int currentLevel = 0;

        if (turretInstance != null) currentLevel = turretInstance.GetLevel();
        else if (turretSlowInstance != null) currentLevel = turretSlowInstance.GetLevel();
        else if (TurretLongRangeInstance != null) currentLevel = TurretLongRangeInstance.GetLevel();
        else if (TurretPoisonInstance != null) currentLevel = TurretPoisonInstance.GetLevel();
        else if (TurretAreaDamageInstance != null) currentLevel = TurretAreaDamageInstance.GetLevel();
        else if (TurretArmourBreakerInstance != null) currentLevel = TurretArmourBreakerInstance.GetLevel();

        int nextLevel = currentLevel + 1;

        float bpsCurrent = 0f, bpsNext = 0f;
        float rangeCurrent = 0f, rangeNext = 0f;
        int damageCurrent = 0, damageNext = 0;
        int armourReductionCurrent = 0, armourReductionNext = 0;

        if (turretInstance != null)
        {
            bpsCurrent = turretInstance.CalculateBPS(currentLevel);
            bpsNext = turretInstance.CalculateBPS(nextLevel);
            rangeCurrent = turretInstance.CalculateRange(currentLevel);
            rangeNext = turretInstance.CalculateRange(nextLevel);
            damageCurrent = turretInstance.CalculateBulletDamage(currentLevel);
            damageNext = turretInstance.CalculateBulletDamage(nextLevel);
        }
        else if (turretSlowInstance != null)
        {
            bpsCurrent = turretSlowInstance.CalculateBPS(currentLevel);
            bpsNext = turretSlowInstance.CalculateBPS(nextLevel);
            rangeCurrent = turretSlowInstance.CalculateRange(currentLevel);
            rangeNext = turretSlowInstance.CalculateRange(nextLevel);
            damageCurrent = turretSlowInstance.CalculateBulletDamage(currentLevel);
            damageNext = turretSlowInstance.CalculateBulletDamage(nextLevel);
        }
        else if (TurretLongRangeInstance != null)
        {
            bpsCurrent = TurretLongRangeInstance.CalculateBPS(currentLevel);
            bpsNext = TurretLongRangeInstance.CalculateBPS(nextLevel);
            rangeCurrent = TurretLongRangeInstance.CalculateRange(currentLevel);
            rangeNext = TurretLongRangeInstance.CalculateRange(nextLevel);
            damageCurrent = TurretLongRangeInstance.CalculateBulletDamage(currentLevel);
            damageNext = TurretLongRangeInstance.CalculateBulletDamage(nextLevel);
        }
        else if (TurretPoisonInstance != null)
        {
            bpsCurrent = TurretPoisonInstance.CalculateBPS(currentLevel);
            bpsNext = TurretPoisonInstance.CalculateBPS(nextLevel);
            rangeCurrent = TurretPoisonInstance.CalculateRange(currentLevel);
            rangeNext = TurretPoisonInstance.CalculateRange(nextLevel);
            damageCurrent = TurretPoisonInstance.CalculateBulletDamage(currentLevel);
            damageNext = TurretPoisonInstance.CalculateBulletDamage(nextLevel);
        }
        else if (TurretAreaDamageInstance != null)
        {
            bpsCurrent = TurretAreaDamageInstance.CalculateBPS(currentLevel);
            bpsNext = TurretAreaDamageInstance.CalculateBPS(nextLevel);
            rangeCurrent = TurretAreaDamageInstance.CalculateRange(currentLevel);
            rangeNext = TurretAreaDamageInstance.CalculateRange(nextLevel);
            damageCurrent = TurretAreaDamageInstance.CalculateBulletDamage(currentLevel);
            damageNext = TurretAreaDamageInstance.CalculateBulletDamage(nextLevel);
        }
        else if (TurretArmourBreakerInstance != null)
        {
            bpsCurrent = TurretArmourBreakerInstance.CalculateBPS(currentLevel);
            bpsNext = TurretArmourBreakerInstance.CalculateBPS(nextLevel);
            rangeCurrent = TurretArmourBreakerInstance.CalculateRange(currentLevel);
            rangeNext = TurretArmourBreakerInstance.CalculateRange(nextLevel);
            damageCurrent = TurretArmourBreakerInstance.CalculateBulletDamage(currentLevel);
            damageNext = TurretArmourBreakerInstance.CalculateBulletDamage(nextLevel);
            armourReductionCurrent = TurretArmourBreakerInstance.CalculateArmourReduction(currentLevel);
            armourReductionNext = TurretArmourBreakerInstance.CalculateArmourReduction(nextLevel);
        }

        // Format preview text
        string previewText = $"BPS: {bpsCurrent:F2} -> {bpsNext:F2}\n" +
                            $"Range: {rangeCurrent:F2} -> {rangeNext:F2}\n" +
                            $"Damage: {damageCurrent} -> {damageNext}";

        // Add armour reduction for armour breaker
        if (TurretArmourBreakerInstance != null)
        {
            previewText += $"\nArmour Red: {armourReductionCurrent} -> {armourReductionNext}";
        }

        statPreviewUI.text = previewText;
    }

    private void HideStatPreview()
    {
        if (statPreviewUI != null) statPreviewUI.gameObject.SetActive(false);
    }

    private void UpdatePerformanceStats()
    {
        int kills = 0;
        float dps = 0f;
        string extraInfo = "";

        if (turretInstance != null)
        {
            kills = turretInstance.KillCount;
            dps = turretInstance.CalculateCurrentDPS();
        }
        else if (turretSlowInstance != null)
        {
            kills = turretSlowInstance.KillCount;
            dps = turretSlowInstance.CalculateCurrentDPS();
        }
        else if (TurretLongRangeInstance != null)
        {
            kills = TurretLongRangeInstance.KillCount;
            dps = TurretLongRangeInstance.CalculateCurrentDPS();
        }
        else if (TurretPoisonInstance != null)
        {
            kills = TurretPoisonInstance.KillCount;
            dps = TurretPoisonInstance.CalculateCurrentDPS();
        }
        else if (TurretAreaDamageInstance != null)
        {
            kills = TurretAreaDamageInstance.KillCount;
            dps = TurretAreaDamageInstance.CalculateCurrentDPS();
        }
        else if (TurretArmourBreakerInstance != null)
        {
            kills = TurretArmourBreakerInstance.KillCount;
            dps = TurretArmourBreakerInstance.CalculateCurrentDPS();
            extraInfo = TurretArmourBreakerInstance.DamageStats;
        }

        if (killsUI != null) killsUI.text = $"Kills: {kills}";
        
        if (dpsUI != null)
        {
            if (!string.IsNullOrEmpty(extraInfo))
                dpsUI.text = $"DPS: {dps:F1}\n{extraInfo}";
            else
                dpsUI.text = $"DPS: {dps:F1}";
        }
    }
}
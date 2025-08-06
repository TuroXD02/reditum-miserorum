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

    [Header("Range Circle")]
    public int circleSegments = 50;
    public float lineSpacing = 0.5f;
    public Color innerCircleColor = new Color(1f, 0.8f, 0.8f);
    public Color outerCircleColor = new Color(1f, 0.3f, 0.3f);
    public Material lineMaterial;

    // Use LineRenderer pool instead of creating/destroying
    private List<LineRenderer> lineRendererPool = new List<LineRenderer>();
    private List<LineRenderer> activeRenderers = new List<LineRenderer>();

    private bool isHoveringUI = false;

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
            enter.callback.AddListener((data) => ShowStatPreview());

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((data) => HideStatPreview());

            trigger.triggers.Clear();
            trigger.triggers.Add(enter);
            trigger.triggers.Add(exit);
        }

        statPreviewUI.text = "";
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        UpdateCostAndSellGainUI();
    }

    public void ShowUpgradePanel()
    {
        gameObject.SetActive(true);
        UiManager.main?.SetHoveringState(true);
        DrawCircle();
    }

    public void CloseUpgradePanel()
    {
        UiManager.main?.SetHoveringState(false);
        HideCircle();
        HideStatPreview();
        gameObject.SetActive(false);
        isHoveringUI = false;
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

    public void UpgradeTower()
    {
        bool upgraded = false;
        
        // FIXED: Use LevelManager.main.currency instead of GetCurrency()
        if (turretInstance != null && LevelManager.main.currency >= turretInstance.CalculateCost())
        {
            LevelManager.main.AddCurrency(-turretInstance.CalculateCost());
            turretInstance.Upgrade();
            upgraded = true;
        }
        else if (turretSlowInstance != null && LevelManager.main.currency >= turretSlowInstance.CalculateCost())
        {
            LevelManager.main.AddCurrency(-turretSlowInstance.CalculateCost());
            turretSlowInstance.Upgrade();
            upgraded = true;
        }
        else if (TurretLongRangeInstance != null && LevelManager.main.currency >= TurretLongRangeInstance.CalculateCost())
        {
            LevelManager.main.AddCurrency(-TurretLongRangeInstance.CalculateCost());
            TurretLongRangeInstance.Upgrade();
            upgraded = true;
        }
        else if (TurretPoisonInstance != null && LevelManager.main.currency >= TurretPoisonInstance.CalculateCost())
        {
            LevelManager.main.AddCurrency(-TurretPoisonInstance.CalculateCost());
            TurretPoisonInstance.Upgrade();
            upgraded = true;
        }
        else if (TurretAreaDamageInstance != null && LevelManager.main.currency >= TurretAreaDamageInstance.CalculateCost())
        {
            LevelManager.main.AddCurrency(-TurretAreaDamageInstance.CalculateCost());
            TurretAreaDamageInstance.Upgrade();
            upgraded = true;
        }
        else if (TurretArmourBreakerInstance != null && LevelManager.main.currency >= TurretArmourBreakerInstance.CalculateCost())
        {
            LevelManager.main.AddCurrency(-TurretArmourBreakerInstance.CalculateCost());
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
            upgradeCost = turretInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(turretInstance.BaseCost * 0.5f);
            level = turretInstance.GetLevel();
        }
        else if (turretSlowInstance != null)
        {
            upgradeCost = turretSlowInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(turretSlowInstance.BaseCost * 0.5f);
            level = turretSlowInstance.GetLevel();
        }
        else if (TurretLongRangeInstance != null)
        {
            upgradeCost = TurretLongRangeInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretLongRangeInstance.BaseCost * 0.5f);
            level = TurretLongRangeInstance.GetLevel();
        }
        else if (TurretPoisonInstance != null)
        {
            upgradeCost = TurretPoisonInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretPoisonInstance.BaseCost * 0.5f);
            level = TurretPoisonInstance.GetLevel();
        }
        else if (TurretAreaDamageInstance != null)
        {
            upgradeCost = TurretAreaDamageInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretAreaDamageInstance.BaseCost * 0.5f);
            level = TurretAreaDamageInstance.GetLevel();
        }
        else if (TurretArmourBreakerInstance != null)
        {
            upgradeCost = TurretArmourBreakerInstance.CalculateCost();
            sellGain = Mathf.RoundToInt(TurretArmourBreakerInstance.BaseCost * 0.5f);
            level = TurretArmourBreakerInstance.GetLevel();
        }

        costUI.text = upgradeCost.ToString();
        sellGainUI.text = sellGain.ToString();
        levelUI.text = $"Level {level}";
    }

    public void ShowStatPreview()
    {
        string preview = "";

        if (turretInstance != null)
            preview = GenerateTurretStatPreview(turretInstance.GetLevel(), turretInstance);
        else if (turretSlowInstance != null)
            preview = GenerateTurretStatPreview(turretSlowInstance.GetLevel(), turretSlowInstance);
        else if (TurretLongRangeInstance != null)
            preview = GenerateTurretStatPreview(TurretLongRangeInstance.GetLevel(), TurretLongRangeInstance);
        else if (TurretPoisonInstance != null)
            preview = GenerateTurretStatPreview(TurretPoisonInstance.GetLevel(), TurretPoisonInstance);
        else if (TurretAreaDamageInstance != null)
            preview = GenerateTurretStatPreview(TurretAreaDamageInstance.GetLevel(), TurretAreaDamageInstance);
        else if (TurretArmourBreakerInstance != null)
            preview = GenerateTurretStatPreview(TurretArmourBreakerInstance.GetLevel(), TurretArmourBreakerInstance);

        statPreviewUI.text = preview;
    }

    private void HideStatPreview() => statPreviewUI.text = "";

    private string GenerateTurretStatPreview(int currentLevel, object turret)
    {
        int nextLevel = currentLevel + 1;
        float currBPS = 0f, nextBPS = 0f;
        float currRange = 0f, nextRange = 0f;
        int currDamage = 0, nextDamage = 0;

        switch (turret)
        {
            case Turret t:
                currBPS = t.CalculateBPS(currentLevel);
                nextBPS = t.CalculateBPS(nextLevel);
                currRange = t.CalculateRange(currentLevel);
                nextRange = t.CalculateRange(nextLevel);
                currDamage = t.CalculateBulletDamage(currentLevel);
                nextDamage = t.CalculateBulletDamage(nextLevel);
                break;

            case TurretSlow tS:
                currBPS = tS.CalculateBPS(currentLevel);
                nextBPS = tS.CalculateBPS(nextLevel);
                currRange = tS.CalculateRange(currentLevel);
                nextRange = tS.CalculateRange(nextLevel);
                break;

            case TurretLongRange tLR:
                currBPS = tLR.CalculateBPS(currentLevel);
                nextBPS = tLR.CalculateBPS(nextLevel);
                currRange = tLR.CalculateRange(currentLevel);
                nextRange = tLR.CalculateRange(nextLevel);
                currDamage = tLR.CalculateBulletDamage(currentLevel);
                nextDamage = tLR.CalculateBulletDamage(nextLevel);
                return $"<b>Next Upgrade:</b>\n" +
                       $"\u2022 Damage: {currDamage} → {nextDamage} (x1 min dist. - x350 max dist)\n" +
                       $"\u2022 Range: {currRange:F1} → {nextRange:F1}\n" +
                       $"\u2022 Fire Rate: {currBPS:F2} → {nextBPS:F2}";

            case TurretPoison tP:
                currBPS = tP.CalculateBPS(currentLevel);
                nextBPS = tP.CalculateBPS(nextLevel);
                currRange = tP.CalculateRange(currentLevel);
                nextRange = tP.CalculateRange(nextLevel);
                currDamage = tP.CalculateBulletDamage(currentLevel);
                nextDamage = tP.CalculateBulletDamage(nextLevel);
                return $"<b>Next Upgrade:</b>\n" +
                       $"\u2022 Damage: {currDamage} → {nextDamage} (+ poison = 50dmg x 2sec. stackable)\n" +
                       $"\u2022 Range: {currRange:F1} → {nextRange:F1}\n" +
                       $"\u2022 Fire Rate: {currBPS:F2} → {nextBPS:F2}";

            case TurretAreaDamage tAD:
                currBPS = tAD.CalculateBPS(currentLevel);
                nextBPS = tAD.CalculateBPS(nextLevel);
                currRange = tAD.CalculateRange(currentLevel);
                nextRange = tAD.CalculateRange(nextLevel);
                currDamage = tAD.CalculateBulletDamage(currentLevel);
                nextDamage = tAD.CalculateBulletDamage(nextLevel);
                break;

            case TurretArmourBreaker tAB:
                currBPS = tAB.CalculateBPS(currentLevel);
                nextBPS = tAB.CalculateBPS(nextLevel);
                currRange = tAB.CalculateRange(currentLevel);
                nextRange = tAB.CalculateRange(nextLevel);
                currDamage = tAB.CalculateDamage(currentLevel);
                nextDamage = tAB.CalculateDamage(nextLevel);
                break;
        }

        return $"<b>Next Upgrade:</b>\n" +
               $"\u2022 Damage: {currDamage} → {nextDamage}\n" +
               $"\u2022 Range: {currRange:F1} → {nextRange:F1}\n" +
               $"\u2022 Fire Rate: {currBPS:F2} → {nextBPS:F2}";
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

    private void DrawCircle()
    {
        float range = GetTurretTargetingRange();
        if (range <= 0f)
        {
            Debug.LogWarning("Targeting range is 0 or negative");
            return;
        }

        ClearRings();
        Vector3 center = GetTurretPosition();
        center.z = -1f;

        int ringsDrawn = 0;

        for (float radius = lineSpacing; radius < range; radius += lineSpacing)
        {
            DrawRing(radius, center, innerCircleColor);
            ringsDrawn++;
        }

        DrawRing(range, center, outerCircleColor);
        ringsDrawn++;

        Debug.Log($"Drew {ringsDrawn} rings for range {range} at position {center}");
    }

    private void DrawRing(float radius, Vector3 center, Color color)
    {
        LineRenderer ring = GetAvailableLineRenderer();
        activeRenderers.Add(ring);

        ring.gameObject.SetActive(true);
        ring.positionCount = circleSegments + 1;
        ring.startColor = color;
        ring.endColor = color;
        ring.widthMultiplier = 0.07f;

        ring.material = lineMaterial ? lineMaterial : CreateDefaultMaterial();
        ring.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        ring.receiveShadows = false;
        ring.loop = true;
        ring.useWorldSpace = true;
        ring.sortingOrder = 2;

        float angleStep = 360f / circleSegments;
        for (int j = 0; j <= circleSegments; j++)
        {
            float angle = j * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius + center.x,
                Mathf.Sin(angle) * radius + center.y,
                center.z
            );
            ring.SetPosition(j, pos);
        }
    }

    private LineRenderer GetAvailableLineRenderer()
    {
        foreach (var lr in lineRendererPool)
        {
            if (!lr.gameObject.activeSelf)
                return lr;
        }

        GameObject go = new GameObject("RangeCircleRing");
        go.transform.SetParent(transform);
        LineRenderer newLR = go.AddComponent<LineRenderer>();
        lineRendererPool.Add(newLR);
        return newLR;
    }

    private Material CreateDefaultMaterial()
    {
        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null)
        {
            Debug.LogError("Unlit/Color shader not found!");
            return null;
        }

        Material mat = new Material(shader);
        mat.color = Color.white;
        return mat;
    }

    private void ClearRings()
    {
        foreach (var ring in activeRenderers)
        {
            if (ring != null)
                ring.gameObject.SetActive(false);
        }
        activeRenderers.Clear();
    }

    private void HideCircle() => ClearRings();

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
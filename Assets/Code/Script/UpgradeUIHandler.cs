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

    private List<LineRenderer> ringRenderers = new List<LineRenderer>();
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
            upgradeButton.onClick.AddListener(ShowStatPreview);

            // 🔁 Add hover triggers to show/hide stat preview
            EventTrigger trigger = upgradeButton.gameObject.AddComponent<EventTrigger>();

            var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener((data) => ShowStatPreview());

            var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener((data) => HideStatPreview());

            trigger.triggers.Add(enter);
            trigger.triggers.Add(exit);
        }

        // Hide stat preview at start
        statPreviewUI.text = "";

        gameObject.SetActive(false); // Start hidden
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
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHoveringUI = false;
        UiManager.main?.SetHoveringState(false);
        StartCoroutine(HideWhenNotHovering());
    }

    private System.Collections.IEnumerator HideWhenNotHovering()
    {
        yield return new WaitForSeconds(0.1f);
        if (!isHoveringUI && !UiManager.main.IsHoveringUI())
        {
            CloseUpgradePanel();
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

    private void HideStatPreview()
    {
        statPreviewUI.text = "";
    }


private string GenerateTurretStatPreview(int currentLevel, object turret)
{
    int nextLevel = currentLevel + 1;
    float currBPS = 0f, nextBPS = 0f;
    float currRange = 0f, nextRange = 0f;
    int currDamage = 0, nextDamage = 0;

    if (turret is Turret t)
    {
        currBPS = t.CalculateBPS(currentLevel);
        nextBPS = t.CalculateBPS(nextLevel);
        currRange = t.CalculateRange(currentLevel);
        nextRange = t.CalculateRange(nextLevel);
        currDamage = t.CalculateBulletDamage(currentLevel);
        nextDamage = t.CalculateBulletDamage(nextLevel);
    }
    else if (turret is TurretSlow tS)
    {
        currBPS = tS.CalculateBPS(currentLevel);
        nextBPS = tS.CalculateBPS(nextLevel);
        currRange = tS.CalculateRange(currentLevel);
        nextRange = tS.CalculateRange(nextLevel);
    }
    // Add handling for TurretLongRange
    else if (turret is TurretLongRange tLR)
    {
        currBPS = tLR.CalculateBPS(currentLevel);
        nextBPS = tLR.CalculateBPS(nextLevel);
        currRange = tLR.CalculateRange(currentLevel);
        nextRange = tLR.CalculateRange(nextLevel);
        currDamage = tLR.CalculateBulletDamage(currentLevel);
        nextDamage = tLR.CalculateBulletDamage(nextLevel);
    }
    // Add handling for other turret types
    else if (turret is TurretPoison tP)
    {
        currBPS = tP.CalculateBPS(currentLevel);
        nextBPS = tP.CalculateBPS(nextLevel);
        currRange = tP.CalculateRange(currentLevel);
        nextRange = tP.CalculateRange(nextLevel);
        currDamage = tP.CalculateBulletDamage(currentLevel);
        nextDamage = tP.CalculateBulletDamage(nextLevel);
    }
    else if (turret is TurretAreaDamage tAD)
    {
        currBPS = tAD.CalculateBPS(currentLevel);
        nextBPS = tAD.CalculateBPS(nextLevel);
        currRange = tAD.CalculateRange(currentLevel);
        nextRange = tAD.CalculateRange(nextLevel);
        currDamage = tAD.CalculateBulletDamage(currentLevel);
        nextDamage = tAD.CalculateBulletDamage(nextLevel);
    }
    else if (turret is TurretArmourBreaker tAB)
    {
        currBPS = tAB.CalculateBPS(currentLevel);
        nextBPS = tAB.CalculateBPS(nextLevel);
        currRange = tAB.CalculateRange(currentLevel);
        nextRange = tAB.CalculateRange(nextLevel);

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
        if (range <= 0f) return;

        ClearRings();
        Vector3 center = GetTurretPosition();
        int index = 0;

        for (float radius = lineSpacing; radius < range; radius += lineSpacing)
            DrawRing(radius, center, innerCircleColor, ++index);

        DrawRing(range, center, outerCircleColor, ++index);
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
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * radius + center.x,
                Mathf.Sin(angle) * radius + center.y,
                0
            );
            ring.SetPosition(j, pos);
        }

        ringRenderers.Add(ring);
    }

    private void ClearRings()
    {
        foreach (var ring in ringRenderers)
        {
            if (ring != null) Destroy(ring.gameObject);
        }
        ringRenderers.Clear();
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

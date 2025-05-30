using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    public static LevelManager instance;

    public Transform startPoint;
    public Transform[] path;
    public int currency;

    // Turret ghost preview
    private GameObject turretGhost;
    private Vector3 ghostOriginalScale;

    [Header("Ghost Scaling Settings")]
    [SerializeField] private float baseResolutionWidth = 1920f;
    [SerializeField] private float ghostScaleMultiplier = 1f;
    [SerializeField] private float ghostScaleModifier = 0.5f;

    [Header("Armor Change Effect Settings")]
    [SerializeField] private GameObject armorIncreasedEffectPrefab;
    [SerializeField] private GameObject armorReducedEffectPrefab;
    [SerializeField] private GameObject armorZeroEffectPrefab;
    [SerializeField] private float effectDuration = 2f;

    // LineRenderer range preview
    [Header("Ghost Range Preview")]
    [SerializeField] private int circleSegments = 60;
    [SerializeField] private Color circleColor = Color.cyan;
    private LineRenderer ghostRangeLR;

    // --- Armor Change Event ---
    public delegate void ArmorChangeEvent(Transform target, bool armorUp, bool isArmorZero);
    public static event ArmorChangeEvent OnArmorChanged;

    private void Awake()
    {
        main = this;
        instance = this;
        OnArmorChanged += HandleArmorChangeEvent;
    }

    private void OnDestroy()
    {
        OnArmorChanged -= HandleArmorChangeEvent;
    }

    private void Start()
    {
        currency = 1000;
        GetComponent<PlayerHealthSystem>().Init();
        ClearSelectedTurret(); // <-- Ensure no ghost is selected at start
    
    }

    private void Update()
    {
        UpdateTurretGhostPosition();
    }

    // Currency
    public void IncreaseCurrency(int amount) => currency += amount;
    public bool SpendCurrency(int amount)
    {
        if (amount <= currency)
        {
            currency -= amount;
            return true;
        }
        return false;
    }
    public void AddCurrency(int amount) => IncreaseCurrency(amount);

    // Ghost turret preview
    public void SetSelectedTurret(GameObject turretPrefab)
    {
        ClearSelectedTurret();

        if (turretPrefab == null) return;

        turretGhost = Instantiate(turretPrefab);
        turretGhost.name = turretPrefab.name + "_Ghost";
        ghostOriginalScale = turretGhost.transform.localScale;

        // semi-transparent
        var sr = turretGhost.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.5f;
            sr.color = c;
        }

        // add LineRenderer for range
        ghostRangeLR = turretGhost.AddComponent<LineRenderer>();
        ghostRangeLR.loop = true;
        ghostRangeLR.positionCount = circleSegments + 1;
        ghostRangeLR.useWorldSpace = true;
        ghostRangeLR.widthMultiplier = 0.03f;
        ghostRangeLR.material = new Material(Shader.Find("Sprites/Default"));
        ghostRangeLR.startColor = circleColor;
        ghostRangeLR.endColor = circleColor;
    }

    private void UpdateTurretGhostPosition()
    {
        if (turretGhost == null) return;

        // follow mouse
        Vector3 m = Input.mousePosition; m.z = 10f;
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 world = cam.ScreenToWorldPoint(m);
        turretGhost.transform.position = new Vector3(world.x, world.y, 0f);

        // scale ghost
        float resScale = Screen.width / baseResolutionWidth;
        turretGhost.transform.localScale = ghostOriginalScale * resScale * ghostScaleMultiplier * ghostScaleModifier;

        // update range circle
        if (ghostRangeLR != null)
        {
            float range = 0f;
            // detect range from common turret scripts:
            if (turretGhost.TryGetComponent<TurretSlow>(out var slow))              range = slow.targetingRange;
            else if (turretGhost.TryGetComponent<TurretPoison>(out var poison))     range = poison.targetingRange;
            else if (turretGhost.TryGetComponent<TurretLongRange>(out var lr))      range = lr.targetingRange;
            else if (turretGhost.TryGetComponent<TurretAreaDamage>(out var area))   range = area.targetingRange;
            else if (turretGhost.TryGetComponent<TurretArmourBreaker>(out var ab))  range = ab.targetingRange;
            else if (turretGhost.TryGetComponent<Turret>(out var basic))            range = basic.targetingRange;
            // draw circle
            float angleStep = 360f / circleSegments;
            for (int i = 0; i <= circleSegments; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleStep);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * range, Mathf.Sin(angle) * range, 0f) 
                              + turretGhost.transform.position;
                ghostRangeLR.SetPosition(i, pos);
            }
        }
    }

    public void ClearSelectedTurret()
    {
        if (turretGhost != null) Destroy(turretGhost);
        turretGhost = null;
        if (ghostRangeLR != null) Destroy(ghostRangeLR);
        ghostRangeLR = null;
    }

    // Armor change handler
    private void HandleArmorChangeEvent(Transform target, bool armorUp, bool isArmorZero)
    {
        if (target == null) return;
        GameObject prefab = isArmorZero
            ? armorZeroEffectPrefab
            : (armorUp ? armorIncreasedEffectPrefab : armorReducedEffectPrefab);
        if (prefab != null)
        {
            var fx = Instantiate(prefab, target.position, Quaternion.identity, target);
            Destroy(fx, effectDuration);
        }
    }

    // Called by enemies
    public void PlayArmorChangeEffect(Transform target, bool armorUp, bool isArmorZero = false)
    {
        OnArmorChanged?.Invoke(target, armorUp, isArmorZero);
    }
}

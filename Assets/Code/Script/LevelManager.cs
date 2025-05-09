using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    public static LevelManager instance;

    public Transform startPoint;
    public Transform[] path;
    public int currency;

    // Turret ghost preview variables
    private GameObject turretGhost;
    private GameObject rangeIndicator;
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

    // Range indicator prefab (a 1×1 transparent circle)
    [Header("Ghost Range Preview")]
    [SerializeField] private GameObject rangeIndicatorPrefab;

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
        if (turretPrefab == null) return;
        if (turretGhost != null) Destroy(turretGhost);
        if (rangeIndicator != null) Destroy(rangeIndicator);

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

        // spawn range indicator
        if (rangeIndicatorPrefab != null)
        {
            rangeIndicator = Instantiate(rangeIndicatorPrefab, turretGhost.transform);
            rangeIndicator.name = "RangeIndicator";
            // remove any collider so it doesn't block clicks
            var col = rangeIndicator.GetComponent<Collider2D>();
            if (col != null) Destroy(col);
        }
    }

    private void UpdateTurretGhostPosition()
    {
        if (turretGhost == null) return;

        // follow mouse
        Vector3 m = Input.mousePosition;
        m.z = 10f;
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 world = cam.ScreenToWorldPoint(m);
        turretGhost.transform.position = new Vector3(world.x, world.y, 0f);

        // scale ghost
        float resScale = Screen.width / baseResolutionWidth;
        turretGhost.transform.localScale = ghostOriginalScale * resScale * ghostScaleMultiplier * ghostScaleModifier;

        // update range indicator
        if (rangeIndicator != null)
        {
            float range = 0f;
            // try common turret scripts
            var slow = turretGhost.GetComponent<TurretSlow>();
            if (slow != null) range = slow.targetingRange;
            else
            {
                var poison = turretGhost.GetComponent<TurretPoison>();
                if (poison != null) range = poison.targetingRange;
            }
            // scale circle diameter = 2 * range
            rangeIndicator.transform.localScale = Vector3.one * (range * 2f);
        }
    }

    public void ClearSelectedTurret()
    {
        if (turretGhost != null) Destroy(turretGhost);
        turretGhost = null;
        if (rangeIndicator != null) Destroy(rangeIndicator);
        rangeIndicator = null;
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

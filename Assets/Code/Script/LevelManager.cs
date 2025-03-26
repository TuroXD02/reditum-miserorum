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

    // Turret ghost preview variables (unchanged)...
    private GameObject turretGhost;
    private Vector3 ghostOriginalScale;

    [Header("Ghost Scaling Settings")]
    [SerializeField] private float baseResolutionWidth = 1920f;
    [SerializeField] private float ghostScaleMultiplier = 1f;
    [SerializeField] private float ghostScaleModifier = 0.5f;

    [Header("Armor Change Effect Settings")]
    [SerializeField] private GameObject armorIncreasedEffectPrefab; // Effect for armor increase.
    [SerializeField] private GameObject armorReducedEffectPrefab;   // Effect for armor reduction.
    [SerializeField] private GameObject armorZeroEffectPrefab;      // Effect when armor reaches 0.
    [SerializeField] private float effectDuration = 2f;             // Duration the effect stays.

    // --- New: Armor Change Event ---
    public delegate void ArmorChangeEvent(Transform target, bool armorUp, bool isArmorZero);
    public static event ArmorChangeEvent OnArmorChanged;

    private void Awake()
    {
        main = this;
        instance = this;
        // Subscribe to the event.
        OnArmorChanged += HandleArmorChangeEvent;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks.
        OnArmorChanged -= HandleArmorChangeEvent;
    }

    private void Start()
    {
        currency = 1000;
        // Initialize the player's health system.
        GetComponent<PlayerHealthSystem>().Init();
    }

    private void Update()
    {
        UpdateTurretGhostPosition();
    }

    // Currency management methods...
    public void IncreaseCurrency(int amount)
    {
        currency += amount;
    }

    public bool SpendCurrency(int amount)
    {
        if (amount <= currency)
        {
            currency -= amount;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void AddCurrency(int amount)
    {
        IncreaseCurrency(amount);
    }

    // Turret ghost preview functions...
    public void SetSelectedTurret(GameObject turretPrefab)
    {
        if (turretPrefab == null)
        {
            Debug.LogError("[LevelManager] SetSelectedTurret: turretPrefab is null!");
            return;
        }
        if (turretGhost != null)
        {
            Destroy(turretGhost);
            turretGhost = null;
        }
        turretGhost = Instantiate(turretPrefab);
        turretGhost.name = turretPrefab.name + "_Ghost";
        ghostOriginalScale = turretGhost.transform.localScale;
        SpriteRenderer sr = turretGhost.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color col = sr.color;
            col.a = 0.5f;
            sr.color = col;
        }
        else
        {
            Debug.LogWarning("[LevelManager] SetSelectedTurret: No SpriteRenderer found on " + turretPrefab.name);
        }
    }

    private void UpdateTurretGhostPosition()
    {
        if (turretGhost != null)
        {
            Vector3 mousePos = Input.mousePosition;
            if (Camera.main == null)
            {
                Debug.LogError("[LevelManager] UpdateTurretGhostPosition: Camera.main is null!");
                return;
            }
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            turretGhost.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            float resolutionScale = Screen.width / baseResolutionWidth;
            turretGhost.transform.localScale = ghostOriginalScale * resolutionScale * ghostScaleMultiplier * ghostScaleModifier;
        }
    }

    public void ClearSelectedTurret()
    {
        if (turretGhost != null)
        {
            Destroy(turretGhost);
            turretGhost = null;
            Debug.Log("Turret ghost cleared.");
        }
    }

    /// <summary>
    /// Central handler for armor change effects.  
    /// This method is invoked when any enemy calls LevelManager.PlayArmorChangeEffect().
    /// It instantiates the appropriate effect prefab at the target's position.
    /// </summary>
    /// <param name="target">The enemy transform.</param>
    /// <param name="armorUp">True for armor up, false for armor down.</param>
    /// <param name="isArmorZero">True if armor reached zero.</param>
    private void HandleArmorChangeEvent(Transform target, bool armorUp, bool isArmorZero)
    {
        if (target == null)
            return;

        GameObject effectPrefab = null;
        if (isArmorZero)
        {
            effectPrefab = armorZeroEffectPrefab;
        }
        else
        {
            effectPrefab = armorUp ? armorIncreasedEffectPrefab : armorReducedEffectPrefab;
        }

        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, target.position, Quaternion.identity, target);
            Destroy(effect, effectDuration);
        }
    }

    /// <summary>
    /// Call this method from any enemy health script to trigger an armor change effect.
    /// </summary>
    /// <param name="target">The enemy transform.</param>
    /// <param name="armorUp">True if armor increased; false if armor decreased.</param>
    /// <param name="isArmorZero">True if armor reached 0.</param>
    public void PlayArmorChangeEffect(Transform target, bool armorUp, bool isArmorZero = false)
    {
        if (OnArmorChanged != null)
        {
            OnArmorChanged(target, armorUp, isArmorZero);
        }
    }
}

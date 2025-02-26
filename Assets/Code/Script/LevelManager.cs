using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager main;
    public static LevelManager instance;

    public Transform startPoint;
    public Transform[] path;

    public int currency;

    // ------------------------------
    // Turret Ghost Preview Variables
    // ------------------------------
    // The preview object that follows the cursor.
    private GameObject turretGhost;
    // Stores the original local scale of the ghost.
    private Vector3 ghostOriginalScale;

    [Header("Ghost Scaling Settings")]
    [SerializeField] private float baseResolutionWidth = 1920f;
    [SerializeField] private float ghostScaleMultiplier = 1f;
    [SerializeField] private float ghostScaleModifier = 0.5f;

    [Header("Armor Change Effect Settings")]
    [SerializeField] private GameObject armorReducedEffectPrefab;   // Prefab for armor reduction effect.
    [SerializeField] private GameObject armorIncreasedEffectPrefab;   // Prefab for armor increase effect.
    [SerializeField] private float armorEffectDuration = 2f;          // Duration the effect stays on the enemy.

    private void Awake()
    {
        // Do not remove these assignments—they are used by other scripts.
        main = this;
        instance = this;
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

    // ---------------------------
    // Currency Management Methods
    // ---------------------------
    
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

    // ---------------------------
    // Turret Ghost Preview Functions
    // ---------------------------
    
    /// <summary>
    /// Call this function when a turret is selected from your build UI.
    /// It instantiates a ghost version of the turret (using its prefab) at the cursor with reduced opacity.
    /// </summary>
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

    /// <summary>
    /// Updates the turret ghost's position and scales it relative to screen resolution.
    /// </summary>
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

    /// <summary>
    /// Clears the turret ghost preview.
    /// </summary>
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
    /// Plays an armor change effect on an enemy.
    /// Call this function whenever an enemy's armor is increased or reduced.
    /// </summary>
    /// <param name="enemyTransform">The enemy's transform.</param>
    /// <param name="armorIncreased">True if armor is increased; false if reduced.</param>
    public void PlayArmorChangeEffect(Transform enemyTransform, bool armorIncreased)
    {
        if(enemyTransform == null)
        {
            Debug.LogWarning("[LevelManager] PlayArmorChangeEffect: enemyTransform is null");
            return;
        }
    
        // Choose the appropriate prefab based on whether armor is increased or reduced.
        GameObject effectPrefab = armorIncreased ? armorIncreasedEffectPrefab : armorReducedEffectPrefab;
        if(effectPrefab == null)
        {
            Debug.LogWarning("[LevelManager] PlayArmorChangeEffect: No effect prefab assigned for " + (armorIncreased ? "armor increase" : "armor reduction"));
            return;
        }
    
        Debug.Log("[LevelManager] Playing armor change effect on " + enemyTransform.name + " (armorIncreased: " + armorIncreased + ")");
        // Instantiate the effect as a child of the enemy so it follows the enemy.
        GameObject effect = Instantiate(effectPrefab, enemyTransform.position, Quaternion.identity, enemyTransform);
        // Force the effect to appear at the enemy's origin.
        effect.transform.localPosition = Vector3.zero;
        Destroy(effect, armorEffectDuration);
    }

}

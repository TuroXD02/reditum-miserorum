using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    // Base resolution width to compare against.
    [SerializeField] private float baseResolutionWidth = 1920f;
    // Multiplier to fine-tune ghost scale relative to the screen resolution.
    [SerializeField] private float ghostScaleMultiplier = 1f;
    // Scale modifier to reduce the ghost size (values less than 1 will scale it down).
    [SerializeField] private float ghostScaleModifier = 0.5f;

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
            Debug.Log("You don't have enough money.");
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
    /// <param name="turretPrefab">The turret prefab to display as a ghost.</param>
    public void SetSelectedTurret(GameObject turretPrefab)
    {
        if (turretPrefab == null)
        {
            Debug.LogError("[LevelManager] SetSelectedTurret: turretPrefab is null!");
            return;
        }
        
        // Destroy any existing ghost.
        if (turretGhost != null)
        {
            Destroy(turretGhost);
            turretGhost = null;
        }
        
        // Instantiate the ghost turret.
        turretGhost = Instantiate(turretPrefab);
        turretGhost.name = turretPrefab.name + "_Ghost";
        
        // Save its original scale.
        ghostOriginalScale = turretGhost.transform.localScale;
        
        // Lower its opacity so it appears as a "ghost" preview.
        // Use GetComponentInChildren in case the SpriteRenderer is on a child.
        SpriteRenderer sr = turretGhost.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color col = sr.color;
            col.a = 0.5f; // Set to 50% opacity.
            sr.color = col;
        }
        else
        {
            Debug.LogWarning("[LevelManager] SetSelectedTurret: No SpriteRenderer found on " + turretPrefab.name);
        }
        
        Debug.Log("Turret ghost spawned: " + turretGhost.name);
    }

    /// <summary>
    /// Updates the position of the turret ghost so that it follows the mouse cursor.
    /// Also adjusts the ghost's scale based on the current screen resolution.
    /// </summary>
    private void UpdateTurretGhostPosition()
    {
        if (turretGhost != null)
        {
            // Convert mouse position to world position.
            Vector3 mousePos = Input.mousePosition;
            if (Camera.main == null)
            {
                Debug.LogError("[LevelManager] UpdateTurretGhostPosition: Camera.main is null!");
                return;
            }
            mousePos.z = 10f; // Adjust this based on your camera's distance from the game plane.
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            turretGhost.transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            
            // Adjust the ghost's scale based on the current screen width relative to a base resolution.
            float resolutionScale = Screen.width / baseResolutionWidth;
            turretGhost.transform.localScale = ghostOriginalScale * resolutionScale * ghostScaleMultiplier * ghostScaleModifier;
        }
    }

    /// <summary>
    /// Call this function when the turret is placed or the action is canceled,
    /// to remove the turret ghost preview.
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Plot : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Color hoverColor;

    [Header("Placement SFX (optional)")]
    [Tooltip("Sound to play when a turret is successfully placed on this plot.")]
    [SerializeField] private AudioClip placementClip;
    [Tooltip("Optional mixer group for the placement sound.")]
    [SerializeField] private AudioMixerGroup placementMixerGroup;
    [Range(0f, 1f)]
    [SerializeField] private float placementVolume = 1f;

    private Color startColor;
    private GameObject towerObj;

    private void Start()
    {
        // Save the starting color of the sprite renderer
        if (sr != null) startColor = sr.color;
    }

    private void OnMouseEnter()
    {
        // Change color to hover color when the mouse enters
        if (sr != null) sr.color = hoverColor;
    }

    private void OnMouseExit()
    {
        // Revert to the starting color when the mouse exits
        if (sr != null) sr.color = startColor;
    }

    private void OnMouseDown() // aka click mouse
    {
        if (UiManager.main.IsHoveringUI()) return; // Prevent interaction if the UI is active

        // Check if there is already a tower on this plot
        if (towerObj != null)
        {
            // Switch based on the component name to open the appropriate UI
            switch (towerObj.name)
            {
                case "Maddalena(Clone)":
                    towerObj.GetComponent<TurretSlow>()?.OpenUpgradeUI();
                    break;
                case "SanPietro(Clone)":
                    towerObj.GetComponent<Turret>()?.OpenUpgradeUI();
                    break;
                case "Davide(Clone)":
                    towerObj.GetComponent<TurretLongRange>()?.OpenUpgradeUI();
                    break;
                case "Eva(Clone)":
                    towerObj.GetComponent<TurretPoison>()?.OpenUpgradeUI();
                    break;
                case "Lot(Clone)":
                    towerObj.GetComponent<TurretAreaDamage>()?.OpenUpgradeUI();
                    break;
                case "Tubal(Clone)":
                    towerObj.GetComponent<TurretArmourBreaker>()?.OpenUpgradeUI();
                    break;
                default:
                    Debug.LogWarning("Unknown turret type.");
                    break;
            }

            return;
        }

        // Get the tower to build or use the last selected tower
        Tower towerToBuild = BuildManager.main.GetSelectedTower();

        // If no tower is selected to build, exit
        if (towerToBuild == null)
        {
            Debug.LogWarning("No tower selected to build.");
            return;
        }

        // Check if the player has enough currency and spend it
        if (LevelManager.main.SpendCurrency(towerToBuild.cost))
        {
            // Instantiate the new tower and set it to the towerObj reference
            towerObj = Instantiate(towerToBuild.prefab, transform.position, Quaternion.identity);

            // Try to use the Turret API to ensure placement flows properly
            var turret = towerObj.GetComponent<Turret>();
            if (turret != null)
            {
                // Mark as preview BEFORE its Start runs (this call happens immediately after Instantiate),
                // so Start() won't play the place sound. Then call OnPlaced(...) to finalize placement.
                turret.SetPreview(true);                 // suppress preview start SFX
                turret.OnPlaced(towerToBuild.cost);      // finalize placement (sets invested, clears preview)

                // Play the plot-specific placement SFX (if assigned).
                PlayPlacementClip();
            }
            else
            {
                // Fallback: still play placement sfx when prefab doesn't have a Turret component
                Debug.LogWarning($"Placed prefab {towerObj.name} does not contain a Turret component; cannot call OnPlaced().");
                PlayPlacementClip();
            }
        }
    }

    /// <summary>
    /// Plays the placement clip via a temporary AudioSource so it survives object destruction/scene changes.
    /// </summary>
    private void PlayPlacementClip()
    {
        if (placementClip == null) return;

        float vol = Mathf.Clamp01(placementVolume);

        GameObject tmp = new GameObject("PlacementSFX");
        tmp.transform.position = transform.position;

        AudioSource src = tmp.AddComponent<AudioSource>();
        src.clip = placementClip;
        src.playOnAwake = false;
        src.spatialBlend = 0f; // 2D sound
        src.volume = vol;

        if (placementMixerGroup != null)
            src.outputAudioMixerGroup = placementMixerGroup;

        DontDestroyOnLoad(tmp);
        src.Play();
        Destroy(tmp, placementClip.length + 0.1f);
    }
}

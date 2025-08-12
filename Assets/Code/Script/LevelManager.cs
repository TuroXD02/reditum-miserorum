using System.Collections.Generic;
using UnityEngine;

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

    [Header("Armor Change Sounds")]
    [SerializeField] private AudioClip armorUpSound;
    [SerializeField] private AudioClip armorDownSound;
    [SerializeField] private AudioClip armorZeroSound;

    [SerializeField] private GameObject armorAudioSourcePrefab;
    [SerializeField] private int maxSimultaneousSounds = 5;

    private readonly List<AudioSource> activeArmorSources = new();

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
        currency = 120;
        GetComponent<PlayerHealthSystem>().Init();
        ClearSelectedTurret();
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

        var sr = turretGhost.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 0.5f;
            sr.color = c;
        }

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

        Vector3 m = Input.mousePosition; m.z = 10f;
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 world = cam.ScreenToWorldPoint(m);
        turretGhost.transform.position = new Vector3(world.x, world.y, 0f);

        float resScale = Screen.width / baseResolutionWidth;
        turretGhost.transform.localScale = ghostOriginalScale * resScale * ghostScaleMultiplier * ghostScaleModifier;

        if (ghostRangeLR != null)
        {
            float range = 0f;
            if (turretGhost.TryGetComponent<TurretSlow>(out var slow))              range = slow.TargetingRange;
            else if (turretGhost.TryGetComponent<TurretPoison>(out var poison))     range = poison.TargetingRange;
            else if (turretGhost.TryGetComponent<TurretLongRange>(out var lr))      range = lr.TargetingRange;
            else if (turretGhost.TryGetComponent<TurretAreaDamage>(out var area))   range = area.TargetingRange;
            else if (turretGhost.TryGetComponent<TurretArmourBreaker>(out var ab))  range = ab.TargetingRange;
            else if (turretGhost.TryGetComponent<Turret>(out var basic))            range = basic.TargetingRange;

            float angleStep = 360f / circleSegments;
            for (int i = 0; i <= circleSegments; i++)
            {
                float angle = Mathf.Deg2Rad * (i * angleStep);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * range, Mathf.Sin(angle) * range, 0f) + turretGhost.transform.position;
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

        AudioClip clipToPlay = null;
        if (isArmorZero && armorZeroSound != null)
            clipToPlay = armorZeroSound;
        else if (armorUp && armorUpSound != null)
            clipToPlay = armorUpSound;
        else if (!armorUp && armorDownSound != null)
            clipToPlay = armorDownSound;

        PlayArmorSound(clipToPlay);
    }

    private void PlayArmorSound(AudioClip clip)
    {
        if (clip == null || armorAudioSourcePrefab == null)
        {
            Debug.LogWarning("Missing AudioClip or AudioSource prefab.");
            return;
        }

        // Clean up old sources
        activeArmorSources.RemoveAll(src => src == null || !src.isPlaying);

        // Create new instance
        var audioObj = Instantiate(armorAudioSourcePrefab, transform);
        var newSource = audioObj.GetComponent<AudioSource>();
        if (newSource == null)
        {
            Debug.LogError("Prefab is missing AudioSource component.");
            Destroy(audioObj);
            return;
        }

        newSource.clip = clip;
        newSource.volume = 1f;
        newSource.Play();

        Destroy(audioObj, clip.length + 0.1f);
        activeArmorSources.Add(newSource);

        // Lower older ones
        int count = activeArmorSources.Count;
        for (int i = 0; i < count; i++)
        {
            if (activeArmorSources[i] != null)
            {
                float volumeFactor = Mathf.Clamp01(1f - ((float)(count - i) / maxSimultaneousSounds));
                activeArmorSources[i].volume = volumeFactor;
            }
        }
    }

    // External call
    public void PlayArmorChangeEffect(Transform target, bool armorUp, bool isArmorZero = false)
    {
        OnArmorChanged?.Invoke(target, armorUp, isArmorZero);
    }
    
    public void ResetState()
    {
        Debug.Log("[LevelManager] Resetting state for new game...");

        currency = 1000; // reset money

        // Reset waves or progression if you track them
        // waveNumber = 0;

        // Reset game speed
        Time.timeScale = 1f;

        // Reset health
        var health = GetComponent<PlayerHealthSystem>();
        if (health != null)
            health.Init();

        // Close turret ghost previews
        ClearSelectedTurret();

        // TODO: Close any open UI popups
        // e.g., UiManager.main?.CloseAllPopups();

        // Reset other systems if needed
    }
    
}

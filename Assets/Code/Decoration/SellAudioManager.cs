using System;
using UnityEngine;
using UnityEngine.Audio;

public class SellAudioManager : MonoBehaviour
{
    // Optional default AudioMixerGroup to fall back on
    [SerializeField] private AudioMixerGroup defaultMixerGroup;

    private void Awake()
    {
        // Optional singleton-style protection
        if (FindObjectsOfType<SellAudioManager>().Length > 1)
        {
            // keep the first one, destroy extras
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Turret.OnTurretSold += HandleTurretSold;
    }

    private void OnDestroy()
    {
        Turret.OnTurretSold -= HandleTurretSold;
    }

    private void HandleTurretSold(Turret.TurretSoldEventArgs args)
    {
        if (args == null) return;
        if (args.clip == null)
        {
            Debug.LogWarning($"SellAudioManager: Received TurretSold event but clip is null (source: {args.source?.name}).");
            return;
        }

        PlaySellClipAt(args.position, args.clip, args.volume, args.mixerGroup);
    }

    /// <summary>
    /// Play the clip at a world position using a temporary GameObject+AudioSource so that playback is independent
    /// of other GameObjects. Copies mixer group if provided; otherwise uses defaultMixerGroup if assigned.
    /// </summary>
    private void PlaySellClipAt(Vector3 position, AudioClip clip, float volume, AudioMixerGroup mixerGroup)
    {
        GameObject tmp = new GameObject("SellSFX");
        tmp.transform.position = position;

        AudioSource src = tmp.AddComponent<AudioSource>();
        src.spatialBlend = 0f; // 2D sound (set to 1 for 3D)
        src.playOnAwake = false;
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);

        // Copy mixer group or use fallback
        src.outputAudioMixerGroup = (mixerGroup != null) ? mixerGroup : defaultMixerGroup;

        src.Play();
        Destroy(tmp, clip.length + 0.1f);
    }
}

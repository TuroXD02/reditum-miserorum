using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;  // Singleton instance

    [Header("Audio Settings")]
    public AudioClip backgroundMusic;       // Background music clip (assign in Inspector).
    [Range(0f, 1f)]
    public float targetVolume = 1f;         // Normal volume level.
    public float fadeInDuration = 15f;      // Duration of fade-in effect.
    public float fadeOutDuration = 5f;      // Duration of fade-out effect.
    public float loseVolume = 0.2f;         // Volume level after losing.
    public bool playOnAwake = true;         // Should music start on scene load?

    [Header("Mixer Settings")]
    public AudioMixer audioMixer;           // Reference to your AudioMixer asset.
    public string musicVolumeParameter = "Music"; // Exposed parameter name in the AudioMixer.

    private AudioSource audioSource;

    private void Awake()
    {
        // Implement the singleton pattern.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Persist across scenes.
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (backgroundMusic == null)
        {
            Debug.LogError("No background music assigned in AudioManager!");
            return;
        }

        // Configure the AudioSource.
        audioSource.clip = backgroundMusic;
        audioSource.loop = true;
        audioSource.volume = targetVolume;
        
        // Set the AudioSource's output group to the one that uses the "Music" parameter.
        if (audioMixer != null)
        {
            var groups = audioMixer.FindMatchingGroups("Music");
            if (groups.Length > 0)
            {
                audioSource.outputAudioMixerGroup = groups[0];
            }
            else
            {
                Debug.LogError("No matching AudioMixer group found for 'Music'");
            }

            float dB = (targetVolume > 0) ? Mathf.Log10(targetVolume) * 20 : -80f;
            audioMixer.SetFloat(musicVolumeParameter, dB);
        }
        else
        {
            Debug.LogWarning("AudioMixer is not assigned in AudioManager!");
        }

        if (playOnAwake)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// Sets the music volume in real time.
    /// </summary>
    /// <param name="vol">Volume value from 0 to 1</param>
    public void SetVolume(float vol)
    {
        targetVolume = Mathf.Clamp01(vol);
        if (audioSource != null)
        {
            audioSource.volume = targetVolume;
        }
        if (audioMixer != null)
        {
            float dB = (targetVolume > 0) ? Mathf.Log10(targetVolume) * 20 : -80f;
            audioMixer.SetFloat(musicVolumeParameter, dB);
        }
    }

    /// <summary>
    /// Resets the music by stopping, rewinding, setting volume to 0, and then fading in.
    /// </summary>
    public void ResetMusic()
    {
        if (audioSource != null)
        {
            StopAllCoroutines();
            audioSource.Stop();
            audioSource.time = 0f;
            audioSource.volume = 0f;
            audioSource.Play();
            StartCoroutine(FadeInMusic());
        }
        else
        {
            Debug.LogError("AudioSource is not assigned in AudioManager!");
        }
    }

    /// <summary>
    /// Gradually increases the volume from 0 to targetVolume over fadeInDuration seconds.
    /// </summary>
    private IEnumerator FadeInMusic()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float newVolume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            audioSource.volume = newVolume;
            if (audioMixer != null)
            {
                float dB = (newVolume > 0) ? Mathf.Log10(newVolume) * 20 : -80f;
                audioMixer.SetFloat(musicVolumeParameter, dB);
            }
            yield return null;
        }
        audioSource.volume = targetVolume;
        if (audioMixer != null)
        {
            float dB = (targetVolume > 0) ? Mathf.Log10(targetVolume) * 20 : -80f;
            audioMixer.SetFloat(musicVolumeParameter, dB);
        }
    }

    /// <summary>
    /// Gradually decreases the volume to loseVolume over fadeOutDuration seconds.
    /// Uses unscaled time so that it continues even when the game is paused.
    /// </summary>
    private IEnumerator FadeOutMusic()
    {
        float elapsed = 0f;
        float startVolume = audioSource.volume;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float newVolume = Mathf.Lerp(startVolume, loseVolume, elapsed / fadeOutDuration);
            audioSource.volume = newVolume;
            if (audioMixer != null)
            {
                float dB = (newVolume > 0) ? Mathf.Log10(newVolume) * 20 : -80f;
                audioMixer.SetFloat(musicVolumeParameter, dB);
            }
            yield return null;
        }
        audioSource.volume = loseVolume;
        if (audioMixer != null)
        {
            float dB = (loseVolume > 0) ? Mathf.Log10(loseVolume) * 20 : -80f;
            audioMixer.SetFloat(musicVolumeParameter, dB);
        }
    }

    /// <summary>
    /// Starts the fade-out effect when the game is lost.
    /// </summary>
    public void FadeToLoseVolume()
    {
        if (audioSource != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutMusic());
        }
        else
        {
            Debug.LogError("AudioSource is not assigned in AudioManager!");
        }
    }
}

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;  // Singleton instance

    [Header("Audio Settings")]
    public AudioClip backgroundMusic;       // Background music clip
    [Range(0f, 1f)]
    public float targetVolume = 1f;         // Normal volume level
    public float fadeInDuration = 15f;      // Duration of fade-in effect
    public float fadeOutDuration = 5f;      // Duration of fade-out effect
    public float loseVolume = 0.2f;         // Volume level after losing
    public bool playOnAwake = true;         // Should music start on scene load?

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

        if (playOnAwake)
        {
            audioSource.Play();
            
        }
    }

    /// <summary>
    /// Resets the music by stopping, rewinding, setting volume to 0, and fading in.
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
            audioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    /// <summary>
    /// Gradually decreases the volume to loseVolume over fadeOutDuration seconds.
    /// </summary>
    private IEnumerator FadeOutMusic()
    {
        float elapsed = 0f;
        float startVolume = audioSource.volume;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;  // Use unscaled time to continue during pause.
            audioSource.volume = Mathf.Lerp(startVolume, loseVolume, elapsed / fadeOutDuration);
            yield return null;
        }
        audioSource.volume = loseVolume;
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

using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    public float fadeInDuration = 15f;
    public float fadeOutDuration = 5f;
    public float loseVolume = 0.2f;
    public bool playOnAwake = true;

    [Header("Mixer Settings")]
    public AudioMixer audioMixer;
    public string musicVolumeParameter = "Music";

    private AudioSource audioSource;
    private const string VolumePrefKey = "MusicVolume";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
            Debug.LogError("No background music assigned!");
            return;
        }

        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        SetVolumeLinear(savedVolume);

        audioSource.clip = backgroundMusic;
        audioSource.loop = true;

        if (playOnAwake)
        {
            audioSource.Play();
        }
    }

    public void SetVolumeLinear(float linearValue)
    {
        linearValue = Mathf.Clamp01(linearValue);

        float dB;
        if (linearValue <= 0.0001f)
            dB = -80f;
        else
        {
            float adjusted = Mathf.Lerp(0.0001f, 1f, linearValue);
            dB = Mathf.Log10(adjusted) * 20f;
        }

        if (audioMixer != null)
        {
            audioMixer.SetFloat(musicVolumeParameter, dB);
        }

        if (audioSource != null)
        {
            audioSource.volume = linearValue;
        }

        PlayerPrefs.SetFloat(VolumePrefKey, linearValue);
        PlayerPrefs.Save();
    }

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
    }

    private IEnumerator FadeInMusic()
    {
        float elapsed = 0f;
        float targetVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float currentVolume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            SetVolumeLinear(currentVolume);
            yield return null;
        }

        SetVolumeLinear(targetVolume);
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float currentVolume = Mathf.Lerp(startVolume, loseVolume, elapsed / fadeOutDuration);
            SetVolumeLinear(currentVolume);
            yield return null;
        }

        SetVolumeLinear(loseVolume);
    }

    public void FadeToLoseVolume()
    {
        if (audioSource != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOutMusic());
        }
    }
}

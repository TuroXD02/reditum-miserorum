using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (outputMixerGroup != null)
            audioSource.outputAudioMixerGroup = outputMixerGroup;
    }

    public void PlaySFX()
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("SFXPlayer: No AudioClip assigned.");
        }
    }
}
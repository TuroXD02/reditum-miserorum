using UnityEngine;
using UnityEngine.Audio;

public class DeathEffect : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip deathAudioClip;
    [SerializeField, Range(0f, 1f)] private float audioVolume = 1f;
    [SerializeField] private AudioMixerGroup audioMixerGroup; // Optional override

    private AudioSource audioSource;
    private Animator animator;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = deathAudioClip;
        audioSource.volume = audioVolume;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D audio

        // If no mixer group assigned, try to find and assign "SFX" group
        if (audioMixerGroup == null)
        {
            AudioMixer mixer = Resources.Load<AudioMixer>("Audio/MainMixer"); // Must be placed in Resources/Audio
            if (mixer != null)
            {
                AudioMixerGroup[] groups = mixer.FindMatchingGroups("SFX");
                if (groups.Length > 0)
                {
                    audioMixerGroup = groups[0];
                }
            }
        }

        audioSource.outputAudioMixerGroup = audioMixerGroup;

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (deathAudioClip != null)
        {
            audioSource.Play();
        }

        if (animator != null)
        {
            animator.Play(0);
        }

        float audioLength = deathAudioClip != null ? deathAudioClip.length : 0f;
        float animationLength = animator != null ? GetAnimationLength() : 0f;

        float destroyDelay = Mathf.Max(audioLength, animationLength);
        Destroy(gameObject, destroyDelay);
    }

    private float GetAnimationLength()
    {
        if (animator == null) return 0f;

        AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            return clipInfo[0].clip.length;
        }

        return 0f;
    }
}

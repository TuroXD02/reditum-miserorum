using UnityEngine;

public class DeathEffect : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioClip deathAudioClip;
    [SerializeField, Range(0f, 1f)] private float audioVolume = 1f;

    private AudioSource audioSource;
    private Animator animator;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = deathAudioClip;
        audioSource.volume = audioVolume;
        audioSource.playOnAwake = false;

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

        // Wait for the longer duration between audio and animation before destroying
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
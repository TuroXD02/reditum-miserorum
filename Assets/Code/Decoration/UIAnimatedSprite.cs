using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimatedSprite : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private List<Sprite> frames;
    [SerializeField] private float frameRate = 10f;

    private Image uiImage;
    private Coroutine playCoroutine;

    private void Awake()
    {
        uiImage = GetComponent<Image>();
        if (frames.Count > 0)
            uiImage.sprite = frames[0]; // Initialize to first frame
    }

    public void PlayOnce()
    {
        if (frames.Count == 0 || uiImage == null) return;

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        playCoroutine = StartCoroutine(PlayAnimationOnce());
    }

    private IEnumerator PlayAnimationOnce()
    {
        for (int i = 0; i < frames.Count; i++)
        {
            uiImage.sprite = frames[i];
            yield return new WaitForSeconds(1f / frameRate);
        }

        // Reset to frame 1 after animation ends
        if (frames.Count > 0)
        {
            uiImage.sprite = frames[0];
        }
    }
}
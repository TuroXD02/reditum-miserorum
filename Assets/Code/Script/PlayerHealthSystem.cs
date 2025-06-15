using UnityEngine;
using TMPro;

public class PlayerHealthSystem : MonoBehaviour
{
    public TextMeshProUGUI txt_lifeCount;
    public int defaultHealthCount;
    public int healthCount;

    [Header("Lose Screen Reference")]
    [SerializeField] private GameObject loseScreen;

    [Header("Life Loss Animation")]
    [SerializeField] private UIAnimatedSprite lifeLossAnimation;

    public void Init()
    {
        healthCount = defaultHealthCount;
        txt_lifeCount.text = healthCount.ToString();

        if (loseScreen != null)
        {
            loseScreen.SetActive(false);
        }
    }

    public void LoseHealth()
    {
        if (healthCount < 1)
            return;

        healthCount--;
        txt_lifeCount.text = healthCount.ToString();

        // Play the hit animation once
        if (lifeLossAnimation != null)
        {
            lifeLossAnimation.PlayOnce();
        }

        CheckHealthCount();
    }

    private void CheckHealthCount()
    {
        if (healthCount < 1)
        {
            if (loseScreen != null)
            {
                loseScreen.SetActive(true);
            }

            if (AudioManager.instance != null)
            {
                AudioManager.instance.FadeToLoseVolume();
            }

            Invoke(nameof(PauseGame), 1f);
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
    }
}
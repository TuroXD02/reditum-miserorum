using UnityEngine;
using TMPro;

public class PlayerHealthSystem : MonoBehaviour
{
    public TextMeshProUGUI txt_lifeCount;
    public int defaultHealthCount;
    public int healthCount;

    [Header("Lose Screen Reference")]
    [SerializeField] private GameObject loseScreen;

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
        CheckHealthCount();
    }

    private void CheckHealthCount()
    {
        if (healthCount < 1)
        {
            Debug.Log("Game Over");

            if (loseScreen != null)
            {
                loseScreen.SetActive(true);
            }

            // Lower the volume before pausing.
            if (AudioManager.instance != null)
            {
                AudioManager.instance.FadeToLoseVolume();
            }

            // Slight delay to allow fade-out to be noticeable.
            Invoke(nameof(PauseGame), 1f);  // Delay game pause by 1 second
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // Now pause the game.
    }
}
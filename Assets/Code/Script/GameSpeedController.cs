using UnityEngine;
using UnityEngine.UI;

public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Control Buttons")]
    [SerializeField] private Button normalSpeedButton;
    [SerializeField] private Button doubleSpeedButton;
    [SerializeField] private Button quadSpeedButton;
    [SerializeField] private Button pauseButton;

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.gray;   // Highlighted color
    [SerializeField] private Color defaultColor = Color.white; // Normal color

    private void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => SetGameSpeed(0f));
        if (normalSpeedButton != null)
            normalSpeedButton.onClick.AddListener(() => SetGameSpeed(1f));
        if (doubleSpeedButton != null)
            doubleSpeedButton.onClick.AddListener(() => SetGameSpeed(2f));
        if (quadSpeedButton != null)
            quadSpeedButton.onClick.AddListener(() => SetGameSpeed(4f));
        
        UpdateHighlight(); // Set correct highlight at start
    }

    private void Update()
    {
        UpdateHighlight(); // Continuously ensure correct button is highlighted
    }

    private void SetGameSpeed(float speed)
    {
        Time.timeScale = speed;
        UpdateHighlight();
        Debug.Log($"Game speed set to {speed}x");
    }

    private void UpdateHighlight()
    {
        ResetAllButtons();

        if (Time.timeScale == 0f && pauseButton != null)
        {
            pauseButton.image.color = activeColor;
        }
        else if (Time.timeScale == 1f && normalSpeedButton != null)
        {
            normalSpeedButton.image.color = activeColor;
        }
        else if (Time.timeScale == 2f && doubleSpeedButton != null)
        {
            doubleSpeedButton.image.color = activeColor;
        }
        else if (Time.timeScale == 4f && quadSpeedButton != null)
        {
            quadSpeedButton.image.color = activeColor;
        }
        else
        {
            // Optional: handle weird timescales if needed
        }
    }

    private void ResetAllButtons()
    {
        if (normalSpeedButton != null)
            normalSpeedButton.image.color = defaultColor;
        if (doubleSpeedButton != null)
            doubleSpeedButton.image.color = defaultColor;
        if (quadSpeedButton != null)
            quadSpeedButton.image.color = defaultColor;
        if (pauseButton != null)
            pauseButton.image.color = defaultColor;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Control Buttons")]
    [SerializeField] private Button normalSpeedButton;  // Assign this button from the Inspector for 1x speed.
    [SerializeField] private Button doubleSpeedButton;  // Assign this button from the Inspector for 2x speed.
    [SerializeField] private Button quadSpeedButton;    // Assign this button from the Inspector for 4x speed.

    private void Start()
    {
        if(normalSpeedButton != null)
            normalSpeedButton.onClick.AddListener(SetNormalSpeed);
        if(doubleSpeedButton != null)
            doubleSpeedButton.onClick.AddListener(SetDoubleSpeed);
        if(quadSpeedButton != null)
            quadSpeedButton.onClick.AddListener(SetQuadSpeed);
    }

    public void SetNormalSpeed()
    {
        Time.timeScale = 1f;
        Debug.Log("Game speed set to 1x");
    }

    public void SetDoubleSpeed()
    {
        Time.timeScale = 2f;
        Debug.Log("Game speed set to 2x");
    }

    public void SetQuadSpeed()
    {
        Time.timeScale = 4f;
        Debug.Log("Game speed set to 4x");
    }
}
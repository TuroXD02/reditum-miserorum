using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameSpeedController : MonoBehaviour
{
    public static GameSpeedController Instance;

    [Header("Speed Control Buttons")]
    [SerializeField] private Button normalSpeedButton;
    [SerializeField] private Button doubleSpeedButton;
    [SerializeField] private Button quadSpeedButton;
    [SerializeField] private Button pauseButton;

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.gray;
    [SerializeField] private Color defaultColor = Color.white;

    private float lastSpeed = 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => { SetGameSpeed(0f); ClearUISelection(); });

        if (normalSpeedButton != null)
            normalSpeedButton.onClick.AddListener(() => { SetGameSpeed(1f); ClearUISelection(); });

        if (doubleSpeedButton != null)
            doubleSpeedButton.onClick.AddListener(() => { SetGameSpeed(2f); ClearUISelection(); });

        if (quadSpeedButton != null)
            quadSpeedButton.onClick.AddListener(() => { SetGameSpeed(4f); ClearUISelection(); });

        HighlightButtonForSpeed(Time.timeScale);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z)) SetGameSpeed(1f);
        if (Input.GetKeyDown(KeyCode.X)) SetGameSpeed(2f);
        if (Input.GetKeyDown(KeyCode.C)) SetGameSpeed(4f);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Mathf.Approximately(Time.timeScale, 0f))
                SetGameSpeed(lastSpeed <= 0f ? 1f : lastSpeed);
            else
                SetGameSpeed(0f);
        }
    }

    public void SetGameSpeed(float speed)
    {
        if (speed > 0f)
            lastSpeed = speed;

        Time.timeScale = speed;
        HighlightButtonForSpeed(speed);
        Debug.Log($"[GameSpeed] Time scale set to {speed}");
    }

    public void HighlightButtonForSpeed(float speed)
    {
        DeselectAllButtons();

        if (Mathf.Approximately(speed, 0f))
            pauseButton.image.color = activeColor;
        else if (Mathf.Approximately(speed, 1f))
            normalSpeedButton.image.color = activeColor;
        else if (Mathf.Approximately(speed, 2f))
            doubleSpeedButton.image.color = activeColor;
        else if (Mathf.Approximately(speed, 4f))
            quadSpeedButton.image.color = activeColor;
    }

    public void DeselectAllButtons()
    {
        if (pauseButton != null)
            pauseButton.image.color = defaultColor;
        if (normalSpeedButton != null)
            normalSpeedButton.image.color = defaultColor;
        if (doubleSpeedButton != null)
            doubleSpeedButton.image.color = defaultColor;
        if (quadSpeedButton != null)
            quadSpeedButton.image.color = defaultColor;
    }

    private void ClearUISelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public float GetLastSpeed()
    {
        return lastSpeed > 0f ? lastSpeed : 1f;
    }
}

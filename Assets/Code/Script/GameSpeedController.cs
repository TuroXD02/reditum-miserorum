using UnityEngine;
using UnityEngine.UI;

public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Control Buttons")]
    [SerializeField] private Button normalSpeedButton;
    [SerializeField] private Button doubleSpeedButton;
    [SerializeField] private Button quadSpeedButton;
    [SerializeField] private Button pauseButton;

    private Color defaultColor = Color.white;
    private Color selectedColor = new Color(0.6f, 0.6f, 0.6f); // A darker gray for selected button

    private void Start()
    {
        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => { SetPauseSpeed(); HighlightButton(pauseButton); });

        if (normalSpeedButton != null)
            normalSpeedButton.onClick.AddListener(() => { SetNormalSpeed(); HighlightButton(normalSpeedButton); });

        if (doubleSpeedButton != null)
            doubleSpeedButton.onClick.AddListener(() => { SetDoubleSpeed(); HighlightButton(doubleSpeedButton); });

        if (quadSpeedButton != null)
            quadSpeedButton.onClick.AddListener(() => { SetQuadSpeed(); HighlightButton(quadSpeedButton); });

        // Highlight normal speed by default
        HighlightButton(normalSpeedButton);
    }

    public void SetPauseSpeed() => Time.timeScale = 0f;
    public void SetNormalSpeed() => Time.timeScale = 1f;
    public void SetDoubleSpeed() => Time.timeScale = 2f;
    public void SetQuadSpeed() => Time.timeScale = 4f;

    private void HighlightButton(Button selected)
    {
        ResetAllButtons();

        if (selected != null)
        {
            var colors = selected.colors;
            colors.normalColor = selectedColor;
            colors.highlightedColor = selectedColor;
            colors.pressedColor = selectedColor;
            colors.selectedColor = selectedColor;
            selected.colors = colors;
        }
    }

    private void ResetAllButtons()
    {
        ResetButtonColor(pauseButton);
        ResetButtonColor(normalSpeedButton);
        ResetButtonColor(doubleSpeedButton);
        ResetButtonColor(quadSpeedButton);
    }

    private void ResetButtonColor(Button btn)
    {
        if (btn == null) return;

        var colors = btn.colors;
        colors.normalColor = defaultColor;
        colors.highlightedColor = defaultColor;
        colors.pressedColor = defaultColor;
        colors.selectedColor = defaultColor;
        btn.colors = colors;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;   // The entire pause menu panel.
    [SerializeField] private Slider musicSlider;        // Slider for music volume.
    [SerializeField] private Slider sfxSlider;          // Slider for SFX volume.
    [SerializeField] private Button resumeButton;       // Button to resume the game.
    [SerializeField] private Button pauseButton;        // Button to open the pause menu.
    [SerializeField] private Button mainMenuButton;       // Button to load the Main Menu.
    [SerializeField] private Button quitButton;           // Button to quit the game.

    [Header("Mixer Settings")]
    [SerializeField] private AudioMixer audioMixer;       // Reference to your AudioMixer asset.
    public string musicParameter = "Music";              // Exposed parameter for music.
    public string sfxParameter = "SFX";                  // Exposed parameter for SFX.

    private bool isPaused = false;

    private void Start()
    {
        // Ensure the pause panel is hidden at start.
        pausePanel.SetActive(false);

        // Initialize sliders using the AudioMixer settings.
        float currentMusicDB;
        if (audioMixer.GetFloat(musicParameter, out currentMusicDB))
        {
            float musicLinear = Mathf.InverseLerp(-80f, 0f, currentMusicDB);
            musicSlider.value = musicLinear;
            
        }
        else
        {
            Debug.LogWarning("[PauseMenu] Could not get music parameter from AudioMixer.");
        }

        float currentSfxDB;
        if (audioMixer.GetFloat(sfxParameter, out currentSfxDB))
        {
            float sfxLinear = Mathf.InverseLerp(-80f, 0f, currentSfxDB);
            sfxSlider.value = sfxLinear;
            
        }
        else
        {
            Debug.LogWarning("[PauseMenu] Could not get sfx parameter from AudioMixer.");
        }

        // Add listeners to sliders.
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);

        // Add listener to the resume button.
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        // Add listener to the pause button.
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePauseMenu);
        }

        // Add listener to the Main Menu button.
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(MainMenu);
        }

        // Add listener to the Quit button.
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    private void Update()
    {
        Debug.Log("[PauseMenu] Update is running...");
    
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[PauseMenu] Escape key detected!");
            TogglePauseMenu();
        }
    }


    private void OnMusicSliderChanged(float value)
    {
        Debug.Log("[PauseMenu] Music slider changed to: " + value);
        SetMusicVolume(value);
    }

    private void OnSFXSliderChanged(float value)
    {
        Debug.Log("[PauseMenu] SFX slider changed to: " + value);
        SetSFXVolume(value);
    }

    /// <summary>
    /// Sets the music volume based on slider value (0 to 1).
    /// Converts the value to decibels and sets the AudioMixer parameter.
    /// </summary>
    public void SetMusicVolume(float value)
    {
        float dB = Mathf.Lerp(-80f, 0f, value);
        audioMixer.SetFloat(musicParameter, dB);
        Debug.Log("[PauseMenu] Music parameter set to " + dB + " dB");
    }

    /// <summary>
    /// Sets the SFX volume based on slider value (0 to 1).
    /// Converts the value to decibels and sets the AudioMixer parameter.
    /// </summary>
    public void SetSFXVolume(float value)
    {
        float dB = Mathf.Lerp(-80f, 0f, value);
        audioMixer.SetFloat(sfxParameter, dB);
        Debug.Log("[PauseMenu] SFX parameter set to " + dB + " dB");
    }

    /// <summary>
    /// Toggles the pause menu on or off.
    /// When paused, Time.timeScale is set to 0.
    /// </summary>
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log("[PauseMenu] Pause menu toggled. isPaused: " + isPaused);
    }

    /// <summary>
    /// Resumes the game by closing the pause menu and setting Time.timeScale to 1.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        Debug.Log("[PauseMenu] Game resumed.");
    }

    /// <summary>
    /// Loads the Main Menu scene.
    /// </summary>
    public void MainMenu()
    {
        Time.timeScale = 1f; // Ensure timeScale is reset.
        Debug.Log("[PauseMenu] Loading Main Menu.");
        SceneManager.LoadScene("MainMenu"); // Change "MainMenu" to your actual main menu scene name.
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[PauseMenu] Quitting Game.");
        Application.Quit();
    }
}

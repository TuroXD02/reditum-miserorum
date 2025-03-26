using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;    // The pause menu panel.
    [SerializeField] private Slider musicSlider;         // Slider for music volume.
    [SerializeField] private Slider sfxSlider;           // Slider for SFX volume.
    [SerializeField] private Button resumeButton;        // Button to resume the game.
    [SerializeField] private Button mainMenuButton;      // Button to go to the main menu.
    [SerializeField] private Button quitButton;          // Button to quit the game.
    [SerializeField] private Button pauseButton;         // Extra button to open the pause menu.

    [Header("Mixer Settings")]
    [SerializeField] private AudioMixer audioMixer;        // Reference to your AudioMixer asset.
    public string musicParameter = "Music";               // Exposed parameter for music.
    public string sfxParameter = "SFX";                   // Exposed parameter for SFX.

    private bool isPaused = false;

    private void Start()
    {
        Debug.Log("[PauseMenu] Starting PauseMenu script.");
        // Ensure the pause panel starts inactive.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[PauseMenu] pausePanel is not assigned!");
        }

        // Initialize the music slider using the current AudioManager volume.
        if (AudioManager.instance != null)
        {
            musicSlider.value = AudioManager.instance.targetVolume;
        }
        else
        {
            Debug.LogWarning("[PauseMenu] AudioManager instance not found!");
        }
        
        // Initialize the SFX slider from AudioMixer.
        float currentSfxDB;
        if (audioMixer.GetFloat(sfxParameter, out currentSfxDB))
        {
            float sfxLinear = Mathf.InverseLerp(-80f, 0f, currentSfxDB);
            sfxSlider.value = sfxLinear;
        }
        else
        {
            Debug.LogWarning("[PauseMenu] Could not get SFX parameter from AudioMixer.");
        }

        // Add listeners.
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));
        if (quitButton != null)
            quitButton.onClick.AddListener(Application.Quit);
        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePauseMenu);
    }

    private void Update()
    {
        // Debug log to verify Update is being called.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("[PauseMenu] Escape key detected.");
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;
        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
        else
        {
            Debug.LogWarning("[PauseMenu] pausePanel is not assigned!");
        }
        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log("[PauseMenu] Pause toggled. isPaused = " + isPaused);
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        Time.timeScale = 1f;
        Debug.Log("[PauseMenu] Game resumed.");
    }

    public void SetMusicVolume(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetVolume(value);
        }
    }

    public void SetSFXVolume(float value)
    {
        float dB = Mathf.Lerp(-80f, 0f, value);
        audioMixer.SetFloat(sfxParameter, dB);
    }
}

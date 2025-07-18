using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button extraResumeButton; // NEW: second resume button
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button pauseButton;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParam = "Music";
    [SerializeField] private string sfxVolumeParam = "SFX";

    private bool isPaused = false;

    private void Start()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        InitializeSliders();
        AddListeners();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void InitializeSliders()
    {
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (musicSlider != null)
        {
            musicSlider.value = savedMusic;
            SetMusicVolume(savedMusic);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = savedSFX;
            SetSFXVolume(savedSFX);
        }
    }

    private void AddListeners()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (extraResumeButton != null) // Hook up second resume button
            extraResumeButton.onClick.AddListener(ResumeGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

        if (quitButton != null)
            quitButton.onClick.AddListener(Application.Quit);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePauseMenu);
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void SetMusicVolume(float value)
    {
        if (audioMixer != null)
        {
            float dB = Mathf.Approximately(value, 0f) ? -80f : Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(musicVolumeParam, dB);
        }

        PlayerPrefs.SetFloat("MusicVolume", Mathf.Clamp01(value));
        PlayerPrefs.Save();

        if (AudioManager.instance != null)
            AudioManager.instance.SetVolumeLinear(value);
    }

    public void SetSFXVolume(float value)
    {
        if (audioMixer != null)
        {
            float dB = Mathf.Approximately(value, 0f) ? -80f : Mathf.Log10(value) * 20f;
            audioMixer.SetFloat(sfxVolumeParam, dB);
        }

        PlayerPrefs.SetFloat("SFXVolume", Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public enum GameState { MainMenu, Playing, Paused, GameOver, Victory }

public class MenuManager : MonoBehaviour
{
    #region Singleton
    public static MenuManager instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    #endregion

    #region Fields
    [Header("State")]
    public GameState currentState;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject endMenuPanel;
    [SerializeField] private GameObject victoryMenuPanel;
    [SerializeField] private GameObject gameHUDPanel;

    [Header("Settings UI")]
    [SerializeField] private Slider mainMenuMusicSlider;
    [SerializeField] private Slider mainMenuSfxSlider;
    [SerializeField] private Slider pauseMusicSlider;
    [SerializeField] private Slider pauseSfxSlider;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI mainMenuHighScoreText;
    [SerializeField] private TextMeshProUGUI endScoreText;
    [SerializeField] private TextMeshProUGUI endHighScoreText;
    [SerializeField] private TextMeshProUGUI victoryScoreText;
    [SerializeField] private TextMeshProUGUI victoryHighScoreText;
    #endregion

    #region Unity Methods
    private void Start()
    {
        InitializeSettingsUI();
        SwitchState(GameState.MainMenu);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentState == GameState.Playing || currentState == GameState.Paused)
                TogglePause();
        }
    }
    #endregion

    #region State Management
    public void SwitchState(GameState newState)
    {
        currentState = newState;

        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        if (endMenuPanel) endMenuPanel.SetActive(false);
        if (victoryMenuPanel) victoryMenuPanel.SetActive(false);
        if (gameHUDPanel) gameHUDPanel.SetActive(false);

        switch (currentState)
        {
            case GameState.MainMenu:
                if (mainMenuPanel) mainMenuPanel.SetActive(true);
                UpdateMainMenuHighScore();
                UpdateSliderValues();
                Time.timeScale = 0f;
                break;

            case GameState.Playing:
                if (gameHUDPanel) gameHUDPanel.SetActive(true);
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
                UpdateSliderValues();
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                if (endMenuPanel) endMenuPanel.SetActive(true);
                ProcessEndGameScore(false);
                Time.timeScale = 0f;
                break;

            case GameState.Victory:
                if (victoryMenuPanel) victoryMenuPanel.SetActive(true);
                ProcessEndGameScore(true);
                Time.timeScale = 0f;
                break;
        }
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing) SwitchState(GameState.Paused);
        else if (currentState == GameState.Paused) SwitchState(GameState.Playing);
    }

    public void TriggerGameOver()
    {
        SwitchState(GameState.GameOver);

        // [Analytics] บันทึกเหตุการณ์ Game Over
        if (AnalyticsManager.instance != null && GameManager.instance != null)
            AnalyticsManager.instance.OnGameEnded(false, GameManager.instance.CurrentWave);
    }

    public void TriggerVictory()
    {
        SwitchState(GameState.Victory);

        // [Analytics] บันทึกเหตุการณ์ Victory
        if (AnalyticsManager.instance != null && GameManager.instance != null)
            AnalyticsManager.instance.OnGameEnded(true, GameManager.instance.CurrentWave);
    }
    #endregion

    #region UI Events (Buttons)
    public void OnPlayButton()
    {
        SwitchState(GameState.Playing);
        if (GameManager.instance != null) GameManager.instance.StartGame();

        // [Analytics] เริ่มเกมใหม่จากเมนูหลัก (ไม่ใช่การเล่นซ้ำ)
        if (AnalyticsManager.instance != null) AnalyticsManager.instance.OnGameStarted(false);
    }

    public void OnResumeButton() => SwitchState(GameState.Playing);
    public void OnPauseButton() => TogglePause();

    public void OnReplayButton()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.StopAndResetGame();
            SwitchState(GameState.Playing);
            GameManager.instance.StartGame();

            // [Analytics] กดปุ่มเล่นซ้ำ
            if (AnalyticsManager.instance != null) AnalyticsManager.instance.OnGameStarted(true);
        }
    }

    public void OnExitToMainButton()
    {
        if (GameManager.instance != null) GameManager.instance.StopAndResetGame();
        SwitchState(GameState.MainMenu);
    }

    /// <summary>
    /// ออกจากเกม (ใช้ผูกกับปุ่ม Quit ในหน้า Main Menu)
    /// </summary>
    public void OnQuitButton()
    {
        Debug.Log("Quitting Game...");

#if UNITY_EDITOR
        // หยุด Play Mode ถ้ากำลังรันทดสอบใน Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // ปิดแอปพลิเคชันเมื่อ Build เป็นตัวเกมจริง (PC/Mobile)
            // หมายเหตุ: สำหรับ WebGL คำสั่งนี้อาจจะไม่มีผลเบราว์เซอร์บางตัว ซึ่งเป็นข้อจำกัดของแพลตฟอร์ม WebGL ปกติ
            Application.Quit();
#endif
    }
    #endregion

    #region Settings Logic
    private void InitializeSettingsUI()
    {
        if (mainMenuMusicSlider) mainMenuMusicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (mainMenuSfxSlider) mainMenuSfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (pauseMusicSlider) pauseMusicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (pauseSfxSlider) pauseSfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void UpdateSliderValues()
    {
        if (FindAnyObjectByType<AudioListener>() == null) return;

        if (AudioManager.instance == null) return;
        float music = AudioManager.instance.GetMusicVolume();
        float sfx = AudioManager.instance.GetSFXVolume();
        if (mainMenuMusicSlider) mainMenuMusicSlider.value = music;
        if (mainMenuSfxSlider) mainMenuSfxSlider.value = sfx;
        if (pauseMusicSlider) pauseMusicSlider.value = music;
        if (pauseSfxSlider) pauseSfxSlider.value = sfx;
    }

    public void OnMusicVolumeChanged(float value)
    {
         if (AudioManager.instance != null) AudioManager.instance.SetMusicVolume(value);
    }

    public void OnSFXVolumeChanged(float value)
    {
         if (AudioManager.instance != null) AudioManager.instance.SetSFXVolume(value);
    }
    #endregion

    #region Score Logic
    private void UpdateMainMenuHighScore()
    {
        if (mainMenuHighScoreText != null)
            mainMenuHighScoreText.text = "Best\nScore\n" + PlayerPrefs.GetInt("HighScore", 0);
    }

    private void ProcessEndGameScore(bool isVictory)
    {
        int currentScore = (Character.instance != null) ? Character.instance.GetScore() : 0;
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        string scoreStr = "" + currentScore;
        string bestStr = "Best " + highScore;

        if (isVictory)
        {
            if (victoryScoreText) victoryScoreText.text = scoreStr;
            if (victoryHighScoreText) victoryHighScoreText.text = bestStr;
        }
        else
        {
            if (endScoreText) endScoreText.text = scoreStr;
            if (endHighScoreText) endHighScoreText.text = bestStr;
        }
    }
    #endregion
}
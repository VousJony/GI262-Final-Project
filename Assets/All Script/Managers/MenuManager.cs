using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro; // จำเป็นต้องใช้สำหรับ TextMeshProUGUI

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory
}

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    [Header("Game State")]
    public GameState currentState;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject endMenuPanel; // Game Over Panel
    [SerializeField] private GameObject victoryMenuPanel; // Victory Panel
    [SerializeField] private GameObject gameHUDPanel;

    [Header("Score UI References")]
    [Tooltip("Text แสดง HighScore ในหน้า Main Menu")]
    [SerializeField] private TextMeshProUGUI mainMenuHighScoreText;

    [Tooltip("Text แสดง Score ปัจจุบันในหน้า Game Over")]
    [SerializeField] private TextMeshProUGUI endScoreText;
    [Tooltip("Text แสดง HighScore ในหน้า Game Over")]
    [SerializeField] private TextMeshProUGUI endHighScoreText;

    [Tooltip("Text แสดง Score ปัจจุบันในหน้า Victory")]
    [SerializeField] private TextMeshProUGUI victoryScoreText;
    [Tooltip("Text แสดง HighScore ในหน้า Victory")]
    [SerializeField] private TextMeshProUGUI victoryHighScoreText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SwitchState(GameState.MainMenu);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            SwitchState(GameState.Paused);
        }
        else if (currentState == GameState.Paused)
        {
            SwitchState(GameState.Playing);
        }
    }

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
                UpdateMainMenuHighScore(); // อัปเดต HighScore หน้าแรก
                Time.timeScale = 0f;
                break;

            case GameState.Playing:
                if (gameHUDPanel) gameHUDPanel.SetActive(true);
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                if (endMenuPanel) endMenuPanel.SetActive(true);
                ProcessEndGameScore(false); // คำนวณและแสดงคะแนน (แพ้)
                Time.timeScale = 0f;
                break;

            case GameState.Victory:
                if (victoryMenuPanel) victoryMenuPanel.SetActive(true);
                ProcessEndGameScore(true); // คำนวณและแสดงคะแนน (ชนะ)
                Time.timeScale = 0f;
                break;
        }
    }

    // --- Score Logic ---

    private void UpdateMainMenuHighScore()
    {
        // โหลด HighScore จากเครื่อง (ถ้าไม่มีจะได้ 0)
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (mainMenuHighScoreText != null)
        {
            mainMenuHighScoreText.text = "Best\nScore\n" + highScore.ToString();
        }
    }

    private void ProcessEndGameScore(bool isVictory)
    {
        // 1. ดึงคะแนนปัจจุบันจาก Character
        int currentScore = 0;
        if (Character.instance != null)
        {
            currentScore = Character.instance.GetScore();
        }

        // 2. โหลด HighScore เดิม
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        // 3. เช็กว่าทำลายสถิติไหม
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore); // บันทึกลงเครื่อง
            PlayerPrefs.Save(); // ยืนยันการบันทึกทันที
        }

        // 4. อัปเดต UI ตามสถานะ (ชนะ/แพ้)
        if (isVictory)
        {
            if (victoryScoreText != null) victoryScoreText.text = "" +currentScore;
            if (victoryHighScoreText != null) victoryHighScoreText.text = "Best " + highScore;
        }
        else
        {
            if (endScoreText != null) endScoreText.text = "" + currentScore;
            if (endHighScoreText != null) endHighScoreText.text = "Best " + highScore;
        }
    }

    // --- Button Events ---

    public void OnPauseButton()
    {
        if (currentState == GameState.Playing)
        {
            SwitchState(GameState.Paused);
        }
    }

    public void OnPlayButton()
    {
        SwitchState(GameState.Playing);
        if (GameManager.instance != null) GameManager.instance.StartGame();
    }

    public void OnResumeButton()
    {
        SwitchState(GameState.Playing);
    }

    public void OnExitToMainButton()
    {
        if (GameManager.instance != null) GameManager.instance.StopAndResetGame();
        SwitchState(GameState.MainMenu);
    }

    public void OnReplayButton()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.StopAndResetGame();
            SwitchState(GameState.Playing);
            GameManager.instance.StartGame();
        }
    }

    public void TriggerGameOver()
    {
        SwitchState(GameState.GameOver);
    }

    public void TriggerVictory()
    {
        SwitchState(GameState.Victory);
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel;
    public Button settingsButton;
    public Button restartButton;
    public Button pauseButton;
    public Button exitButton;
    public Button continueButton;

    private bool isPaused = false;

    void Start()
    {
        settingsPanel.SetActive(false);

        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        restartButton.onClick.AddListener(RestartGame);
        pauseButton.onClick.AddListener(TogglePause);
        exitButton.onClick.AddListener(ExitGame);
        continueButton.onClick.AddListener(ContinueGame);
    }

    void ToggleSettingsPanel()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
    }

    void ExitGame()
    {
        Time.timeScale = 1; // Đảm bảo unpause nếu đang pause
        SceneManager.LoadScene("MainMenu"); // Load scene theo tên
    }

    void ContinueGame()
    {
        if (isPaused)
        {
            isPaused = false;
            Time.timeScale = 1;
            settingsPanel.SetActive(false); // Ẩn bảng settings nếu muốn
        }
    }
}

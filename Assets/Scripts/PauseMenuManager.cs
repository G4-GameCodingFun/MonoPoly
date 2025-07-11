using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameplayMenuPanel;        
    public Button pauseButton;
    public Button continueButton;
    public Button backToMenuButton;
    public Toggle musicToggle;
    public Toggle sfxToggle;

    private void Start()
    {
        musicToggle.onValueChanged.RemoveAllListeners();
        sfxToggle.onValueChanged.RemoveAllListeners();

        // 2) Đồng bộ giá trị ban đầu
        if (AudioManager.Instance != null)
        {
            musicToggle.isOn = !AudioManager.Instance.bgmSource.mute;

            sfxToggle.isOn = !AudioManager.Instance.sfxSource.mute;
        }

        // 3) Gán lại listener sau khi set isOn
        musicToggle.onValueChanged.AddListener(ToggleMusic);
        sfxToggle.onValueChanged.AddListener(ToggleSFX);
    }


    public void OpenPauseMenu()
    {
        Debug.Log("Pause button clicked!");
        gameplayMenuPanel.SetActive(true);
        pauseButton.gameObject.SetActive(false);  // Ẩn nút pause
        Time.timeScale = 0f;
    }

    public void ClosePauseMenu()
    {
        gameplayMenuPanel.SetActive(false);
        pauseButton.gameObject.SetActive(true);   // Hiện lại nút pause
        Time.timeScale = 1f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Đảm bảo resume game
        SceneManager.LoadScene("MainMenu");
    }

    public void ToggleMusic(bool isOn)
    {
        if (AudioManager.Instance == null) return;

        // isOn = true  ➜ bật nhạc
        // isOn = false ➜ tắt nhạc
        AudioManager.Instance.SetMusicEnabled(isOn);
    }


    public void ToggleSFX(bool isOn)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXEnabled(isOn);
    }
}

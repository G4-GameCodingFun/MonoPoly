using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject TutorialsPanel;
    public GameObject SettingsPanel;

    [Header("Buttons")]
    public Button PlayButton;
    public Button TutorialsButton;
    public Button SettingsButton;
    public Button ExitButton;
    public Button CloseTutorialsButton;
    public Button CloseSettingsButton;

    [Header("Toggles")]
    public Toggle MusicToggle;
    public Toggle SoundToggle;

    void Start()
    {
        // Gán các sự kiện OnClick
        PlayButton.onClick.AddListener(PlayGame);
        TutorialsButton.onClick.AddListener(ShowTutorials);
        CloseTutorialsButton.onClick.AddListener(CloseTutorials);
        SettingsButton.onClick.AddListener(ShowSettings);
        CloseSettingsButton.onClick.AddListener(CloseSettings);
        ExitButton.onClick.AddListener(ExitGame);

        // Gán sự kiện Toggle và Slider
        MusicToggle.onValueChanged.AddListener(delegate { ToggleMusic(); });
        SoundToggle.onValueChanged.AddListener(delegate { ToggleSound(); });

        // Ẩn panel ban đầu
        TutorialsPanel.SetActive(false);
        SettingsPanel.SetActive(false);

        // Khởi tạo trạng thái Toggle và Slider từ AudioManager (nếu có)
        if (AudioManager.Instance != null)
        {
            MusicToggle.isOn = !AudioManager.Instance.bgmSource.mute;
        }
    }

    public void PlayGame()
    {
        Debug.Log("Play Game Clicked");
        SceneManager.LoadScene("GamePlay"); // Đổi tên thành scene thực tế
    }

    public void ShowTutorials()
    {
        TutorialsPanel.SetActive(true);
        SettingsButton.gameObject.SetActive(false);
        ExitButton.gameObject.SetActive(false);
    }

    public void CloseTutorials()
    {
        TutorialsPanel.SetActive(false);
        SettingsButton.gameObject.SetActive(true);
        ExitButton.gameObject.SetActive(true);
    }

    public void ShowSettings()
    {
        SettingsPanel.SetActive(true);
        ExitButton.gameObject.SetActive(false);
    }

    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
        ExitButton.gameObject.SetActive(true);
    }

    public void ToggleMusic()
    {
        Debug.Log("Music is " + (MusicToggle.isOn ? "ON" : "OFF"));

        if (AudioManager.Instance != null)
            AudioManager.Instance.MuteMusic(!MusicToggle.isOn);
    }

    public void OnMusicVolumeChanged(float volume)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(volume);
    }

    public void ToggleSound()
    {
        bool enabled = SoundToggle.isOn;
        Debug.Log("Sound is " + (enabled ? "ON" : "OFF"));

        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXEnabled(enabled);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game Clicked");
        Application.Quit();
    }
}

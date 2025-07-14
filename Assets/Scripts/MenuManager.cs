using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject TutorialsPanel;
    public GameObject SettingsPanel;
    public GameObject MultiplayerPanel; // Thêm panel cho multiplayer options nếu cần

    [Header("Buttons")]
    public Button TutorialsButton;
    public Button SettingsButton;
    public Button ExitButton;
    public Button CloseTutorialsButton;
    public Button CloseSettingsButton;
    public Button PlayGameButton;

    [Header("Toggles")]
    public Toggle MusicToggle;
    public Toggle SoundToggle;

    void Start()
    {
        // Gán sự kiện OnClick - kiểm tra null trước khi gán
        if (TutorialsButton != null) TutorialsButton.onClick.AddListener(ShowTutorials);
        if (CloseTutorialsButton != null) CloseTutorialsButton.onClick.AddListener(CloseTutorials);
        if (SettingsButton != null) SettingsButton.onClick.AddListener(ShowSettings);
        if (CloseSettingsButton != null) CloseSettingsButton.onClick.AddListener(CloseSettings);
        if (ExitButton != null) ExitButton.onClick.AddListener(ExitGame);
        if (PlayGameButton != null) PlayGameButton.onClick.AddListener(PlayGame);

        // Gán sự kiện Toggle - kiểm tra null trước khi gán
        if (MusicToggle != null) MusicToggle.onValueChanged.AddListener(delegate { ToggleMusic(); });
        if (SoundToggle != null) SoundToggle.onValueChanged.AddListener(delegate { ToggleSound(); });

        // Ẩn panel ban đầu
        TutorialsPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        if (MultiplayerPanel != null) MultiplayerPanel.SetActive(false);

        // Khởi tạo trạng thái Toggle từ AudioManager
        if (AudioManager.Instance != null)
        {
            if (MusicToggle != null) MusicToggle.isOn = !AudioManager.Instance.bgmSource.mute;
            if (SoundToggle != null) SoundToggle.isOn = true; // Giả sử sound mặc định ON
        }
    }

    public void ShowTutorials()
    {
        TutorialsPanel.SetActive(true);
        if (SettingsPanel != null) SettingsPanel.SetActive(false);
        if (ExitButton != null) ExitButton.gameObject.SetActive(false);
        if (SettingsButton != null) SettingsButton.gameObject.SetActive(false);
    }

    public void CloseTutorials()
    {
        TutorialsPanel.SetActive(false);
        if (SettingsButton != null) SettingsButton.gameObject.SetActive(true);
        // Hiện lại ExitButton nếu không có panel nào khác đang mở
        if ((SettingsPanel == null || !SettingsPanel.activeSelf) && ExitButton != null)
            ExitButton.gameObject.SetActive(true);
    }

    public void ShowSettings()
    {
        SettingsPanel.SetActive(true);
        if (ExitButton != null) ExitButton.gameObject.SetActive(false);
    }

    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
        // Hiện lại ExitButton nếu không có panel nào khác đang mở
        if ((TutorialsPanel == null || !TutorialsPanel.activeSelf) && ExitButton != null)
            ExitButton.gameObject.SetActive(true);
    }

    public void ToggleMusic()
    {
        Debug.Log("Music is " + (MusicToggle.isOn ? "ON" : "OFF"));
        if (AudioManager.Instance != null)
            AudioManager.Instance.MuteMusic(!MusicToggle.isOn);
    }

    public void ToggleSound()
    {
        Debug.Log("Sound is " + (SoundToggle.isOn ? "ON" : "OFF"));
        //if (AudioManager.Instance != null)
        //    AudioManager.Instance.MuteSound(!SoundToggle.isOn); // Giả sử có method MuteSound
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game Clicked");
        Application.Quit();
    }

    public void PlayGame()
    {
        Debug.Log("Play Game Clicked");
        SceneManager.LoadScene("CreateAccountScene");
    }
}

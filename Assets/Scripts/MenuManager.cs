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
    public Button SinglePlayerButton; // Thêm button cho Single Player
    public Button HostButton; // Button Host Multiplayer
    public Button JoinButton; // Button Join Multiplayer
    public Button TutorialsButton;
    public Button SettingsButton;
    public Button ExitButton;
    public Button CloseTutorialsButton;
    public Button CloseSettingsButton;

    [Header("Toggles")]
    public Toggle MusicToggle;
    public Toggle SoundToggle;

    [Header("Sliders")]
    public Slider MusicSlider;

    [Header("Input Fields")]
    public InputField IpInput; // Để nhập IP cho Join nếu cần

    void Start()
    {
        // Gán sự kiện OnClick
        SinglePlayerButton.onClick.AddListener(SinglePlayer);
        HostButton.onClick.AddListener(HostGame);
        JoinButton.onClick.AddListener(JoinGame);
        TutorialsButton.onClick.AddListener(ShowTutorials);
        CloseTutorialsButton.onClick.AddListener(CloseTutorials);
        SettingsButton.onClick.AddListener(ShowSettings);
        CloseSettingsButton.onClick.AddListener(CloseSettings);
        ExitButton.onClick.AddListener(ExitGame);

        // Gán sự kiện Toggle và Slider
        MusicToggle.onValueChanged.AddListener(delegate { ToggleMusic(); });
        SoundToggle.onValueChanged.AddListener(delegate { ToggleSound(); });
        MusicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        // Ẩn panel ban đầu
        TutorialsPanel.SetActive(false);
        SettingsPanel.SetActive(false);
        if (MultiplayerPanel != null) MultiplayerPanel.SetActive(false);

        // Khởi tạo trạng thái Toggle và Slider từ AudioManager
        if (AudioManager.Instance != null)
        {
            MusicSlider.value = AudioManager.Instance.bgmSource.volume;
            MusicToggle.isOn = !AudioManager.Instance.bgmSource.mute;
            SoundToggle.isOn = true; // Giả sử sound mặc định ON
        }
    }

    public void SinglePlayer()
    {
        Debug.Log("Single Player Clicked");
        // Giả sử single player là host local với bots
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene("GamePlay"); // Load scene game
    }

    public void HostGame()
    {
        Debug.Log("Host Multiplayer Clicked");
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene("GamePlay"); // Load scene game
    }

    public void JoinGame()
    {
        Debug.Log("Join Multiplayer Clicked");
        // Giả sử có input IP, set transport IP
        if (IpInput != null && !string.IsNullOrEmpty(IpInput.text))
        {
            // Set transport address, ví dụ UnityTransport
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = IpInput.text;
            }
        }
        NetworkManager.Singleton.StartClient();
        SceneManager.LoadScene("GamePlay"); // Load scene game
    }

    public void ShowTutorials()
    {
        TutorialsPanel.SetActive(true);
    }

    public void CloseTutorials()
    {
        TutorialsPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        SettingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        SettingsPanel.SetActive(false);
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
        Debug.Log("Sound is " + (SoundToggle.isOn ? "ON" : "OFF"));
        //if (AudioManager.Instance != null)
        //    AudioManager.Instance.MuteSound(!SoundToggle.isOn); // Giả sử có method MuteSound
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game Clicked");
        Application.Quit();
    }
}
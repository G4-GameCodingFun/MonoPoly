using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayWithBotController : MonoBehaviour
{
    public Button playWithBotButton;

    void Start()
    {
        playWithBotButton.onClick.AddListener(OnPlayWithBotClicked);
    }

    void OnPlayWithBotClicked()
    {
        // Tạo session cho bot (ví dụ: lưu vào PlayerPrefs)
        PlayerPrefs.SetString("GameMode", "Bot");
        PlayerPrefs.SetString("PlayerName", "BotPlayer");
        PlayerPrefs.Save();
        Debug.Log("Button clicked");

        // Chuyển sang scene GamePlay
        SceneManager.LoadScene("GamePLay");
    }
} 
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo : MonoBehaviour
{
    public Button playersInfo; // Button để hiển thị thông tin
    public Button outButton;   // Button để đóng panel
    public GameObject PlayerInfoPanel; // Panel chứa thông tin
    public TMP_Text[] playerInfoTexts; // Text để hiển thị thông tin
    public PlayerController[] players; // Mảng các PlayerController
    public GameManager gameManager;

    void Start()
    {
        if (gameManager != null && gameManager.players != null)
        {
            players = gameManager.players.ToArray();
        }

        // Gắn sự kiện cho button playersInfo
        if (playersInfo != null)
        {
            playersInfo.onClick.AddListener(ShowPlayerInfo);
        }

        // Gắn sự kiện cho button outButton
        if (outButton != null)
        {
            outButton.onClick.AddListener(HidePlayerInfo);
        }

        // Ẩn panel ban đầu
        if (PlayerInfoPanel != null)
        {
            PlayerInfoPanel.SetActive(false);
        }
    }

    void ShowPlayerInfo()
    {
        if (PlayerInfoPanel == null || playerInfoTexts == null || playerInfoTexts.Length == 0 ||
            players == null || players.Length == 0 || players.Length != playerInfoTexts.Length)
        {
            Debug.LogError("Một hoặc nhiều tham chiếu trong PlayerInfo là null hoặc không khớp số lượng!");
            return;
        }

        // Cập nhật thông tin tiền cho từng nhân vật
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && playerInfoTexts[i] != null && !string.IsNullOrEmpty(players[i].playerName))
            {
                playerInfoTexts[i].text = $"Player: {players[i].playerName}\nMoney: {players[i].money}$";
                Debug.Log($"Debug - {players[i].playerName} has money: {players[i].money}");
            }
            else
            {
                Debug.LogWarning($"Player or Text at index {i} is null or has no name!");
            }
        }
        PlayerInfoPanel.SetActive(true); // Hiển thị panel
    }

    void HidePlayerInfo()
    {
        if (PlayerInfoPanel != null)
        {
            PlayerInfoPanel.SetActive(false); // Ẩn panel
        }
    }
}

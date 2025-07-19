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
            Debug.LogWarning("Một hoặc nhiều tham chiếu trong PlayerInfo là null hoặc không khớp số lượng!");
            return;
        }

        // Cập nhật thông tin tiền cho từng nhân vật
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && playerInfoTexts[i] != null && !string.IsNullOrEmpty(players[i].playerName))
            {
                // Kiểm tra trạng thái phá sản
                if (players[i].isBankrupt)
                {
                    // Player đã phá sản - hiển thị màu đỏ
                    string moneyText = players[i].money < 0 ? $"{players[i].money}$" : "0$";
                    playerInfoTexts[i].text = $"Player: {players[i].playerName}\nMoney: {moneyText} (PHÁ SẢN)";
                    playerInfoTexts[i].color = Color.red;
                }
                else if (players[i].money < 0)
                {
                    // Player có tiền âm nhưng chưa phá sản (có thể bán tài sản)
                    playerInfoTexts[i].text = $"Player: {players[i].playerName}\nMoney: {players[i].money}$ (THIẾU TIỀN)";
                    playerInfoTexts[i].color = new Color(1f, 0.5f, 0f); // Màu cam
                }
                else
                {
                    // Player bình thường
                    playerInfoTexts[i].text = $"Player: {players[i].playerName}\nMoney: {players[i].money}$";
                    playerInfoTexts[i].color = Color.white; // Reset về màu trắng
                }
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

    /// <summary>
    /// Hàm public để refresh thông tin player (có thể gọi từ GameManager)
    /// </summary>
    public void RefreshPlayerInfo()
    {
        Debug.Log("🔄 Refreshing PlayerInfo...");
        
        // Cập nhật lại danh sách players từ GameManager
        if (gameManager != null && gameManager.players != null)
        {
            players = gameManager.players.ToArray();
            Debug.Log($"📊 Cập nhật danh sách players: {players.Length} players");
            
            // Log thông tin từng player để debug
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    Debug.Log($"👤 Player {i}: {players[i].playerName} - Money: {players[i].money}$ - isBankrupt: {players[i].isBankrupt}");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager hoặc players list là null!");
        }
        
        // Nếu panel đang hiển thị, cập nhật lại thông tin
        if (PlayerInfoPanel != null && PlayerInfoPanel.activeSelf)
        {
            Debug.Log("📋 Panel đang hiển thị, cập nhật thông tin...");
            ShowPlayerInfo();
        }
        else
        {
            Debug.Log("📋 Panel không hiển thị, không cần cập nhật");
        }
    }
}

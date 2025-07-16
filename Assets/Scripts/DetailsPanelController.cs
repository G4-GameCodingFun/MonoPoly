using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DetailsPanelController : MonoBehaviour
{
    [Header("Panel & UI Elements")]
    public GameObject panel; // Chính là DetailsPanel
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text rentText;
    public Button buyButton;
    public Button sellButton; // Thêm nút bán
    public GameManager gameManager;
    public Button quitButton;
    public TMP_Text ownerText; // Hiển thị chủ sở hữu

    private PropertyTile currentTile;
    private PlayerController currentPlayer;

    /// <summary>
    /// Hiển thị panel với thông tin ô đất và player hiện tại
    /// </summary>
    /// <param name="tile">PropertyTile cần hiển thị</param>
    /// <param name="player">Player hiện tại</param>
    public void Show(PropertyTile tile, PlayerController player)
    {
        currentTile = tile;
        currentPlayer = player;
        panel.SetActive(true);
        Debug.Log("Show DetailsPanel: " + (tile != null ? tile.data.provinceName : "null"));

        // Gán sự kiện cho QuitButton
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(() => {
                Hide();
                if (gameManager != null) gameManager.isWaitingForPlayerAction = false;
            });
        }
        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellProperty);
        }

        if (tile != null && tile.data != null)
        {
            nameText.text = tile.data.provinceName;
            priceText.text = "Giá mua đứt: " + tile.data.purchasePrice + "$";
            rentText.text = "Giá thuê (phạt): " + tile.data.rentByHouse[0] + "$";
            if (ownerText != null)
                ownerText.text = "Chủ sở hữu: " + (tile.owner != null ? tile.owner.playerName : "Chưa có");
        }
        else
        {
            nameText.text = "";
            priceText.text = "";
            rentText.text = "";
            if (ownerText != null) ownerText.text = "";
        }

        // Điều kiện mở nút Mua/Bán
        bool isPlayerOnTile = player != null && tile != null && player.currentTileIndex == tile.transform.GetSiblingIndex();
        bool canBuy = tile != null && tile.owner == null && player != null && player.CanPay(tile.GetPrice()) && isPlayerOnTile;
        bool canSell = tile != null && tile.owner == player;
        if (buyButton != null) buyButton.gameObject.SetActive(canBuy);
        if (sellButton != null) sellButton.gameObject.SetActive(canSell);
        if (!canBuy && !canSell)
        {
            if (buyButton != null) buyButton.gameObject.SetActive(false);
            if (sellButton != null) sellButton.gameObject.SetActive(false);
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyProperty);
        }

        // --- Logic tự động mua cho bot (nếu cần giữ lại) ---
        if (player != null && player.isBot && canBuy)
        {
            player.BuyProperty(tile);
            if (gameManager != null)
                gameManager.ShowStatus($"{player.playerName} (Bot) tự động mua {tile.tileName}");
            Hide();
            if (gameManager != null) gameManager.isWaitingForPlayerAction = false;
            return;
        }
        // --- Kết thúc logic bot ---
    }

    public void Hide()
    {
        if (panel != null && panel.activeSelf)
        {
            panel.SetActive(false);
            if (gameManager != null)
            {
                gameManager.isWaitingForPlayerAction = false; // Đặt lại trạng thái
            }
        }
    }

    /// <summary>
    /// Xử lý logic mua đất khi bấm nút Mua
    /// </summary>
    void BuyProperty()
    {
        Debug.Log("Đã bấm nút Mua");
        if (currentPlayer != null && currentTile != null && currentPlayer.CanPay(currentTile.GetPrice()))
        {
            currentPlayer.BuyProperty(currentTile);
            Debug.Log("Đã mua đất: " + currentTile.data.provinceName);
            Hide();
            if (gameManager != null) gameManager.isWaitingForPlayerAction = false;
        }
        else
        {
            Debug.Log("Không đủ tiền hoặc lỗi khi mua đất!");
        }
    }

    void SellProperty()
    {
        Debug.Log("Đã bấm nút Bán");
        if (currentPlayer != null && currentTile != null && currentTile.owner == currentPlayer)
        {
            currentPlayer.SellProperty(currentTile);
            Debug.Log("Đã bán đất: " + currentTile.data.provinceName);
            Hide();
            if (gameManager != null) gameManager.isWaitingForPlayerAction = false;
        }
        else
        {
            Debug.Log("Không phải chủ sở hữu hoặc lỗi khi bán đất!");
        }
    }

    // Đảm bảo hàm QuitPanel chỉ gọi Hide và reset trạng thái
    public void QuitPanel()
    {
        Debug.Log("Đã bấm nút Quit");
        Hide();
        if (gameManager != null)
        {
            gameManager.isWaitingForPlayerAction = false;
        }
    }

    private void Update()
    {
        if (panel != null && panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Hide();
            if (gameManager != null) gameManager.isWaitingForPlayerAction = false;
        }
    }
}
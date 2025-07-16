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
    public void Show(PropertyTile tile, PlayerController player = null)
    {
        currentTile = tile;
        // Lấy player hiện tại từ GameManager nếu không truyền vào
        currentPlayer = player ?? (GameManager.Instance != null ? GameManager.Instance.players[GameManager.Instance.currentPlayerIndex] : null);
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
            priceText.text = "Giá mua đứt: " + tile.GetPrice() + "$";
            rentText.text = "Giá thuê (phạt): " + tile.GetRent() + "$";
            if (ownerText != null)
            {
                if (tile.owner != null)
                {
                    ownerText.text = $"Chủ sở hữu: {tile.owner.playerName}";
                    // Thêm màu sắc để phân biệt chủ sở hữu
                    if (tile.owner == currentPlayer)
                    {
                        ownerText.color = Color.green; // Màu xanh cho tài sản của mình
                    }
                    else
                    {
                        ownerText.color = Color.red; // Màu đỏ cho tài sản của người khác
                    }
                }
                else
                {
                    ownerText.text = "Chủ sở hữu: Chưa có";
                    ownerText.color = Color.white;
                }
            }
        }
        else
        {
            nameText.text = "";
            priceText.text = "";
            rentText.text = "";
            if (ownerText != null) ownerText.text = "";
        }

        // Điều kiện mở nút Mua/Bán
        bool isPlayerOnTile = currentPlayer != null && tile != null && currentPlayer.currentTileIndex == tile.transform.GetSiblingIndex();
        
        // Kiểm tra các trạng thái đặc biệt
        bool canBuyNormally = tile != null && tile.owner == null && currentPlayer != null && currentPlayer.CanPay(tile.GetPrice()) && isPlayerOnTile;
        bool cannotBuyDueToCard = currentPlayer != null && currentPlayer.cannotBuyNextTurn;
        bool canBuyWithDiscount = currentPlayer != null && currentPlayer.canBuyDiscountProperty;
        
        bool canBuy = canBuyNormally && !cannotBuyDueToCard;
        
        // Nút bán chỉ hiện khi player là chủ sở hữu (so sánh object thay vì string)
        bool canSell = tile != null && tile.owner != null && currentPlayer != null && tile.owner == currentPlayer;

        if (buyButton != null) 
        {
            buyButton.gameObject.SetActive(canBuy);
            // Hiển thị thông báo nếu không thể mua do thẻ
            if (cannotBuyDueToCard)
            {
                buyButton.GetComponentInChildren<TMP_Text>().text = "Không thể mua (thẻ)";
            }
            else if (canBuyWithDiscount)
            {
                buyButton.GetComponentInChildren<TMP_Text>().text = "Mua (Giảm 50%)";
            }
            else
            {
                buyButton.GetComponentInChildren<TMP_Text>().text = "Mua";
            }
        }
        if (sellButton != null) sellButton.gameObject.SetActive(canSell);

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(BuyProperty);
        }

        // --- Logic tự động mua cho bot (nếu cần giữ lại) ---
        if (currentPlayer != null && currentPlayer.isBot && canBuy)
        {
            currentPlayer.BuyProperty(tile);
            if (gameManager != null)
                gameManager.ShowStatus($"{currentPlayer.playerName} (Bot) tự động mua {tile.tileName}");
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
        
        // Kiểm tra bảo mật chặt chẽ
        if (currentPlayer == null)
        {
            Debug.LogError("❌ Lỗi: currentPlayer là null!");
            return;
        }
        
        if (currentTile == null)
        {
            Debug.LogError("❌ Lỗi: currentTile là null!");
            return;
        }
        
        // Kiểm tra xem đất đã có chủ chưa
        if (currentTile.owner != null)
        {
            Debug.LogError($"❌ Lỗi: {currentTile.tileName} đã có chủ sở hữu: {currentTile.owner.playerName}!");
            if (gameManager != null)
                gameManager.ShowStatus($"❌ {currentTile.tileName} đã có chủ sở hữu!");
            return;
        }
        
        // Kiểm tra xem player có đang đứng trên ô này không
        if (currentPlayer.currentTileIndex != currentTile.transform.GetSiblingIndex())
        {
            Debug.LogError($"❌ Lỗi: {currentPlayer.playerName} không đang đứng trên {currentTile.tileName}!");
            if (gameManager != null)
                gameManager.ShowStatus($"❌ Bạn phải đứng trên {currentTile.tileName} để mua!");
            return;
        }
        
        // Kiểm tra trạng thái không thể mua do thẻ
        if (currentPlayer.cannotBuyNextTurn)
        {
            Debug.LogError($"❌ Lỗi: {currentPlayer.playerName} không thể mua đất do thẻ khí vận/cơ hội!");
            if (gameManager != null)
                gameManager.ShowStatus($"❌ Bạn không thể mua đất trong lượt này do thẻ!");
            return;
        }
        
        // Tính giá mua (có thể giảm 50% nếu có thẻ)
        int originalPrice = currentTile.GetPrice();
        int actualPrice = originalPrice;
        bool isDiscounted = false;
        
        if (currentPlayer.canBuyDiscountProperty)
        {
            actualPrice = originalPrice / 2;
            isDiscounted = true;
            currentPlayer.canBuyDiscountProperty = false; // Reset sau khi sử dụng
        }
        
        // Kiểm tra tiền
        if (!currentPlayer.CanPay(actualPrice))
        {
            Debug.LogError($"❌ Lỗi: {currentPlayer.playerName} không đủ tiền mua {currentTile.tileName}!");
            if (gameManager != null)
                gameManager.ShowStatus($"{currentPlayer.playerName} không đủ tiền mua {currentTile.tileName}!");
            return;
        }
        
        // Thực hiện mua đất
        currentPlayer.money -= actualPrice; // Trừ tiền trực tiếp
        currentTile.owner = currentPlayer;
        currentPlayer.ownedTiles.Add(currentTile);
        currentTile.SetOwner(currentPlayer);
        
        // Set houseCount = 1 và cập nhật visuals cho người chơi chính
        if (!currentPlayer.isBot)
        {
            currentTile.houseCount = 1;
            currentTile.UpdateVisuals();
        }
        
        string discountMessage = isDiscounted ? $" (Giảm 50%: {actualPrice}$ thay vì {originalPrice}$)" : "";
        Debug.Log($"✅ {currentPlayer.playerName} đã mua {currentTile.tileName} thành công!{discountMessage}");
        
        if (gameManager != null)
            gameManager.ShowInfoHud($"{currentPlayer.playerName} đã mua {currentTile.tileName}{discountMessage}");
        
        // Đóng panel sau khi mua thành công
        Hide();
        if (gameManager != null) gameManager.isWaitingForPlayerAction = false;
    }

    void SellProperty()
    {
        Debug.Log("Đã bấm nút Bán");
        
        // Kiểm tra bảo mật chặt chẽ
        if (currentPlayer == null)
        {
            Debug.LogError("❌ Lỗi: currentPlayer là null!");
            return;
        }
        
        if (currentTile == null)
        {
            Debug.LogError("❌ Lỗi: currentTile là null!");
            return;
        }
        
        if (currentTile.owner == null)
        {
            Debug.LogError("❌ Lỗi: Tài sản này chưa có chủ sở hữu!");
            return;
        }
        
        // Kiểm tra quyền sở hữu (so sánh object thay vì string)
        if (currentTile.owner != currentPlayer)
        {
            Debug.LogError($"❌ Lỗi bảo mật: {currentPlayer.playerName} không phải chủ sở hữu của {currentTile.tileName}!");
            if (gameManager != null)
                gameManager.ShowStatus($"❌ Bạn không phải chủ sở hữu của {currentTile.tileName}!");
            return;
        }
        
        // Thực hiện bán tài sản
        currentPlayer.SellProperty(currentTile);
        Debug.Log($"✅ {currentPlayer.playerName} đã bán {currentTile.tileName} thành công!");
        
        if (gameManager != null)
            gameManager.ShowStatus($"{currentPlayer.playerName} đã bán {currentTile.tileName}");
        
        Hide();
        if (gameManager != null) gameManager.isWaitingForPlayerAction = false;
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

    /// <summary>
    /// Refresh UI để cập nhật thông tin mới nhất
    /// </summary>
    public void RefreshUI()
    {
        if (currentTile != null && currentPlayer != null)
        {
            Show(currentTile, currentPlayer);
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
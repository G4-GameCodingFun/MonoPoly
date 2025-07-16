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
    public GameManager gameManager;
    public Button quitButton;

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
            quitButton.onClick.AddListener(QuitPanel);
        }

        if (tile != null && tile.data != null)
        {
            nameText.text = tile.data.provinceName;
            priceText.text = "Giá mua: " + tile.data.purchasePrice;
            rentText.text = "Giá thuê: " + tile.GetRent();

        }
        else
        {
            nameText.text = "";
            priceText.text = "";
            rentText.text = "";
        }

        // Chỉ hiện nút mua nếu ô chưa có chủ
        buyButton.gameObject.SetActive(tile != null && tile.owner == null);

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(BuyProperty);
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

    // Thêm hàm xử lý cho QuitButton
    public void QuitPanel()
    {
        Debug.Log("Đã bấm nút Quit");
        Hide();
        if (gameManager != null)
        {
            gameManager.isWaitingForPlayerAction = false; // Đặt lại trạng thái
        }
    }
}
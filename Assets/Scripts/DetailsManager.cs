using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI; // Để dùng Button

public class DetailsManager : MonoBehaviour
{
    public static DetailsManager Instance;

    // UI Elements
    public GameObject detailsPanel;
    public TextMeshProUGUI nameText; // Tên nhà
    public TextMeshProUGUI priceText; // Giá mua
    public TextMeshProUGUI rentText; // Giá thuê
    public TextMeshProUGUI ownerText; // Chủ sở hữu
    public Button buyButton; // Button Mua
    public Button sellButton; // Button Bán
    public Button closeButton; // Button Đóng

    private Tile currentTile; // Tile đang xem
    private PlayerController currentPlayer; // Player hiện tại (lấy từ GameManager)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        detailsPanel.SetActive(false); // Ẩn ban đầu
        closeButton.onClick.AddListener(CloseDetails); // Gán close
        buyButton.onClick.AddListener(BuyTile); // Gán buy
        sellButton.onClick.AddListener(SellTile); // Gán sell
    }

    public void ShowDetails(Tile tile)
    {
        if (tile == null) return;

        currentTile = tile;
        currentPlayer = GameManager.Instance.GetCurrentPlayer(); // Lấy player hiện tại

        nameText.text = "Tên: " + tile.tileName;
        priceText.text = "Giá mua: " + tile.GetPrice() + "$";
        rentText.text = "Giá thuê: " + tile.GetRent() + "$";
        ownerText.text = "Chủ sở hữu: " + (tile.owner != null ? tile.owner.playerName : "Chưa có");

        // Enable/Disable buttons
        buyButton.gameObject.SetActive(tile.owner == null && currentPlayer.currentTileIndex.Value == GameManager.Instance.mapTiles.IndexOf(tile.transform)); // Chỉ enable nếu chưa chủ và player ở ô đó
        sellButton.gameObject.SetActive(tile.owner == currentPlayer); // Enable nếu player là chủ

        detailsPanel.SetActive(true); // Hiện panel
    }

    private void BuyTile()
    {
        if (currentTile != null)
        {
            currentPlayer.BuyServerRpc(currentTile.NetworkObject); // Gọi RPC mua
            CloseDetails(); // Đóng panel sau mua
        }
    }

    private void SellTile()
    {
        if (currentTile != null)
        {
            // Giả sử thêm SellServerRpc trong PlayerController
            currentPlayer.SellServerRpc(currentTile.NetworkObject); // Gọi RPC bán (cần thêm code)
            CloseDetails(); // Đóng panel sau bán
        }
    }

    public void CloseDetails()
    {
        detailsPanel.SetActive(false);
        currentTile = null;
    }
}
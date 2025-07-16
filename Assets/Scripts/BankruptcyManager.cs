using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BankruptcyManager : MonoBehaviour
{
    public static BankruptcyManager Instance;
    
    [Header("UI Components")]
    public GameObject bankruptcyPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Transform propertiesContainer;
    public GameObject propertyItemPrefab;
    public Button sellButton;
    public Button cancelButton;
    
    [Header("Settings")]
    public int startingMoney = 200; // Tiền gốc để test
    
    private PlayerController currentPlayer;
    private List<PropertyTile> sellableProperties = new List<PropertyTile>();
    private PropertyTile selectedProperty;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void Start()
    {
        // Hạ thấp tiền gốc xuống 200 để test
        if (GameManager.Instance != null)
        {
            foreach (var player in GameManager.Instance.players)
            {
                player.money = startingMoney;
            }
        }
        
        SetupUI();
    }
    
    private void SetupUI()
    {
        if (sellButton != null)
            sellButton.onClick.AddListener(SellSelectedProperty);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CloseBankruptcyPanel);
    }
    
    /// <summary>
    /// Kiểm tra xem player có bị phá sản không
    /// </summary>
    public bool CheckBankruptcy(PlayerController player)
    {
        if (player == null) return false;
        
        // Kiểm tra nếu tiền < 0
        if (player.money < 0)
        {
            // Tính tổng giá trị tài sản có thể bán
            int totalAssetValue = CalculateTotalAssetValue(player);
            
            // Nếu có tài sản để bán
            if (totalAssetValue > 0)
            {
                ShowBankruptcyPanel(player);
                return true;
            }
            else
            {
                // Không có tài sản, game over
                HandleGameOver(player);
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Tính tổng giá trị tài sản có thể bán
    /// </summary>
    private int CalculateTotalAssetValue(PlayerController player)
    {
        int total = 0;
        sellableProperties.Clear();
        
        foreach (var tile in player.ownedTiles)
        {
            if (tile is PropertyTile propertyTile)
            {
                int sellValue = propertyTile.GetPrice() / 2; // Bán với giá 50% giá mua
                total += sellValue;
                sellableProperties.Add(propertyTile);
            }
        }
        
        return total;
    }
    
    /// <summary>
    /// Hiển thị panel phá sản
    /// </summary>
    private void ShowBankruptcyPanel(PlayerController player)
    {
        currentPlayer = player;
        selectedProperty = null;
        
        if (bankruptcyPanel != null)
        {
            bankruptcyPanel.SetActive(true);
            
            if (titleText != null)
                titleText.text = $"⚠️ PHÁ SẢN - {player.playerName}";
                
            if (messageText != null)
                messageText.text = $"Bạn đang thiếu {Mathf.Abs(player.money)}$!\nHãy bán bớt tài sản để trả nợ.";
            
            PopulatePropertiesList();
            UpdateSellButton();
        }
    }
    
    /// <summary>
    /// Điền danh sách tài sản vào UI
    /// </summary>
    private void PopulatePropertiesList()
    {
        if (propertiesContainer == null) return;
        
        // Xóa các item cũ
        foreach (Transform child in propertiesContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Tạo item mới cho từng tài sản
        foreach (var property in sellableProperties)
        {
            if (propertyItemPrefab != null)
            {
                GameObject item = Instantiate(propertyItemPrefab, propertiesContainer);
                SetupPropertyItem(item, property);
            }
        }
    }
    
    /// <summary>
    /// Thiết lập item tài sản trong danh sách
    /// </summary>
    private void SetupPropertyItem(GameObject item, PropertyTile property)
    {
        PropertyItemUI propertyUI = item.GetComponent<PropertyItemUI>();
        if (propertyUI != null)
        {
            propertyUI.Setup(property);
            
            // Lắng nghe sự kiện chọn
            Toggle toggle = item.GetComponentInChildren<Toggle>();
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) => {
                    if (isOn)
                    {
                        selectedProperty = property;
                        UpdateSellButton();
                    }
                    else if (selectedProperty == property)
                    {
                        selectedProperty = null;
                        UpdateSellButton();
                    }
                });
            }
        }
        else
        {
            // Fallback cho trường hợp không có PropertyItemUI component
            TextMeshProUGUI nameText = item.GetComponentInChildren<TextMeshProUGUI>();
            Toggle toggle = item.GetComponentInChildren<Toggle>();
            Button selectButton = item.GetComponentInChildren<Button>();
            
            if (nameText != null)
            {
                int sellValue = property.GetPrice() / 2;
                string houseInfo = "";
                if (property.houseCount > 0)
                    houseInfo = $" ({property.houseCount} nhà)";
                else if (property.hasHotel)
                    houseInfo = " (Khách sạn)";
                    
                nameText.text = $"{property.tileName}{houseInfo} - Bán: {sellValue}$";
            }
            
            if (toggle != null)
            {
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) => {
                    if (isOn)
                    {
                        selectedProperty = property;
                        UpdateSellButton();
                    }
                    else if (selectedProperty == property)
                    {
                        selectedProperty = null;
                        UpdateSellButton();
                    }
                });
            }
            
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(() => {
                    // Toggle selection
                    if (selectedProperty == property)
                    {
                        selectedProperty = null;
                        if (toggle != null) toggle.isOn = false;
                    }
                    else
                    {
                        selectedProperty = property;
                        if (toggle != null) toggle.isOn = true;
                    }
                    UpdateSellButton();
                });
            }
        }
    }
    
    /// <summary>
    /// Cập nhật trạng thái nút bán
    /// </summary>
    private void UpdateSellButton()
    {
        if (sellButton != null)
        {
            sellButton.interactable = selectedProperty != null;
        }
    }
    
    /// <summary>
    /// Bán tài sản đã chọn
    /// </summary>
    private void SellSelectedProperty()
    {
        if (selectedProperty == null || currentPlayer == null) return;
        
        // Bán tài sản
        currentPlayer.SellProperty(selectedProperty);
        
        // Kiểm tra lại xem còn thiếu tiền không
        if (currentPlayer.money >= 0)
        {
            // Đã đủ tiền, đóng panel
            CloseBankruptcyPanel();
            if (GameManager.Instance != null)
                GameManager.Instance.ShowInfoHud($"{currentPlayer.playerName} đã bán tài sản và thoát khỏi tình trạng phá sản!");
        }
        else
        {
            // Vẫn còn thiếu tiền, cập nhật lại danh sách
            sellableProperties.Remove(selectedProperty);
            selectedProperty = null;
            
            if (sellableProperties.Count == 0)
            {
                // Hết tài sản để bán
                HandleGameOver(currentPlayer);
            }
            else
            {
                // Cập nhật UI
                if (messageText != null)
                    messageText.text = $"Vẫn còn thiếu {Mathf.Abs(currentPlayer.money)}$!\nHãy bán thêm tài sản.";
                    
                PopulatePropertiesList();
                UpdateSellButton();
            }
        }
    }
    
    /// <summary>
    /// Đóng panel phá sản
    /// </summary>
    private void CloseBankruptcyPanel()
    {
        if (bankruptcyPanel != null)
            bankruptcyPanel.SetActive(false);
            
        currentPlayer = null;
        selectedProperty = null;
        sellableProperties.Clear();
    }
    
    /// <summary>
    /// Xử lý game over khi player phá sản hoàn toàn
    /// </summary>
    private void HandleGameOver(PlayerController player)
    {
        Debug.Log($"🎮 GAME OVER - {player.playerName} đã phá sản hoàn toàn!");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ShowInfoHud($"🎮 GAME OVER - {player.playerName} đã phá sản!", 5f);
            
            // Loại bỏ player khỏi danh sách
            GameManager.Instance.players.Remove(player);
            
            // Xóa player object
            if (player.gameObject != null)
                Destroy(player.gameObject);
                
            // Kiểm tra xem còn player nào không
            if (GameManager.Instance.players.Count <= 1)
            {
                // Chỉ còn 1 player hoặc không còn ai - kết thúc game
                StartCoroutine(EndGame());
            }
            else
            {
                // Cập nhật lại currentPlayerIndex nếu cần
                if (GameManager.Instance.currentPlayerIndex >= GameManager.Instance.players.Count)
                {
                    GameManager.Instance.currentPlayerIndex = 0;
                }
            }
        }
        
        CloseBankruptcyPanel();
    }
    
    /// <summary>
    /// Kết thúc game
    /// </summary>
    private IEnumerator EndGame()
    {
        yield return new WaitForSeconds(3f);
        
        if (GameManager.Instance.players.Count == 1)
        {
            PlayerController winner = GameManager.Instance.players[0];
            GameManager.Instance.ShowInfoHud($"🏆 CHIẾN THẮNG! {winner.playerName} là người chiến thắng!", 10f);
        }
        else
        {
            GameManager.Instance.ShowInfoHud("🎮 Tất cả người chơi đã phá sản! Game kết thúc.", 10f);
        }
        
        // Có thể thêm logic restart game hoặc quay về menu chính
        yield return new WaitForSeconds(5f);
        Debug.Log("Game kết thúc. Có thể thêm logic restart hoặc quay về menu.");
    }
} 
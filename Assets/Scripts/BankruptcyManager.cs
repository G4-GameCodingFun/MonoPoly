using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BankruptcyManager : MonoBehaviour
{
    public static BankruptcyManager Instance;
    
    [Header("UI Components")]
    public GameObject bankruptcyPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public TMP_Dropdown propertiesDropdown;
    public Button sellButton;
    public Button cancelButton;
    
    [Header("Animation Settings")]
    public float showDuration = 0.3f;
    public float hideDuration = 0.2f;
    public AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Settings")]
    public int startingMoney = 200; // Tiền gốc để test (đã được set mặc định trong PlayerController)
    
    private PlayerController currentPlayer;
    private List<PropertyTile> sellableProperties = new List<PropertyTile>();
    private PropertyTile selectedProperty;
    private bool isPanelVisible = false;
    private CanvasGroup panelCanvasGroup;
    
    // Thêm biến trạng thái để GameManager biết khi nào đang ở chế độ phá sản
    public bool isInBankruptcyMode = false;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void Start()
    {
        // Tiền đã được set mặc định 200$ trong PlayerController
        // Có thể override ở đây nếu cần test với số tiền khác
        if (GameManager.Instance != null && startingMoney != 200)
        {
            foreach (var player in GameManager.Instance.players)
            {
                player.money = startingMoney;
            }
        }
        
        SetupUI();
        SetupPanel();
        
        Debug.Log("✅ BankruptcyManager đã được khởi tạo thành công!");
    }
    
    private void SetupUI()
    {
        if (propertiesDropdown != null)
        {
            propertiesDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }
        
        if (sellButton != null)
            sellButton.onClick.AddListener(SellSelectedProperty);
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CloseBankruptcyPanel);
    }
    
    private void SetupPanel()
    {
        if (bankruptcyPanel != null)
        {
            // Thêm CanvasGroup nếu chưa có
            panelCanvasGroup = bankruptcyPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null)
            {
                panelCanvasGroup = bankruptcyPanel.AddComponent<CanvasGroup>();
            }
            
            // Ẩn panel ban đầu
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
            bankruptcyPanel.SetActive(false);
            isPanelVisible = false;
        }
    }
    
    /// <summary>
    /// Xử lý khi người dùng chọn item trong dropdown
    /// </summary>
    private void OnDropdownValueChanged(int index)
    {
        if (index > 0 && index - 1 < sellableProperties.Count) // index 0 là "Chọn tài sản..."
        {
            selectedProperty = sellableProperties[index - 1];
        }
        else
        {
            selectedProperty = null;
        }
        
        UpdateSellButton();
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
                if (player.isBot)
                {
                    // Bot tự động bán tài sản
                    HandleBotBankruptcy(player);
                }
                else
                {
                    // Người chơi hiển thị panel để chọn
                    ShowBankruptcyPanel(player);
                }
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
        
        // Set trạng thái phá sản và thông báo cho GameManager
        isInBankruptcyMode = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isWaitingForPlayerAction = true;
            string statusMessage = IconReplacer.ReplaceEmojis($"⚠️ {player.playerName} đang ở tình trạng phá sản! Hãy bán tài sản để trả nợ.");
            GameManager.Instance.ShowStatus(statusMessage);
        }
        
        if (bankruptcyPanel != null && !isPanelVisible)
        {
            StartCoroutine(ShowPanelAnimation());
            
            if (titleText != null)
                titleText.text = IconReplacer.ReplaceEmojis($"⚠️ PHÁ SẢN - {player.playerName}");
                
            if (messageText != null)
                messageText.text = $"Bạn đang thiếu {Mathf.Abs(player.money)}$!\nHãy chọn tài sản để bán.";
            
            PopulateDropdown();
            UpdateSellButton();
        }
    }
    
    /// <summary>
    /// Điền danh sách tài sản vào dropdown
    /// </summary>
    private void PopulateDropdown()
    {
        if (propertiesDropdown == null) return;
        
        propertiesDropdown.ClearOptions();
        
        // Thêm option đầu tiên
        List<string> options = new List<string>();
        
        if (sellableProperties.Count == 0)
        {
            options.Add(IconReplacer.ReplaceEmojis("💀 Không còn tài sản để bán! Tự động cưỡng chế phá sản..."));
        }
        else
        {
            options.Add("Chọn tài sản...");
            
            // Thêm các tài sản
            foreach (var property in sellableProperties)
            {
                int sellValue = property.GetPrice() / 2;
                string houseInfo = "";
                if (property.houseCount > 0)
                    houseInfo = $" ({property.houseCount} nhà)";
                else if (property.hasHotel)
                    houseInfo = " (Khách sạn)";
                    
                string optionText = $"{property.tileName}{houseInfo} - Bán: {sellValue}$";
                options.Add(optionText);
            }
        }
        
        propertiesDropdown.AddOptions(options);
        propertiesDropdown.value = 0; // Reset về option đầu tiên
        
        // Disable dropdown nếu không còn tài sản
        propertiesDropdown.interactable = sellableProperties.Count > 0;
    }
    
    /// <summary>
    /// Cập nhật trạng thái nút bán
    /// </summary>
    private void UpdateSellButton()
    {
        if (sellButton != null)
        {
            sellButton.interactable = selectedProperty != null;
            
            if (selectedProperty != null)
            {
                int sellValue = selectedProperty.GetPrice() / 2;
                sellButton.GetComponentInChildren<TextMeshProUGUI>().text = $"BÁN TÀI SẢN (+{sellValue}$)";
            }
            else
            {
                sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "BÁN TÀI SẢN";
            }
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
                // Hết tài sản để bán - TỰ ĐỘNG cưỡng chế phá sản
                if (messageText != null)
                    messageText.text = IconReplacer.ReplaceEmojis($"💀 Đã bán hết tài sản nhưng vẫn thiếu {Mathf.Abs(currentPlayer.money)}$!\nHệ thống sẽ tự động cưỡng chế phá sản sau 3 giây...");
                
                if (GameManager.Instance != null)
                {
                    string infoMessage = IconReplacer.ReplaceEmojis($"💀 {currentPlayer.playerName} đã bán hết tài sản nhưng vẫn thiếu {Mathf.Abs(currentPlayer.money)}$! Tự động cưỡng chế phá sản.");
                    GameManager.Instance.ShowInfoHud(infoMessage, 3f);
                }
                
                // Tự động cưỡng chế phá sản sau 3 giây
                StartCoroutine(AutoForceBankruptcy());
            }
            else
            {
                // Cập nhật UI
                string warningMessage = "";
                if (sellableProperties.Count <= 2)
                {
                    warningMessage = IconReplacer.ReplaceEmojis($"\n⚠️ CẢNH BÁO: Chỉ còn {sellableProperties.Count} tài sản!");
                }
                
                if (messageText != null)
                    messageText.text = $"Vẫn còn thiếu {Mathf.Abs(currentPlayer.money)}$!\nHãy chọn tài sản khác để bán.{warningMessage}";
                    
                PopulateDropdown();
                UpdateSellButton();
                
                // Thông báo cho GameManager rằng vẫn đang ở chế độ phá sản
                if (GameManager.Instance != null)
                {
                    string statusMessage = IconReplacer.ReplaceEmojis($"⚠️ {currentPlayer.playerName} vẫn còn thiếu {Mathf.Abs(currentPlayer.money)}$!");
                    if (sellableProperties.Count <= 2)
                    {
                        statusMessage += $" Chỉ còn {sellableProperties.Count} tài sản!";
                    }
                    GameManager.Instance.ShowStatus(statusMessage);
                }
            }
        }
    }
    
    /// <summary>
    /// Đóng panel phá sản
    /// </summary>
    private void CloseBankruptcyPanel()
    {
        if (bankruptcyPanel != null && isPanelVisible)
        {
            StartCoroutine(HidePanelAnimation());
        }
        
        // Reset trạng thái phá sản và thông báo cho GameManager
        isInBankruptcyMode = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isWaitingForPlayerAction = false;
            string successMessage = IconReplacer.ReplaceEmojis($"✅ {currentPlayer?.playerName} đã thoát khỏi tình trạng phá sản!");
            GameManager.Instance.ShowStatus(successMessage);
        }
    }
    
    /// <summary>
    /// Xử lý bot phá sản - tự động bán tài sản
    /// </summary>
    private void HandleBotBankruptcy(PlayerController player)
    {
        Debug.Log($"🤖 {player.playerName} (Bot) đang xử lý phá sản...");
        
        // Set trạng thái phá sản cho bot
        isInBankruptcyMode = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isWaitingForPlayerAction = true;
            string botStatus = IconReplacer.ReplaceEmojis($"🤖 {player.playerName} (Bot) đang xử lý phá sản...");
            GameManager.Instance.ShowStatus(botStatus);
        }
        
        // Tính lại danh sách tài sản có thể bán
        CalculateTotalAssetValue(player);
        
        // Bot sẽ bán tài sản cho đến khi đủ tiền
        while (player.money < 0 && sellableProperties.Count > 0)
        {
            // Bán tài sản đắt nhất trước
            PropertyTile mostExpensive = null;
            int maxValue = 0;
            
            foreach (var property in sellableProperties)
            {
                int sellValue = property.GetPrice() / 2;
                if (sellValue > maxValue)
                {
                    maxValue = sellValue;
                    mostExpensive = property;
                }
            }
            
            if (mostExpensive != null)
            {
                player.SellProperty(mostExpensive);
                sellableProperties.Remove(mostExpensive);
                
                if (GameManager.Instance != null)
                {
                    string sellMessage = IconReplacer.ReplaceEmojis($"🤖 {player.playerName} đã bán {mostExpensive.tileName} để trả nợ");
                    GameManager.Instance.ShowInfoHud(sellMessage);
                }
                
                // Chờ một chút để người chơi thấy thông báo
                StartCoroutine(DelayForBotAction());
            }
            else
            {
                break;
            }
        }
        
        // Kiểm tra lại sau khi bán
        if (player.money >= 0)
        {
            // Reset trạng thái phá sản
            isInBankruptcyMode = false;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.isWaitingForPlayerAction = false;
                string botSuccessMessage = IconReplacer.ReplaceEmojis($"🤖 {player.playerName} đã thoát khỏi tình trạng phá sản!");
                GameManager.Instance.ShowInfoHud(botSuccessMessage);
            }
        }
        else
        {
            // Vẫn còn thiếu tiền sau khi bán hết tài sản - cưỡng chế phá sản
            if (GameManager.Instance != null)
            {
                string botBankruptcyMessage = IconReplacer.ReplaceEmojis($"💀 {player.playerName} (Bot) đã bán hết tài sản nhưng vẫn thiếu {Mathf.Abs(player.money)}$! Bị cưỡng chế phá sản.");
                GameManager.Instance.ShowInfoHud(botBankruptcyMessage, 3f);
            }
            
            // Chờ một chút để người chơi thấy thông báo
            StartCoroutine(DelayForBotAction());
            
            // Xử lý game over ngay lập tức cho bot
            HandleGameOver(player);
        }
    }
    
    /// <summary>
    /// Xử lý game over khi player phá sản hoàn toàn
    /// </summary>
    private void HandleGameOver(PlayerController player)
    {
        Debug.Log($"🎮 GAME OVER - {player.playerName} đã phá sản hoàn toàn!");
        
        // Reset trạng thái phá sản
        isInBankruptcyMode = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isWaitingForPlayerAction = false;
            GameManager.Instance.ShowInfoHud($"🎮 GAME OVER - {player.playerName} đã phá sản!", 5f);
            
            // Tìm index của player trước khi xóa
            int playerIndex = GameManager.Instance.players.IndexOf(player);
            
            // Loại bỏ player khỏi danh sách
            GameManager.Instance.players.Remove(player);
            
            // Cập nhật currentTileIndexes nếu cần
            if (playerIndex >= 0 && playerIndex < GameManager.Instance.currentTileIndexes.Length)
            {
                // Xóa index của player đã bị loại
                for (int i = playerIndex; i < GameManager.Instance.currentTileIndexes.Length - 1; i++)
                {
                    GameManager.Instance.currentTileIndexes[i] = GameManager.Instance.currentTileIndexes[i + 1];
                }
            }
            
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
    /// Animation hiển thị panel
    /// </summary>
    private IEnumerator ShowPanelAnimation()
    {
        if (panelCanvasGroup == null) yield break;
        
        isPanelVisible = true;
        bankruptcyPanel.SetActive(true);
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        
        float elapsedTime = 0f;
        while (elapsedTime < showDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / showDuration;
            float alpha = showCurve.Evaluate(progress);
            
            panelCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        panelCanvasGroup.alpha = 1f;
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
    }
    
    /// <summary>
    /// Animation ẩn panel
    /// </summary>
    private IEnumerator HidePanelAnimation()
    {
        if (panelCanvasGroup == null) yield break;
        
        isPanelVisible = false;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        
        float elapsedTime = 0f;
        while (elapsedTime < hideDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / hideDuration;
            float alpha = hideCurve.Evaluate(progress);
            
            panelCanvasGroup.alpha = alpha;
            yield return null;
        }
        
        panelCanvasGroup.alpha = 0f;
        bankruptcyPanel.SetActive(false);
        
        // Reset data
        currentPlayer = null;
        selectedProperty = null;
        sellableProperties.Clear();
    }
    
    /// <summary>
    /// Delay cho bot action để người chơi thấy thông báo
    /// </summary>
    private IEnumerator DelayForBotAction()
    {
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Tự động cưỡng chế phá sản khi đã bán hết tài sản mà vẫn không đủ tiền
    /// </summary>
    private IEnumerator AutoForceBankruptcy()
    {
        yield return new WaitForSeconds(3f);

        if (currentPlayer != null)
        {
                    Debug.Log($"💀 {currentPlayer.playerName} đã bán hết tài sản nhưng vẫn thiếu {Mathf.Abs(currentPlayer.money)}$! Tự động cưỡng chế phá sản.");
        
        if (GameManager.Instance != null)
        {
            string forceBankruptcyMessage = IconReplacer.ReplaceEmojis($"💀 {currentPlayer.playerName} bị cưỡng chế phá sản!");
            GameManager.Instance.ShowInfoHud(forceBankruptcyMessage, 3f);
        }
            
            // Xử lý game over
            HandleGameOver(currentPlayer);
        }
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
        
        // Hiển thị thông báo thoát game
        yield return new WaitForSeconds(5f);
        GameManager.Instance.ShowInfoHud("⏰ Thoát về màn hình chính sau 15 giây...", 15f);
        
        // Chờ 15 giây rồi thoát về màn hình chính
        yield return new WaitForSeconds(15f);
        
        // Thoát về màn hình chính
        UnityEngine.SceneManagement.SceneManager.LoadScene("CreateAccountScene");
    }
} 
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class GameState
{
    public int currentPlayerIndex;
    public List<PlayerState> playersState;
    public List<int> dieRolls;
    public int diceTotal;

    [System.Serializable]
    public class PlayerState
    {
        public string playerName;
        public int money;
        public int currentTileIndex;
        public bool inJail;
        public int jailTurns;
        public bool skipNextTurn;
        public bool cannotBuyNextTurn;
        public bool canBuyDiscountProperty;
        public bool hasGetOutOfJailFreeCard;
        public List<int> ownedTileIndexes;
        public bool isBot;
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public List<Transform> mapTiles;
    public List<PlayerController> players = new List<PlayerController>();
    public float moveSpeed = 15f;
    public CardManager cardManager;
    public Transform jailPosition;

    public int[] currentTileIndexes;
    public int currentPlayerIndex = 0;
    public bool isMoving = false;
    public int diceTotal = 0;
    public bool isWaitingForPlayerAction = false;

    public Dice dice1;
    public Dice dice2;
    public List<int> dieRolls = new List<int>();

    public TextMeshProUGUI statusText;
    public Button rollButton;

    public List<GameObject> botPrefabs;
    public GameObject playerPrefab;

    public TextMeshProUGUI countdownText; // Tham chiếu đến TextMeshProUGUI trong UI
    private float turnTimeLimit = 30f; // Tăng thời gian mỗi lượt lên 30 giây
    private float currentTurnTime;

    // Thêm biến HUD phụ
    public TextMeshProUGUI infoHudText;

    // Thêm tham chiếu DetailsPanelController
    public DetailsPanelController detailsPanelController;

    private string savePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/gameSession.json";
    }

    private void Start()
    {
        SetupPlayers();
        
        // Kiểm tra xem có player nào được tạo không trước khi tiếp tục
        if (players.Count == 0)
        {
            Debug.LogError("Không thể bắt đầu game vì không có player nào!");
            return;
        }
        
        if (dice1 != null) dice1.gameObject.SetActive(false);
        if (dice2 != null) dice2.gameObject.SetActive(false);

        if (rollButton != null)
        {
            rollButton.onClick.AddListener(RollDice);
            rollButton.interactable = true;
            rollButton.gameObject.SetActive(false); // Ẩn nút roll khi bắt đầu
        }

        currentTurnTime = turnTimeLimit;
        isMoving = false; // Đặt lại rõ ràng
        isWaitingForPlayerAction = false; // Đặt lại rõ ràng
        UpdateCountdownText();

        ShowStatus($"Bắt đầu game. Tới lượt: {players[0].playerName} {(players[0].isBot ? "(Bot)" : "(Người chơi)")}");
        
        // Chỉ tự động roll nếu player đầu tiên là bot
        if (players[0].isBot)
        {
            CheckBotTurn();
        }
        else
        {
            ShowStatus($"{players[0].playerName} - Hãy bấm nút Roll để bắt đầu!");
        }
    }

    void SetupPlayers()
    {
        players.Clear();
        Vector3 basePosition = mapTiles[0].position;

        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab chưa được gán!");
            return;
        }

        var playerObj = Instantiate(playerPrefab, basePosition, Quaternion.identity);
        if (playerObj == null)
        {
            Debug.LogError("Không thể instantiate Player Prefab!");
            return;
        }

        var pc = playerObj.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("Player Prefab không có component PlayerController!");
            Destroy(playerObj);
            return;
        }

        pc.playerName = PlayerPrefs.GetString("PlayerName", "You");
        pc.isBot = false;
        pc.money = 200; // Set tiền cho player gốc
        pc.FaceLeft();
        playerObj.transform.position = new Vector3(57f, -16f, basePosition.z);
        pc.currentTileIndex = 0;
        players.Add(pc);

        // Add bots
        if (botPrefabs == null || botPrefabs.Count < 3)
        {
            Debug.LogError("Danh sách botPrefabs rỗng hoặc không đủ 3 bot!");
            return;
        }

        // Tạo 3 bot riêng biệt
        var botPositions = new Vector3[]
        {
        new Vector3(55f, -12f, basePosition.z), // Player3
        new Vector3(45f, -11f, basePosition.z), // Player4
        new Vector3(43f, -16f, basePosition.z)  // Player5
        };

        for (int i = 0; i < 3; i++)
        {
            if (botPrefabs[i] == null)
            {
                Debug.LogError($"Bot Prefab tại chỉ số {i} chưa được gán!");
                continue;
            }

            var botObj = Instantiate(botPrefabs[i], basePosition, Quaternion.identity);
            if (botObj == null)
            {
                Debug.LogError($"Không thể instantiate Bot Prefab tại chỉ số {i}!");
                continue;
            }

            var botPc = botObj.GetComponent<PlayerController>();
            if (botPc == null)
            {
                Debug.LogError($"Bot Prefab tại chỉ số {i} không có component PlayerController!");
                Destroy(botObj);
                continue;
            }

            botPc.playerName = $"Player{i + 3}"; // Player3, Player4, Player5
            botPc.isBot = true;
            botPc.money = 200;
            botPc.FaceLeft();
            botObj.transform.position = botPositions[i];
            botPc.currentTileIndex = 0;
            players.Add(botPc);
        }

        if (players.Count == 0)
        {
            Debug.LogError("Không có player nào được tạo! Vui lòng kiểm tra prefabs.");
            return;
        }

        currentTileIndexes = new int[players.Count];
        for (int i = 0; i < players.Count; i++)
            currentTileIndexes[i] = 0;
    }

    private void Update()
    {
        // Kiểm tra trạng thái phá sản một lần cho toàn bộ hàm Update
        bool isInBankruptcy = BankruptcyManager.Instance != null && BankruptcyManager.Instance.isInBankruptcyMode;
        
        if (rollButton != null)
        {
            // Chỉ hiện nút roll khi tới lượt người chơi, không phải bot, không di chuyển, không chờ hành động, và không đang ở chế độ phá sản
            bool shouldShow = !isMoving && IsCurrentPlayerLocal() && !isWaitingForPlayerAction && !isInBankruptcy;
            rollButton.gameObject.SetActive(shouldShow);
            rollButton.interactable = shouldShow;
        }

        // Chỉ đếm thời gian cho user chính, không đếm cho bot, và không đếm khi đang ở chế độ phá sản
        if (currentTurnTime > 0 && currentPlayerIndex >= 0 && currentPlayerIndex < players.Count && !players[currentPlayerIndex].isBot && !isInBankruptcy)
        {
            currentTurnTime -= Time.deltaTime;
            UpdateCountdownText();

            // Chuyển lượt nếu hết thời gian (chỉ cho user chính)
            if (currentTurnTime <= 0)
            {
                ShowStatus($"Hết thời gian cho {players[currentPlayerIndex].playerName}! Chuyển lượt.");
                NextTurn();
            }
        }
    }

    private bool IsCurrentPlayerLocal()
    {
        // Kiểm tra xem players list có rỗng không và currentPlayerIndex có hợp lệ không
        if (players == null || players.Count == 0 || currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
        {
            return false;
        }
        
        // Chỉ có 1 người chơi local, luôn trả về true nếu không phải bot
        return !players[currentPlayerIndex].isBot;
    }

    public void RollDice()
    {
        // Kiểm tra xem có đang ở chế độ phá sản không
        bool isCurrentlyInBankruptcy = BankruptcyManager.Instance != null && BankruptcyManager.Instance.isInBankruptcyMode;
        if (isCurrentlyInBankruptcy)
        {
            ShowStatus("⚠️ Không thể roll dice khi đang ở chế độ phá sản! Hãy bán tài sản trước.");
            return;
        }
        
        if (!isMoving && IsCurrentPlayerLocal())
        {
            StartCoroutine(HandleRollAndMove(players[currentPlayerIndex]));
        }
    }

    private IEnumerator HandleRollAndMove(PlayerController player)
    {
        currentTurnTime = turnTimeLimit;
        isMoving = true;

        if (player.inJail)
        {
            player.jailTurns--;

            if (player.jailTurns <= 0)
            {
                player.GetOutOfJail();
                ShowStatus($"{player.playerName} đã hết lượt trong tù và được thả");
            }
            else
            {
                ShowStatus($"{player.playerName} đang bị tù. Còn {player.jailTurns} lượt");
                yield return new WaitForSeconds(2f);

                isMoving = false;
                NextTurn(); // 👉 Chuyển lượt cho người tiếp theo
                yield break; // ⛔ Dừng coroutine không thực hiện di chuyển
            }
        }

        diceTotal = 0;
        dieRolls.Clear();

        ShowDice(true);

        yield return StartCoroutine(DieRolling(1));
        yield return StartCoroutine(DieRolling(2));

        yield return new WaitForSeconds(1.5f);

        string playerType = player.isBot ? "(Bot)" : "(Người chơi)";
        ShowStatus($"{player.playerName} {playerType} rolled: {diceTotal} ({dieRolls[0]} + {dieRolls[1]})");

        yield return new WaitForSeconds(1f);

        ShowDice(false);

        int playerIndex = currentPlayerIndex;
        yield return StartCoroutine(MovePlayer(playerIndex, diceTotal));

        isMoving = false;

        if (player.isBot)
        {
            yield return new WaitForSeconds(3f); // Tăng thời gian chờ cho bot
        }

        NextTurn();
    }

    private IEnumerator DieRolling(int diceNumber)
    {
        int rolledValue = 0;
        for (int i = 0; i <= 20; i++)
        {
            rolledValue = Random.Range(1, 7);
            DisplayDice(diceNumber, rolledValue);
            yield return new WaitForSeconds(0.05f);
        }
        AddDiceRoll(rolledValue);
    }

    public void AddDiceRoll(int value)
    {
        diceTotal += value;
        dieRolls.Add(value);
    }

    private void DisplayDice(int diceNumber, int rolledValue)
    {
        Dice dice = diceNumber == 1 ? dice1 : dice2;
        if (dice != null)
        {
            dice.DisplayDice(rolledValue);
        }
    }

    private void ShowDice(bool show)
    {
        if (dice1 != null) dice1.gameObject.SetActive(show);
        if (dice2 != null) dice2.gameObject.SetActive(show);
    }

    private IEnumerator MovePlayer(int playerIndex, int steps)
    {
        PlayerController player = players[playerIndex];
        string playerType = player.isBot ? "(Bot)" : "(Người chơi)";

        player.SetWalking(true);

        for (int i = 0; i < steps; i++)
        {
            currentTileIndexes[playerIndex] = (currentTileIndexes[playerIndex] + 1) % mapTiles.Count;

            bool passedGo = currentTileIndexes[playerIndex] == 0 && i < steps - 1;

            Vector3 target = mapTiles[currentTileIndexes[playerIndex]].position;
            if (target.x < player.transform.position.x)
                player.FaceLeft();
            player.transform.position = target;

            yield return new WaitForSeconds(0.3f);

            if (passedGo)
            {
                player.money += 200;
                ShowStatus($"{player.playerName} {playerType} đi qua Xuất Phát và nhận 200$");
                yield return new WaitForSeconds(1f);
            }
        }

        player.SetWalking(false);

        player.currentTileIndex = currentTileIndexes[playerIndex];

        Tile landedTile = mapTiles[player.currentTileIndex].GetComponent<Tile>();
        if (landedTile != null)
        {
            PropertyTile prop = landedTile as PropertyTile;
            if (prop != null && prop.owner != null)
            {
                ShowStatus($"Nhà đã có chủ: {prop.owner.playerName}");
                yield return new WaitForSeconds(1f);
            }

            landedTile.OnPlayerLanded(player);

            // Xử lý ô Chance hoặc Community Chest
            ChanceTile chanceTile = landedTile as ChanceTile;
            KhiVanTile communityTile = landedTile as KhiVanTile;
            if (chanceTile != null || communityTile != null)
            {
                if (cardManager != null)
                {
                    isWaitingForPlayerAction = true;
                    if (chanceTile != null)
                        cardManager.DrawCoHoiCard(player);
                    else
                        cardManager.DrawKhiVanCard(player);
                    // Chờ đến khi cardPanel được tắt (isFlipping = false)
                    yield return new WaitUntil(() => !cardManager.isFlipping && !cardManager.cardPanel.activeSelf);
                    isWaitingForPlayerAction = false;
                }
            }

            // Xử lý ô PropertyTile
            if (prop != null && prop.data != null && detailsPanelController != null)
            {
                if (player.isBot)
                {
                    // Nếu là bot, hiển thị DetailsPanel nhưng tự động mua và đóng
                    isWaitingForPlayerAction = true;
                    detailsPanelController.Show(prop, player);
                    if (prop.owner == null && player.CanPay(prop.GetPrice()))
                    {
                        player.BuyProperty(prop);
                        ShowStatus($"{player.playerName} {playerType} tự động mua {landedTile.tileName}");
                        yield return new WaitForSeconds(1f); // Chờ để người chơi thấy thông báo
                        
                    }
                    detailsPanelController.QuitPanel(); // Tắt DetailsPanel
                    isWaitingForPlayerAction = false;
                }
                else
                {
                    // Nếu là người chơi chính, hiển thị và chờ hành động thủ công
                    Debug.Log("Gọi Show DetailsPanel từ GameManager cho: " + prop.data.provinceName);
                    isWaitingForPlayerAction = true;
                    detailsPanelController.Show(prop, player);
                    yield return new WaitUntil(() => !isWaitingForPlayerAction);
                }
            }

        }
        else
        {
            ShowStatus($"Ô {player.currentTileIndex} không có component Tile.");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(2.5f); // Tăng thời gian chờ cuối lượt
    }

    private void NextTurn()
    {
        currentTurnTime = turnTimeLimit;

        // Reset trạng thái đặc biệt của người chơi hiện tại
        PlayerController currentPlayer = players[currentPlayerIndex];
        if (currentPlayer != null)
        {
            // Reset các trạng thái đặc biệt
            currentPlayer.skipNextTurn = false;
            currentPlayer.cannotBuyNextTurn = false;
            currentPlayer.canBuyDiscountProperty = false;
        }

        foreach (var player in players)
        {
            player.SetArrowVisible(false);
        }

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        PlayerController nextPlayer = players[currentPlayerIndex];
        string playerType = nextPlayer.isBot ? "(Bot)" : "(Người chơi)";

        // Kiểm tra nếu người chơi bị skip lượt
        if (nextPlayer.skipNextTurn)
        {
            Debug.Log($"⏭️ {nextPlayer.playerName} bị skip lượt do thẻ khí vận/cơ hội");
            nextPlayer.skipNextTurn = false; // Reset ngay lập tức
            NextTurn(); // Chuyển sang người tiếp theo
            return;
        }

        nextPlayer.SetArrowVisible(true);

        ShowStatus($"Tới lượt: {nextPlayer.playerName} {playerType}");

        // Chỉ tự động roll cho bot, không roll cho user chính
        if (nextPlayer.isBot)
        {
            StartCoroutine(AutoRollForBot());
        }
        else
        {
            // User chính cần bấm nút roll thủ công
            ShowStatus($"{nextPlayer.playerName} - Hãy bấm nút Roll để chơi!");
        }
    }

    private IEnumerator AutoRollForBot()
    {
        yield return new WaitForSeconds(4f); // Tăng thời gian chờ bot lên 4 giây
        
        // Kiểm tra xem có đang chờ user action hoặc đang ở chế độ phá sản không
        bool isBotInBankruptcy = BankruptcyManager.Instance != null && BankruptcyManager.Instance.isInBankruptcyMode;
        if (!isWaitingForPlayerAction && !isMoving && !isBotInBankruptcy)
        {
            StartCoroutine(HandleRollAndMove(players[currentPlayerIndex]));
        }
    }

    public void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            StartCoroutine(ClearStatusAfterDelay(4f));
        }
        Debug.Log(message);
    }

    private IEnumerator ClearStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (statusText != null) statusText.text = "";
    }

    private void CheckBotTurn()
    {
        PlayerController currentPlayer = players[currentPlayerIndex];
        if (currentPlayer != null && currentPlayer.isBot)
        {
            StartCoroutine(AutoRollForBot());
        }
    }

    public void MovePlayerToTile(PlayerController player, int targetTileIndex)
    {
        if (player == null || targetTileIndex < 0 || targetTileIndex >= mapTiles.Count) return;
        
        player.currentTileIndex = targetTileIndex;
        Vector3 targetPosition = mapTiles[targetTileIndex].position;
        player.transform.position = targetPosition;
        
        // Gọi OnPlayerLanded cho tile mới
        var tile = mapTiles[targetTileIndex].GetComponent<Tile>();
        if (tile != null)
        {
            tile.OnPlayerLanded(player);
        }
        
        Debug.Log($"{player.playerName} đã di chuyển đến ô {targetTileIndex}");
    }

    public void HandleChanceTile(PlayerController player)
    {
        if (cardManager != null)
        {
            isWaitingForPlayerAction = true;
            cardManager.DrawCoHoiCard(player);
            // Chờ đến khi cardPanel được tắt
            StartCoroutine(WaitForCardAction());
        }
        else
        {
            Debug.LogError("CardManager không được gán!");
        }
    }

    public void HandleCommunityChestTile(PlayerController player)
    {
        if (cardManager != null)
        {
            isWaitingForPlayerAction = true;
            cardManager.DrawKhiVanCard(player);
            StartCoroutine(WaitForCardAction());
        }
        else
        {
            Debug.LogError("CardManager không được gán!");
        }
    }

    private IEnumerator WaitForCardAction()
    {
        yield return new WaitUntil(() => !cardManager.isFlipping && !cardManager.cardPanel.activeSelf);
        isWaitingForPlayerAction = false;
    }

    private void UpdateCountdownText()
    {
        if (countdownText != null)
        {
            // Kiểm tra xem có đang ở chế độ phá sản không
            bool isCountdownInBankruptcy = BankruptcyManager.Instance != null && BankruptcyManager.Instance.isInBankruptcyMode;
            
            if (isCountdownInBankruptcy)
            {
                countdownText.text = "⏸️ GAME TẠM DỪNG - Đang xử lý phá sản";
                countdownText.color = Color.red;
            }
            else
            {
                countdownText.text = "Countdown Time: " + Mathf.Ceil(currentTurnTime).ToString() + "s";
                countdownText.color = Color.white;
            }
        }
    }

    // ===== HUD phụ: ShowInfoHud =====
    public void ShowInfoHud(string message, float duration = 2.5f)
    {
        if (infoHudText == null) return;
        StopCoroutine("ShowInfoHudCoroutine"); // Đảm bảo không bị chồng thông báo
        StartCoroutine(ShowInfoHudCoroutine(message, duration));
    }

    private IEnumerator ShowInfoHudCoroutine(string message, float duration)
    {
        infoHudText.text = message;
        infoHudText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        infoHudText.text = "";
        infoHudText.gameObject.SetActive(false);
    }
}
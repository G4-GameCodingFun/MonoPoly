using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    // Thêm tham chiếu PlayerInfo để refresh bảng player
    public PlayerInfo playerInfo;

    private string savePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/gameSession.json";
    }

    private void Start()
    {
        // Kiểm tra và gán detailsPanelController nếu cần
        if (detailsPanelController == null)
        {
            detailsPanelController = FindObjectOfType<DetailsPanelController>();
            if (detailsPanelController == null)
            {
                Debug.LogWarning("⚠️ Không tìm thấy DetailsPanelController trong scene!");
            }
            else
            {
                Debug.Log("✅ Đã tự động gán DetailsPanelController");
            }
        }
        
        // Reset tất cả tài sản về trạng thái ban đầu
        ResetAllProperties();
        
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

        pc.playerName = "You";
        pc.isBot = false;
        pc.money = 500; // Set tiền cho player gốc
        pc.FaceLeft();
        playerObj.transform.position = new Vector3(57f, -16f, basePosition.z);
        pc.currentTileIndex = 0;
        players.Add(pc);

        // Add 3 bots
        if (botPrefabs == null || botPrefabs.Count < 3)
        {
            Debug.LogError("Danh sách botPrefabs rỗng hoặc không đủ 3 bot!");
            return;
        }

        // Tạo 3 bot riêng biệt
        var botPositions = new Vector3[]
        {
            new Vector3(55f, -12f, basePosition.z), // Bot 1
            new Vector3(45f, -11f, basePosition.z), // Bot 2
            new Vector3(43f, -16f, basePosition.z)  // Bot 3
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

            botPc.playerName = $"Bot{i + 1}";
            botPc.isBot = true;
            botPc.money = 500; // Set tiền cho bot
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
        
        // === CHEAT KEY: Alt + Space + F ===
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.Space) && Input.GetKeyDown(KeyCode.F))
        {
            CheatBankruptBots();
        }
        
        // === CHEAT KEY: Alt + Space + G ===
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.Space) && Input.GetKeyDown(KeyCode.G))
        {
            CheatBankruptBotsWithProperties();
        }
        
        // === CHEAT KEY: Ctrl + Alt + Space ===
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Space))
        {
            CheatKillRandomBot();
        }
        
        if (rollButton != null)
        {
            // Chỉ hiện nút roll khi tới lượt người chơi, không phải bot, không di chuyển, không chờ hành động, không đang ở chế độ phá sản, và KHÔNG đang ở tù
            bool isCurrentPlayerInJail = currentPlayerIndex >= 0 && currentPlayerIndex < players.Count && players[currentPlayerIndex].inJail;
            bool shouldShow = !isMoving && IsCurrentPlayerLocal() && !isWaitingForPlayerAction && !isInBankruptcy && !isCurrentPlayerInJail;
            rollButton.gameObject.SetActive(shouldShow);
            rollButton.interactable = shouldShow;
        }

        // Cập nhật countdown text luôn luôn
        UpdateCountdownText();

        // Chỉ đếm thời gian cho user, không đếm cho bot, và không đếm khi đang ở chế độ phá sản hoặc đang ở tù
        bool isPlayerInJailForTimer = currentPlayerIndex >= 0 && currentPlayerIndex < players.Count && players[currentPlayerIndex].inJail;
        if (currentTurnTime > 0 && currentPlayerIndex >= 0 && currentPlayerIndex < players.Count && !players[currentPlayerIndex].isBot && !isInBankruptcy && !isMoving && !isWaitingForPlayerAction && !isPlayerInJailForTimer)
        {
            currentTurnTime -= Time.deltaTime;

            // Chuyển lượt nếu hết thời gian (chỉ cho user)
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
            string warningMessage = IconReplacer.ReplaceEmojis("⚠️ Không thể roll dice khi đang ở chế độ phá sản! Hãy bán tài sản trước.");
            ShowStatus(warningMessage);
            return;
        }
        
        // Kiểm tra xem player có đang ở tù không
        bool isCurrentPlayerInJail = currentPlayerIndex >= 0 && currentPlayerIndex < players.Count && players[currentPlayerIndex].inJail;
        if (isCurrentPlayerInJail)
        {
            string jailMessage = IconReplacer.ReplaceEmojis($"🔒 {players[currentPlayerIndex].playerName} đang ở tù, không thể roll dice!");
            ShowStatus(jailMessage);
            return;
        }
        
        if (!isMoving && IsCurrentPlayerLocal())
        {
            AudioManager.Instance.PlayDiceRoll();

            StartCoroutine(HandleRollAndMove(players[currentPlayerIndex]));
        }
    }

    private IEnumerator HandleRollAndMove(PlayerController player)
    {
        currentTurnTime = turnTimeLimit;
        isMoving = true;

        if (player.inJail)
        {
            Debug.Log($"🔒 {player.playerName} đang ở tù. JailTurns: {player.jailTurns}");
            
            // Kiểm tra xem có thẻ "Get Out of Jail Free" không
            if (player.hasGetOutOfJailFreeCard)
            {
                Debug.Log($"🎫 {player.playerName} sử dụng thẻ 'Get Out of Jail Free'");
                player.hasGetOutOfJailFreeCard = false;
                player.GetOutOfJail();
                
                // Đồng bộ lại vị trí trong GameManager sau khi ra tù
                if (currentTileIndexes != null && currentPlayerIndex >= 0 && currentPlayerIndex < currentTileIndexes.Length)
                {
                    currentTileIndexes[currentPlayerIndex] = player.currentTileIndex;
                    Debug.Log($"📍 Đồng bộ vị trí {player.playerName}: {currentTileIndexes[currentPlayerIndex]}");
                }
                
                ShowStatus($"{player.playerName} sử dụng thẻ 'Get Out of Jail Free' và được thả");
                yield return new WaitForSeconds(2f);
                
                // Chỉ thả ra tù, không roll dice và di chuyển
                isMoving = false;
                NextTurn();
                yield break;
            }
            
            // Nếu không có thẻ, giảm số lượt tù
            player.jailTurns--;

            if (player.jailTurns <= 0)
            {
                Debug.Log($"🔓 {player.playerName} hết lượt tù, được thả ra");
                player.GetOutOfJail();
                
                // Đồng bộ lại vị trí trong GameManager sau khi ra tù
                if (currentTileIndexes != null && currentPlayerIndex >= 0 && currentPlayerIndex < currentTileIndexes.Length)
                {
                    currentTileIndexes[currentPlayerIndex] = player.currentTileIndex;
                    Debug.Log($"📍 Đồng bộ vị trí {player.playerName}: {currentTileIndexes[currentPlayerIndex]}");
                }
                
                ShowStatus($"{player.playerName} đã hết lượt trong tù và được thả");
                yield return new WaitForSeconds(2f);
                
                // Chỉ thả ra tù, không roll dice và di chuyển
                isMoving = false;
                NextTurn();
                yield break;
            }
            else
            {
                Debug.Log($"🔒 {player.playerName} vẫn ở tù. Còn {player.jailTurns} lượt");
                ShowStatus($"{player.playerName} đang bị tù. Còn {player.jailTurns} lượt");
                yield return new WaitForSeconds(2f);

                isMoving = false;
                NextTurn(); // 👉 Chuyển lượt cho người tiếp theo
                yield break; // ⛔ Dừng coroutine không thực hiện di chuyển
            }
        }
        
        // Kiểm tra và sửa lỗi nếu player bị stuck trong tù
        if (player.inJail && player.jailTurns <= 0)
        {
            Debug.LogWarning($"⚠️ {player.playerName} bị stuck trong tù! Tự động thả ra...");
            player.GetOutOfJail();
            if (currentTileIndexes != null && currentPlayerIndex >= 0 && currentPlayerIndex < currentTileIndexes.Length)
            {
                currentTileIndexes[currentPlayerIndex] = player.currentTileIndex;
            }
            ShowStatus($"{player.playerName} được tự động thả ra tù do lỗi hệ thống");
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
            AudioManager.Instance.PlayStepTile();
            currentTileIndexes[playerIndex] = (currentTileIndexes[playerIndex] + 1) % mapTiles.Count;

            bool passedGo = currentTileIndexes[playerIndex] == 0 && i < steps - 1;

            Vector3 target = mapTiles[currentTileIndexes[playerIndex]].position;
            if (target.x < player.transform.position.x)
                player.FaceLeft();
            player.transform.position = target;

            yield return new WaitForSeconds(0.3f);

            if (passedGo)
            {
                AudioManager.Instance.PlayReceiveMoney();
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
                    AudioManager.Instance.PlayCardDraw();
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
                    if (detailsPanelController != null)
                    {
                        detailsPanelController.Show(prop, player);
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ detailsPanelController là null! Không thể hiển thị DetailsPanel cho bot");
                    }
                    if (prop.owner == null && player.CanPay(prop.GetPrice()))
                    {
                        player.BuyProperty(prop);
                        ShowStatus($"{player.playerName} {playerType} tự động mua {landedTile.tileName}");
                        yield return new WaitForSeconds(1f); // Chờ để người chơi thấy thông báo
                        
                    }
                    detailsPanelController.QuitPanel(); 
                    isWaitingForPlayerAction = false;
                }
                else
                {
                    // Nếu là người chơi chính, hiển thị và chờ hành động thủ công
                    Debug.Log("Gọi Show DetailsPanel từ GameManager cho: " + prop.data.provinceName);
                    isWaitingForPlayerAction = true;
                    if (detailsPanelController != null)
                    {
                        detailsPanelController.Show(prop, player);
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ detailsPanelController là null! Không thể hiển thị DetailsPanel cho người chơi");
                    }
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
        // Reset trạng thái đặc biệt của người chơi hiện tại
        PlayerController currentPlayer = players[currentPlayerIndex];
        if (currentPlayer != null)
        {
            // Reset các trạng thái đặc biệt
            currentPlayer.skipNextTurn = false;
            currentPlayer.cannotBuyNextTurn = false;
            currentPlayer.canBuyDiscountProperty = false;
            
            // Kiểm tra và xử lý trạng thái tù
            if (currentPlayer.inJail && currentPlayer.jailTurns <= 0)
            {
                Debug.LogWarning($"⚠️ {currentPlayer.playerName} bị stuck trong tù! Tự động thả ra...");
                currentPlayer.GetOutOfJail();
                ShowStatus($"{currentPlayer.playerName} được tự động thả ra tù do lỗi hệ thống");
            }
        }

        // Tìm người chơi tiếp theo (bỏ qua những người đã phá sản)
        int attempts = 0;
        int nextPlayerIndex = currentPlayerIndex;
        int activePlayers = 0;
        
        // Đếm số player còn sống trước
        foreach (var player in players)
        {
            if (!player.isBankrupt && player.gameObject.activeSelf)
            {
                activePlayers++;
            }
        }
        
        // Kiểm tra game over ngay từ đầu
        if (activePlayers <= 1)
        {
            Debug.LogError($"❌ Game Over! Chỉ còn {activePlayers} player còn sống!");
            HandleGameOver();
            return;
        }
        
        do
        {
            nextPlayerIndex = (nextPlayerIndex + 1) % players.Count;
            attempts++;
            
            // Nếu đã kiểm tra hết tất cả players mà không tìm được ai còn sống
            if (attempts >= players.Count)
            {
                Debug.LogError("❌ Không tìm thấy player nào còn sống! Game Over!");
                HandleGameOver();
                return;
            }
        } while (players[nextPlayerIndex].isBankrupt || !players[nextPlayerIndex].gameObject.activeSelf);

        // Cập nhật currentPlayerIndex
        currentPlayerIndex = nextPlayerIndex;

        // Ẩn tất cả arrows trước
        foreach (var player in players)
        {
            player.SetArrowVisible(false);
        }

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
        AudioManager.Instance.PlayTurnStart();
        
        // Hiển thị thông báo phù hợp với trạng thái tù
        if (nextPlayer.inJail)
        {
            ShowStatus($"Tới lượt: {nextPlayer.playerName} {playerType} (Đang ở tù - Còn {nextPlayer.jailTurns} lượt)");
        }
        else
        {
            ShowStatus($"Tới lượt: {nextPlayer.playerName} {playerType}");
        }

        // Reset countdown cho user
        if (!nextPlayer.isBot)
        {
            currentTurnTime = turnTimeLimit;
            if (!nextPlayer.inJail)
            {
                ShowStatus($"{nextPlayer.playerName} - Hãy bấm nút Roll để chơi!");
            }
            else
            {
                ShowStatus($"{nextPlayer.playerName} - Đang ở tù, không thể roll dice!");
            }
        }
        else
        {
            // Bot không cần countdown
            currentTurnTime = 0;
            
            // Kiểm tra xem bot có đang ở tù không
            if (nextPlayer.inJail)
            {
                Debug.Log($"🤖 {nextPlayer.playerName} (Bot) đang ở tù, không thể roll dice");
                ShowStatus($"{nextPlayer.playerName} (Bot) đang ở tù - Còn {nextPlayer.jailTurns} lượt");
                
                // Bot ở tù, chuyển lượt sau một khoảng thời gian
                StartCoroutine(DelayAndNextTurnForJailedBot());
            }
            else
            {
                // Bot bình thường, có thể roll dice
                StartCoroutine(AutoRollForBot());
            }
        }
    }

    private IEnumerator AutoRollForBot()
    {
        yield return new WaitForSeconds(4f);
        if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
        {
            PlayerController currentBot = players[currentPlayerIndex];
            if (currentBot.isBankrupt || !currentBot.gameObject.activeSelf)
            {
                Debug.Log($"💀 {currentBot.playerName} (Bot) đã phá sản, bỏ qua lượt");
                NextTurn();
                yield break;
            }
            bool isBotInBankruptcy = BankruptcyManager.Instance != null && BankruptcyManager.Instance.isInBankruptcyMode;
            bool isBotInJail = currentBot.inJail;
            int activePlayers = 0;
            foreach (var player in players)
            {
                if (!player.isBankrupt && player.gameObject.activeSelf)
                {
                    activePlayers++;
                }
            }
            if (activePlayers <= 1)
            {
                Debug.LogError($"❌ Game Over! Chỉ còn {activePlayers} player còn sống!");
                HandleGameOver();
                yield break;
            }
            // Nếu bot đang ở tù thì chỉ giảm jailTurns và chuyển lượt
            if (isBotInJail)
            {
                Debug.Log($"🤖 {currentBot.playerName} (Bot) đang ở tù, không thể roll dice");
                ShowStatus($"{currentBot.playerName} (Bot) đang ở tù - Còn {currentBot.jailTurns} lượt");
                yield return new WaitForSeconds(2f);
                currentBot.jailTurns--;
                if (currentBot.jailTurns <= 0)
                {
                    currentBot.GetOutOfJail();
                    ShowStatus($"{currentBot.playerName} (Bot) đã hết lượt tù và được thả");
                    yield return new WaitForSeconds(1f);
                }
                NextTurn();
                yield break;
            }
            if (!isWaitingForPlayerAction && !isMoving && !isBotInBankruptcy)
            {
                StartCoroutine(HandleRollAndMove(currentBot));
            }
        }
    }

    /// <summary>
    /// Delay và chuyển lượt cho bot đang ở tù
    /// </summary>
    private IEnumerator DelayAndNextTurnForJailedBot()
    {
        // Chờ một khoảng thời gian để người chơi thấy thông báo
        yield return new WaitForSeconds(3f);
        
        // Chuyển lượt cho người tiếp theo
        NextTurn();
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
            // Kiểm tra bot có còn sống không
            if (currentPlayer.isBankrupt || !currentPlayer.gameObject.activeSelf)
            {
                Debug.Log($"💀 {currentPlayer.playerName} (Bot) đã phá sản, bỏ qua lượt");
                NextTurn();
                return;
            }
            
            // Kiểm tra game over trước
            int activePlayers = 0;
            foreach (var player in players)
            {
                if (!player.isBankrupt && player.gameObject.activeSelf)
                {
                    activePlayers++;
                }
            }
            
            if (activePlayers <= 1)
            {
                Debug.LogError($"❌ Game Over! Chỉ còn {activePlayers} player còn sống!");
                HandleGameOver();
                return;
            }
            
            // Kiểm tra xem bot có đang ở tù không
            if (currentPlayer.inJail)
            {
                Debug.Log($"🤖 {currentPlayer.playerName} (Bot) đang ở tù, không thể roll dice");
                ShowStatus($"{currentPlayer.playerName} (Bot) đang ở tù - Còn {currentPlayer.jailTurns} lượt");
                
                // Bot ở tù, chuyển lượt sau một khoảng thời gian
                StartCoroutine(DelayAndNextTurnForJailedBot());
            }
            else
            {
                // Bot bình thường, có thể roll dice
                StartCoroutine(AutoRollForBot());
            }
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
            
            // Kiểm tra xem player hiện tại có đang ở tù không
            bool isCurrentPlayerInJail = currentPlayerIndex >= 0 && currentPlayerIndex < players.Count && players[currentPlayerIndex].inJail;
            
            if (isCountdownInBankruptcy)
            {
                countdownText.text = IconReplacer.ReplaceEmojis("⏸️ GAME TẠM DỪNG - Đang xử lý phá sản");
                countdownText.color = Color.red;
            }
            else if (isMoving)
            {
                countdownText.text = IconReplacer.ReplaceEmojis("🎲 Đang di chuyển...");
                countdownText.color = Color.yellow;
            }
            else if (isWaitingForPlayerAction)
            {
                countdownText.text = IconReplacer.ReplaceEmojis("⏳ Đang chờ hành động...");
                countdownText.color = Color.cyan;
            }
            else if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count && players[currentPlayerIndex].isBot)
            {
                if (isCurrentPlayerInJail)
                {
                    countdownText.text = IconReplacer.ReplaceEmojis($"🤖 Bot đang ở tù (Còn {players[currentPlayerIndex].jailTurns} lượt)");
                    countdownText.color = new Color(1f, 0.5f, 0f); // Màu cam tùy chỉnh
                }
                else
                {
                    countdownText.text = IconReplacer.ReplaceEmojis("🤖 Lượt của Bot");
                    countdownText.color = Color.green;
                }
            }
            else if (isCurrentPlayerInJail)
            {
                countdownText.text = IconReplacer.ReplaceEmojis($"🔒 Đang ở tù (Còn {players[currentPlayerIndex].jailTurns} lượt)");
                countdownText.color = new Color(1f, 0.5f, 0f); // Màu cam tùy chỉnh
            }
            else if (currentTurnTime > 0)
            {
                int timeLeft = Mathf.CeilToInt(currentTurnTime);
                countdownText.text = IconReplacer.ReplaceEmojis($"⏰ Thời gian: {timeLeft}s");
                countdownText.color = timeLeft <= 10 ? Color.red : Color.white;
            }
            else
            {
                countdownText.text = IconReplacer.ReplaceEmojis("🎮 Sẵn sàng chơi!");
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

    // === CHEAT FUNCTIONS ===
    private void CheatBankruptBots()
    {
        Debug.Log("💸 Cheat key được kích hoạt: Alt + Space + F");
        
        int botCount = 0;
        foreach (var player in players)
        {
            if (player.isBot)
            {
                int oldMoney = player.money;
                // Set tiền âm để tạo ra tình trạng phá sản thực sự
                player.money = -1000; // Thiếu 1000$ để tạo phá sản
                player.isBankrupt = true; // Set trạng thái phá sản
                botCount++;
                
                Debug.Log($"💸 Cheat: Đã set {player.playerName} vào tình trạng phá sản! (Từ {oldMoney}$ xuống -1000$)");
                
                // Kiểm tra phá sản cho bot
                if (BankruptcyManager.Instance != null)
                {
                    BankruptcyManager.Instance.CheckBankruptcy(player);
                }
            }
        }
        
        if (botCount > 0)
        {
            string message = $"💸 Cheat: Đã set {botCount} bot vào tình trạng phá sản!";
            ShowStatus(message);
            ShowInfoHud(message, 3f);
            
            // Refresh bảng player info
            RefreshPlayerInfoDisplay();
        }
        else
        {
            ShowStatus("💸 Cheat: Không có bot nào để set phá sản!");
        }
    }

    private void CheatBankruptBotsWithProperties()
    {
        Debug.Log("💸 Cheat key được kích hoạt: Alt + Space + G");
        
        int botCount = 0;
        foreach (var player in players)
        {
            if (player.isBot)
            {
                int oldMoney = player.money;
                // Set tiền âm để tạo ra tình trạng phá sản thực sự
                player.money = -1000; // Thiếu 1000$ để tạo phá sản
                player.isBankrupt = true; // Set trạng thái phá sản
                botCount++;
                
                Debug.Log($"💸 Cheat: Đã set {player.playerName} vào tình trạng phá sản với tài sản! (Từ {oldMoney}$ xuống -1000$)");
                
                // Kiểm tra phá sản cho bot
                if (BankruptcyManager.Instance != null)
                {
                    BankruptcyManager.Instance.CheckBankruptcy(player);
                }
            }
        }
        
        if (botCount > 0)
        {
            string message = $"💸 Cheat: Đã set {botCount} bot vào tình trạng phá sản với tài sản!";
            ShowStatus(message);
            ShowInfoHud(message, 3f);
            
            // Refresh bảng player info
            RefreshPlayerInfoDisplay();
        }
        else
        {
            ShowStatus("💸 Cheat: Không có bot nào để set phá sản!");
        }
    }

    private void CheatKillRandomBot()
    {
        Debug.Log("💀 Cheat key được kích hoạt: Ctrl + Alt + Space");
        List<PlayerController> aliveBots = new List<PlayerController>();
        foreach (var player in players)
        {
            if (player.isBot && !player.isBankrupt)
            {
                aliveBots.Add(player);
            }
        }
        if (aliveBots.Count == 0)
        {
            ShowStatus("💀 Cheat: Không có bot nào còn sống để giết!");
            return;
        }
        int randomIndex = Random.Range(0, aliveBots.Count);
        PlayerController botToKill = aliveBots[randomIndex];
        bool isCurrentTurn = (players[currentPlayerIndex] == botToKill);
        
        // Xóa tất cả tài sản và nhà/hotel của bot
        foreach (var tile in botToKill.ownedTiles.ToList())
        {
            var propertyTile = tile as PropertyTile;
            if (propertyTile != null)
            {
                // Xóa nhà và hotel
                propertyTile.houseCount = 0;
                propertyTile.hasHotel = false;
                propertyTile.UpdateVisuals();
                Debug.Log($"🏠 Xóa nhà/hotel trên {propertyTile.tileName}");
            }
            tile.owner = null;
            tile.SetOwner(null);
        }
        botToKill.ownedTiles.Clear();
        
        // Kill bot
        botToKill.money = -9999;
        botToKill.isBankrupt = true;
        botToKill.gameObject.SetActive(false);
        
        string message = $"💀 Cheat: Đã giết {botToKill.playerName} và xóa tất cả nhà/hotel!";
        ShowStatus(message);
        ShowInfoHud(message, 3f);
        Debug.Log($"💀 Cheat: Đã giết {botToKill.playerName} (Bot #{randomIndex + 1}/{aliveBots.Count})");
        RefreshPlayerInfoDisplay();
        
        // Nếu bot bị kill đang tới lượt hoặc đang ở tù, phải chuyển lượt để tránh treo game
        if (isCurrentTurn)
        {
            NextTurn();
        }
    }

    /// <summary>
    /// Xử lý khi game kết thúc
    /// </summary>
    private void HandleGameOver()
    {
        // Đếm số player còn sống
        int activePlayers = 0;
        PlayerController winner = null;
        
        foreach (var player in players)
        {
            if (!player.isBankrupt && player.gameObject.activeSelf)
            {
                activePlayers++;
                winner = player;
            }
        }
        
        string message;
        if (activePlayers == 1 && winner != null)
        {
            // Chỉ còn 1 player - người thắng
            message = $"{winner.playerName} đã thắng cuộc! Game sẽ trở về menu chính sau 10 giây...";
        }
        else if (activePlayers == 0)
        {
            // Không còn ai - tất cả đều phá sản
            message = "Tất cả players đều đã phá sản! Game sẽ trở về menu chính sau 10 giây...";
        }
        else if (activePlayers == 2)
        {
            // Còn 2 người - có thể tiếp tục chơi
            message = $"Còn {activePlayers} người chơi. Game sẽ trở về menu chính sau 10 giây...";
        }
        else
        {
            // Trường hợp khác (có thể có lỗi logic)
            message = $"Game kết thúc! Còn {activePlayers} người chơi. Game sẽ trở về menu chính sau 10 giây...";
        }
        
        ShowStatus(message);
        ShowInfoHud(message, 10f);
        isMoving = false;
        isWaitingForPlayerAction = false;
        if (rollButton != null)
        {
            rollButton.gameObject.SetActive(false);
        }
        StartCoroutine(GameOverCountdownAndGoToMenu(10, message));
    }

    private IEnumerator GameOverCountdownAndGoToMenu(int seconds, string message)
    {
        for (int i = seconds; i > 0; i--)
        {
            ShowStatus(message + $" ({i}s)");
            ShowInfoHud(message + $" ({i}s)", 1f);
            yield return new WaitForSeconds(1f);
        }
        ShowStatus("Đang chuyển về menu chính...");
        ShowInfoHud("Đang chuyển về menu chính...", 2f);
        yield return new WaitForSeconds(1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Reset tất cả tài sản về trạng thái ban đầu
    /// </summary>
    private void ResetAllProperties()
    {
        Debug.Log("🔄 Reset tất cả tài sản về trạng thái ban đầu...");
        
        foreach (var tileTransform in mapTiles)
        {
            if (tileTransform != null)
            {
                var propertyTile = tileTransform.GetComponent<PropertyTile>();
                if (propertyTile != null)
                {
                    // Reset owner
                    propertyTile.owner = null;
                    propertyTile.SetOwner(null);
                    
                    // Reset house count và hotel
                    propertyTile.houseCount = 0;
                    propertyTile.hasHotel = false;
                    
                    // Update visuals
                    propertyTile.UpdateVisuals();
                    
                    Debug.Log($"🔄 Reset {propertyTile.tileName}: owner=null, houseCount=0, hasHotel=false");
                }
            }
        }
        
        Debug.Log("✅ Đã reset tất cả tài sản!");
    }

    /// <summary>
    /// Refresh bảng thông tin player khi có thay đổi (phá sản, thay đổi tiền, etc.)
    /// </summary>
    public void RefreshPlayerInfoDisplay()
    {
        if (playerInfo != null)
        {
            playerInfo.RefreshPlayerInfo();
        }
    }
}
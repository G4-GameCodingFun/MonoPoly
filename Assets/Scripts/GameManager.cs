using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
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

    public GameObject botPrefab;
    public GameObject playerPrefab;

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
        }

        ShowStatus($"Bắt đầu game. Tới lượt: {players[0].playerName} {(players[0].isBot ? "(Bot)" : "(Người chơi)")}");
        CheckBotTurn();
    }

    void SetupPlayers()
    {
        players.Clear();

        // Add main player
        if (playerPrefab != null)
        {
            var playerObj = Instantiate(playerPrefab);
            if (playerObj != null)
            {
                var pc = playerObj.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.playerName = PlayerPrefs.GetString("PlayerName", "You");
                    pc.isBot = false;
                    players.Add(pc);
                }
                else
                {
                    Debug.LogError("Player Prefab không có component PlayerController!");
                }
            }
            else
            {
                Debug.LogError("Không thể instantiate Player Prefab!");
            }
        }
        else
        {
            Debug.LogError("Player Prefab chưa được gán!");
        }

        // Add bots
        if (botPrefab != null)
        {
            while (players.Count < 4)
            {
                var botObj = Instantiate(botPrefab);
                if (botObj != null)
                {
                    var botPc = botObj.GetComponent<PlayerController>();
                    if (botPc != null)
                    {
                        botPc.playerName = "Bot " + players.Count;
                        botPc.isBot = true;
                        botPc.money = 2000;
                        players.Add(botPc);
                    }
                    else
                    {
                        Debug.LogError("Bot Prefab không có component PlayerController!");
                        break;
                    }
                }
                else
                {
                    Debug.LogError("Không thể instantiate Bot Prefab!");
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("Bot Prefab chưa được gán!");
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
        if (rollButton != null)
        {
            rollButton.interactable = !isMoving && IsCurrentPlayerLocal();
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
        if (!isMoving && IsCurrentPlayerLocal())
        {
            StartCoroutine(HandleRollAndMove(players[currentPlayerIndex]));
        }
    }

    private IEnumerator HandleRollAndMove(PlayerController player)
    {
        isMoving = true;

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
            yield return new WaitForSeconds(2f);
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

        for (int i = 0; i < steps; i++)
        {
            currentTileIndexes[playerIndex] = (currentTileIndexes[playerIndex] + 1) % mapTiles.Count;

            bool passedGo = currentTileIndexes[playerIndex] == 0 && i < steps - 1;

            Vector3 target = mapTiles[currentTileIndexes[playerIndex]].position;
            player.transform.position = target;

            yield return new WaitForSeconds(0.3f);

            if (passedGo)
            {
                player.money += 200;
                ShowStatus($"{player.playerName} {playerType} đi qua Xuất Phát và nhận 200$");
                yield return new WaitForSeconds(1f);
            }
        }

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

            // Hiện DetailsPanel nếu là người chơi thật và là PropertyTile chưa có chủ
            if (prop != null && detailsPanelController != null)
            {
                Debug.Log("Gọi Show DetailsPanel từ GameManager cho: " + prop.data.provinceName);
                isWaitingForPlayerAction = true;
                detailsPanelController.Show(prop, player);
                yield return new WaitUntil(() => !isWaitingForPlayerAction);
            }

            if (player.isBot && prop != null && prop.owner == null && player.CanPay(prop.GetPrice()))
            {
                player.BuyProperty(prop);
                ShowStatus($"{player.playerName} {playerType} tự động mua {landedTile.tileName}");
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            ShowStatus($"Ô {player.currentTileIndex} không có component Tile.");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1.5f);
    }

    private void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        PlayerController nextPlayer = players[currentPlayerIndex];
        string playerType = nextPlayer.isBot ? "(Bot)" : "(Người chơi)";
        ShowStatus($"Tới lượt: {nextPlayer.playerName} {playerType}");

        if (nextPlayer.isBot)
        {
            StartCoroutine(AutoRollForBot());
        }
    }

    private IEnumerator AutoRollForBot()
    {
        yield return new WaitForSeconds(2f);
        StartCoroutine(HandleRollAndMove(players[currentPlayerIndex]));
    }

    private void ShowStatus(string message)
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
}
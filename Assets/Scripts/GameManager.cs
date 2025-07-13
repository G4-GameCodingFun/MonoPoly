using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Netcode;
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

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public List<Transform> mapTiles;
    public List<NetworkObject> players = new List<NetworkObject>();
    public float moveSpeed = 15f;
    public CardManager cardManager;
    public Transform jailPosition;

    public NetworkVariable<int>[] currentTileIndexes;
    public NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(0);
    public NetworkVariable<bool> IsMoving = new NetworkVariable<bool>(false);
    public NetworkVariable<int> DiceTotal = new NetworkVariable<int>(0);

    public Dice dice1;
    public Dice dice2;
    public List<int> dieRolls = new List<int>();

    public TextMeshProUGUI statusText;
    public Button rollButton;

    public GameObject botPrefab;

    private string savePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        savePath = Application.persistentDataPath + "/gameSession.json";

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (dice1 != null) dice1.gameObject.SetActive(false);
        if (dice2 != null) dice2.gameObject.SetActive(false);

        if (rollButton != null)
        {
            rollButton.onClick.AddListener(RollDice);
            rollButton.interactable = false;
        }

        if (IsServer)
        {
            // Add host player
            var hostPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (hostPlayer != null)
            {
                var pc = hostPlayer.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.playerName = PlayerPrefs.GetString("Username", "Host");
                    pc.isBot = false;
                    players.Add(hostPlayer);
                }
            }

            // Spawn bots
            while (players.Count < 4)
            {
                var botObj = Instantiate(botPrefab);
                botObj.GetComponent<NetworkObject>().Spawn();
                var pc = botObj.GetComponent<PlayerController>();
                if (pc != null)
                {
                    pc.playerName = "Bot " + (players.Count + 1);
                    pc.isBot = true;
                    pc.money.Value = 2000;
                    players.Add(botObj.GetComponent<NetworkObject>());
                }
            }

            currentTileIndexes = new NetworkVariable<int>[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                currentTileIndexes[i] = new NetworkVariable<int>(0);
            }

            if (IsSinglePlayerMode() && File.Exists(savePath))
            {
                LoadGame();
            }

            NetworkObjectReference[] playerRefs = players.Select(p => new NetworkObjectReference(p)).ToArray();
            SyncPlayersClientRpc(playerRefs);

            if (players.Count > 0)
            {
                var firstPc = players[0].GetComponent<PlayerController>();
                ShowStatusClientRpc($"Bắt đầu game. Tới lượt: {firstPc.playerName} {(firstPc.isBot ? "(Bot)" : "(Người chơi)")}");

                CheckBotTurn();
            }
        }
    }

    [ClientRpc]
    private void SyncPlayersClientRpc(NetworkObjectReference[] playerRefs)
    {
        players.Clear();
        foreach (var refObj in playerRefs)
        {
            if (refObj.TryGet(out NetworkObject playerObj))
            {
                players.Add(playerObj);
            }
        }
    }

    private void Update()
    {
        if (rollButton != null)
        {
            rollButton.interactable = !IsMoving.Value && IsCurrentPlayerLocal();
        }
    }

    private bool IsCurrentPlayerLocal()
    {
        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer == null) return false;
        return players[currentPlayerIndex.Value] == localPlayer;
    }

    private bool IsSinglePlayerMode()
    {
        int humanCount = players.Count(p => !p.GetComponent<PlayerController>().isBot);
        return humanCount <= 1;
    }

    public void RollDice()
    {
        if (!IsMoving.Value && IsCurrentPlayerLocal())
        {
            RollDiceAndMoveServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RollDiceAndMoveServerRpc()
    {
        if (IsMoving.Value) return;

        var currentPlayer = GetCurrentPlayer();

        string playerType = currentPlayer.isBot ? "(Bot)" : "(Người chơi)";
        ShowStatusClientRpc($"Tới lượt: {currentPlayer.playerName} {playerType}");

        if (currentPlayer.skipNextTurn.Value)
        {
            currentPlayer.skipNextTurn.Value = false;
            ShowStatusClientRpc($"{currentPlayer.playerName} {playerType} bị mất lượt!");
            NextTurn();
            return;
        }

        if (currentPlayer.inJail.Value)
        {
            currentPlayer.jailTurns.Value--;
            if (currentPlayer.jailTurns.Value > 0)
            {
                ShowStatusClientRpc($"{currentPlayer.playerName} {playerType} đang ở tù. Còn {currentPlayer.jailTurns.Value} lượt.");
                NextTurn();
                return;
            }
            else
            {
                ShowStatusClientRpc($"{currentPlayer.playerName} {playerType} đã ra tù.");
                currentPlayer.inJail.Value = false;
            }
        }

        StartCoroutine(HandleRollAndMove(currentPlayer));
    }

    private IEnumerator HandleRollAndMove(PlayerController player)
    {
        IsMoving.Value = true;

        DiceTotal.Value = 0;
        dieRolls.Clear();

        ShowDiceClientRpc(true);

        StartCoroutine(DieRolling(1));
        StartCoroutine(DieRolling(2));

        while (dieRolls.Count < 2)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        string playerType = player.isBot ? "(Bot)" : "(Người chơi)";
        ShowStatusClientRpc($"{player.playerName} {playerType} rolled: {DiceTotal.Value} ({dieRolls[0]} + {dieRolls[1]})");

        yield return new WaitForSeconds(1f);

        ShowDiceClientRpc(false);

        int playerIndex = players.IndexOf(player.NetworkObject);
        yield return StartCoroutine(MovePlayer(playerIndex, DiceTotal.Value));

        IsMoving.Value = false;

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
            DisplayDiceClientRpc(diceNumber, rolledValue);
            yield return new WaitForSeconds(0.05f);
        }
        AddDiceRoll(rolledValue);
    }

    public void AddDiceRoll(int value)
    {
        DiceTotal.Value += value;
        dieRolls.Add(value);
    }

    [ClientRpc]
    private void DisplayDiceClientRpc(int diceNumber, int rolledValue)
    {
        Dice dice = diceNumber == 1 ? dice1 : dice2;
        if (dice != null)
        {
            dice.DisplayDice(rolledValue);
        }
    }

    private IEnumerator MovePlayer(int playerIndex, int steps)
    {
        PlayerController player = players[playerIndex].GetComponent<PlayerController>();
        string playerType = player.isBot ? "(Bot)" : "(Người chơi)";

        for (int i = 0; i < steps; i++)
        {
            currentTileIndexes[playerIndex].Value = (currentTileIndexes[playerIndex].Value + 1) % mapTiles.Count;

            bool passedGo = currentTileIndexes[playerIndex].Value == 0 && i < steps - 1;

            Vector3 target = mapTiles[currentTileIndexes[playerIndex].Value].position;
            MovePlayerPositionClientRpc(new NetworkObjectReference(players[playerIndex]), target);

            yield return new WaitForSeconds(0.3f);

            if (passedGo)
            {
                player.money.Value += 200;
                ShowStatusClientRpc($"{player.playerName} {playerType} đi qua Xuất Phát và nhận 200$");
                yield return new WaitForSeconds(1f);
            }
        }

        player.currentTileIndex.Value = currentTileIndexes[playerIndex].Value;

        Tile landedTile = mapTiles[player.currentTileIndex.Value].GetComponent<Tile>();
        if (landedTile != null)
        {
            PropertyTile prop = landedTile as PropertyTile;
            if (prop != null && prop.owner != null)
            {
                ShowStatusClientRpc($"Nhà đã có chủ: {prop.owner.playerName}");
                yield return new WaitForSeconds(1f);
            }

            landedTile.OnPlayerLanded(player);

            if (player.isBot && prop != null && prop.owner == null && player.CanPay(prop.GetPrice()))
            {
                player.BuyServerRpc(new NetworkObjectReference(landedTile.NetworkObject));
                ShowStatusClientRpc($"{player.playerName} {playerType} tự động mua {landedTile.tileName}");
                yield return new WaitForSeconds(1f);
            }
        }
        else
        {
            ShowStatusClientRpc($"Ô {player.currentTileIndex.Value} không có component Tile.");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1.5f);
    }

    private void NextTurn()
    {
        currentPlayerIndex.Value = (currentPlayerIndex.Value + 1) % players.Count;
        PlayerController nextPlayer = GetCurrentPlayer();
        if (nextPlayer == null) return;

        string playerType = nextPlayer.isBot ? "(Bot)" : "(Người chơi)";
        ShowStatusClientRpc($"Tới lượt: {nextPlayer.playerName} {playerType}");

        if (IsServer)
        {
            SaveGame();
        }

        if (nextPlayer.isBot)
        {
            StartCoroutine(AutoRollForBot());
        }
    }

    private IEnumerator AutoRollForBot()
    {
        yield return new WaitForSeconds(2f);
        RollDiceAndMoveServerRpc();
    }

    public PlayerController GetCurrentPlayer()
    {
        if (players.Count == 0 || currentPlayerIndex.Value >= players.Count)
        {
            Debug.LogError("No valid player");
            return null;
        }
        return players[currentPlayerIndex.Value].GetComponent<PlayerController>();
    }

    public void SaveGame()
    {
        GameState state = new GameState();
        state.currentPlayerIndex = currentPlayerIndex.Value;
        state.dieRolls = new List<int>(dieRolls);
        state.diceTotal = DiceTotal.Value;

        state.playersState = new List<GameState.PlayerState>();
        for (int i = 0; i < players.Count; i++)
        {
            PlayerController p = players[i].GetComponent<PlayerController>();
            GameState.PlayerState ps = new GameState.PlayerState();
            ps.playerName = p.playerName;
            ps.money = p.money.Value;
            ps.currentTileIndex = p.currentTileIndex.Value;
            ps.inJail = p.inJail.Value;
            ps.jailTurns = p.jailTurns.Value;
            ps.skipNextTurn = p.skipNextTurn.Value;
            ps.cannotBuyNextTurn = p.cannotBuyNextTurn.Value;
            ps.canBuyDiscountProperty = p.canBuyDiscountProperty.Value;
            ps.hasGetOutOfJailFreeCard = p.hasGetOutOfJailFreeCard.Value;
            ps.isBot = p.isBot;
            ps.ownedTileIndexes = new List<int>();
            foreach (var tile in p.ownedTiles)
            {
                int index = mapTiles.IndexOf(tile.transform);
                if (index != -1) ps.ownedTileIndexes.Add(index);
            }
            state.playersState.Add(ps);
        }

        string json = JsonUtility.ToJson(state);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved to " + savePath);
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            GameState state = JsonUtility.FromJson<GameState>(json);
            currentPlayerIndex.Value = state.currentPlayerIndex;
            dieRolls = new List<int>(state.dieRolls);
            DiceTotal.Value = state.diceTotal;

            for (int i = 0; i < state.playersState.Count; i++)
            {
                if (i >= players.Count) break;

                PlayerController p = players[i].GetComponent<PlayerController>();
                var ps = state.playersState[i];
                p.playerName = ps.playerName;
                p.money.Value = ps.money;
                p.currentTileIndex.Value = ps.currentTileIndex;
                p.inJail.Value = ps.inJail;
                p.jailTurns.Value = ps.jailTurns;
                p.skipNextTurn.Value = ps.skipNextTurn;
                p.cannotBuyNextTurn.Value = ps.cannotBuyNextTurn;
                p.canBuyDiscountProperty.Value = ps.canBuyDiscountProperty;
                p.hasGetOutOfJailFreeCard.Value = ps.hasGetOutOfJailFreeCard;
                p.isBot = ps.isBot;
                p.ownedTiles.Clear();
                foreach (int index in ps.ownedTileIndexes)
                {
                    if (index < mapTiles.Count)
                    {
                        Tile tile = mapTiles[index].GetComponent<Tile>();
                        tile.owner = p;
                        p.ownedTiles.Add(tile);
                    }
                }
            }

            for (int i = 0; i < players.Count; i++)
            {
                currentTileIndexes[i].Value = players[i].GetComponent<PlayerController>().currentTileIndex.Value;
            }

            Debug.Log("Game loaded from " + savePath);
            PlayerController currentPlayer = GetCurrentPlayer();
            string playerType = currentPlayer.isBot ? "(Bot)" : "(Người chơi)";
            ShowStatusClientRpc($"Game loaded. Tới lượt: {currentPlayer.playerName} {playerType}");
        }
        else
        {
            Debug.LogWarning("No save file found at " + savePath);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MovePlayerToTileServerRpc(NetworkObjectReference playerRef, int tileIndex)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            Vector3 position = mapTiles[tileIndex].position;
            MovePlayerPositionClientRpc(playerRef, position);
            player.currentTileIndex.Value = tileIndex;
            int playerIndex = players.IndexOf(playerObj);
            currentTileIndexes[playerIndex].Value = tileIndex;

            Tile tile = mapTiles[tileIndex].GetComponent<Tile>();
            if (tile != null)
            {
                PropertyTile prop = tile as PropertyTile;
                if (prop != null && prop.owner != null)
                {
                    ShowStatusClientRpc($"Nhà đã có chủ: {prop.owner.playerName}");
                }
                tile.OnPlayerLanded(player);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MovePlayerByStepsServerRpc(NetworkObjectReference playerRef, int steps)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            int newIndex = (player.currentTileIndex.Value + steps) % mapTiles.Count;
            if (newIndex < 0) newIndex += mapTiles.Count;
            ShowStatusClientRpc($"{player.playerName} di chuyển {steps} bước đến ô {newIndex}");
            MovePlayerToTileServerRpc(playerRef, newIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MovePlayerToMostExpensivePropertyServerRpc(NetworkObjectReference playerRef)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            int maxPrice = -1;
            int targetIndex = -1;

            for (int i = 0; i < mapTiles.Count; i++)
            {
                var prop = mapTiles[i].GetComponent<PropertyTile>();
                if (prop != null && prop.GetPrice() > maxPrice)
                {
                    maxPrice = prop.GetPrice();
                    targetIndex = i;
                }
            }

            if (targetIndex != -1)
            {
                ShowStatusClientRpc($"{player.playerName} di chuyển đến tài sản đắt nhất (ô {targetIndex})");
                MovePlayerToTileServerRpc(playerRef, targetIndex);
            }
            else
            {
                ShowStatusClientRpc("Không tìm thấy ô tài sản nào.");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void MovePlayerToNearestTileWithTagServerRpc(NetworkObjectReference playerRef, string tag)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            int start = player.currentTileIndex.Value;
            for (int offset = 1; offset < mapTiles.Count; offset++)
            {
                int index = (start + offset) % mapTiles.Count;
                if (mapTiles[index].CompareTag(tag))
                {
                    ShowStatusClientRpc($"{player.playerName} di chuyển đến ô gần nhất với tag {tag} (ô {index})");
                    MovePlayerToTileServerRpc(playerRef, index);
                    return;
                }
            }

            ShowStatusClientRpc($"Không tìm thấy ô có tag: {tag}");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PayEveryoneServerRpc(NetworkObjectReference payerRef, int amountEach)
    {
        if (payerRef.TryGet(out NetworkObject payerObj))
        {
            PlayerController payer = payerObj.GetComponent<PlayerController>();
            foreach (var otherObj in players)
            {
                PlayerController other = otherObj.GetComponent<PlayerController>();
                if (other != payer)
                {
                    if (payer.money.Value >= amountEach)
                    {
                        payer.money.Value -= amountEach;
                        other.money.Value += amountEach;
                        ShowStatusClientRpc($"{payer.playerName} trả {amountEach}$ cho {other.playerName}");
                    }
                    else
                    {
                        ShowStatusClientRpc($"{payer.playerName} không đủ tiền để trả {amountEach}$ cho {other.playerName}");
                    }
                }
            }
        }
    }

    public void DrawChanceCard() => cardManager.DrawCoHoiCard(GetCurrentPlayer());
    public void DrawKhiVanCard() => cardManager.DrawKhiVanCard(GetCurrentPlayer());

    [ServerRpc(RequireOwnership = false)]
    public void NextTurnServerRpc()
    {
        NextTurn();
    }

    [ClientRpc]
    private void ShowDiceClientRpc(bool show)
    {
        if (dice1 != null) dice1.gameObject.SetActive(show);
        if (dice2 != null) dice2.gameObject.SetActive(show);
    }

    [ClientRpc]
    private void MovePlayerPositionClientRpc(NetworkObjectReference playerRef, Vector3 target)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            Transform transform = playerObj.transform;
            StartCoroutine(MoveToPosition(transform, target));
        }
    }

    private IEnumerator MoveToPosition(Transform transform, Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    [ClientRpc]
    private void ShowStatusClientRpc(string message)
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

    private void OnClientDisconnect(ulong clientId)
    {
        var disconnectedPlayer = players.FirstOrDefault(p => p.OwnerClientId == clientId);
        if (disconnectedPlayer != null)
        {
            players.Remove(disconnectedPlayer);
            disconnectedPlayer.Despawn();
        }

        if (IsServer)
        {
            if (IsSinglePlayerMode() || NetworkManager.Singleton.ConnectedClients.Count <= 1)
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log("Game session deleted on disconnect in singleplayer mode.");
                }
            }
            else
            {
                SaveGame();
                Debug.Log("Game session saved on disconnect in multiplayer mode.");
            }

            if (players.Count == 0)
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (IsServer)
        {
            if (IsSinglePlayerMode())
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    Debug.Log("Game session deleted on quit in singleplayer mode.");
                }
            }
            else
            {
                SaveGame();
            }
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    private void CheckBotTurn()
    {
        PlayerController currentPlayer = GetCurrentPlayer();
        if (currentPlayer != null && currentPlayer.isBot)
        {
            StartCoroutine(AutoRollForBot());
        }
    }
}
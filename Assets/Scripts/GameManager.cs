using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public List<Transform> mapTiles; // Giả sử mapTiles là shared, không cần sync
    public List<NetworkObject> players; // Sử dụng NetworkObject cho players
    public static GameManager Instance;
    public float moveSpeed = 15f;
    public CardManager cardManager;
    public Transform jailPosition;

    public NetworkVariable<int>[] currentTileIndexes; // Sync array cho từng player
    public NetworkVariable<int> currentPlayerIndex = new NetworkVariable<int>(0);
    private bool isMoving = false;
    public PlayerController currentPlayer;

    public Dice dice1;
    public Dice dice2;
    public List<int> dieRolls = new List<int>();
    public NetworkVariable<int> DiceTotal = new NetworkVariable<int>(0);

    public TextMeshProUGUI statusText; // UI cần sync nếu multiplayer, nhưng giả sử local UI

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentTileIndexes = new NetworkVariable<int>[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                currentTileIndexes[i] = new NetworkVariable<int>(0);
            }
        }

        // Hide dice initially (local)
        if (dice1 != null) dice1.gameObject.SetActive(false);
        if (dice2 != null) dice2.gameObject.SetActive(false);

        if (statusText == null)
        {
            Debug.LogWarning("StatusText not assigned! Please assign in Inspector.");
        }
    }

    public int CurrentPlayerIndex => currentPlayerIndex.Value;

    public PlayerController GetCurrentPlayer()
    {
        return players[currentPlayerIndex.Value].GetComponent<PlayerController>();
    }

    public void AddDiceRoll(int value)
    {
        DiceTotal.Value += value;
        dieRolls.Add(value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RollDiceAndMoveServerRpc()
    {
        if (isMoving) return;

        currentPlayer = GetCurrentPlayer();

        // Check skip turn
        if (currentPlayer.skipNextTurn.Value)
        {
            currentPlayer.skipNextTurn.Value = false;
            ShowStatusClientRpc($"{currentPlayer.playerName} bị mất lượt!");
            currentPlayerIndex.Value = (currentPlayerIndex.Value + 1) % players.Count;
            return;
        }

        if (currentPlayer.inJail.Value)
        {
            currentPlayer.jailTurns.Value--;
            if (currentPlayer.jailTurns.Value > 0)
            {
                ShowStatusClientRpc($"{currentPlayer.playerName} đang ở tù. Còn {currentPlayer.jailTurns.Value} lượt.");
                currentPlayerIndex.Value = (currentPlayerIndex.Value + 1) % players.Count;
                return;
            }
            else
            {
                ShowStatusClientRpc($"{currentPlayer.playerName} đã ra tù.");
                currentPlayer.inJail.Value = false;
            }
        }

        StartCoroutine(HandleRollAndMove());
    }

    private IEnumerator HandleRollAndMove()
    {
        isMoving = true;

        // Reset for new roll
        DiceTotal.Value = 0;
        dieRolls.Clear();

        // Show dice (gửi RPC để client show)
        ShowDiceClientRpc(true);

        // Roll dice (giả sử dice roll trên server)
        if (dice1 != null) dice1.RollDie();
        if (dice2 != null) dice2.RollDie();

        while (dieRolls.Count < 2)
        {
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);

        ShowStatusClientRpc($"{currentPlayer.playerName} rolled: {DiceTotal.Value} ({dieRolls[0]} + {dieRolls[1]})");

        yield return new WaitForSeconds(1f);

        ShowDiceClientRpc(false);

        yield return StartCoroutine(MovePlayer(currentPlayerIndex.Value, DiceTotal.Value));

        isMoving = false;
    }

    private IEnumerator MovePlayer(int playerIndex, int steps)
    {
        PlayerController player = players[playerIndex].GetComponent<PlayerController>();

        for (int i = 0; i < steps; i++)
        {
            currentTileIndexes[playerIndex].Value++;
            bool passedGo = false;

            if (currentTileIndexes[playerIndex].Value >= mapTiles.Count)
            {
                currentTileIndexes[playerIndex].Value = 0;
                passedGo = true;
            }

            Vector3 target = mapTiles[currentTileIndexes[playerIndex].Value].position;
            MovePlayerPositionClientRpc(player.NetworkObject, target);

            yield return new WaitForSeconds(0.3f);

            if (passedGo)
            {
                player.money.Value += 200;
                ShowStatusClientRpc($"{player.playerName} đi qua Xuất Phát và nhận 200$");
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
            landedTile.OnPlayerLanded(player); // Giả sử OnPlayerLanded sync nếu cần
        }
        else
        {
            ShowStatusClientRpc($"Ô {player.currentTileIndex.Value} không có component Tile.");
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1.5f);

        currentPlayerIndex.Value = (currentPlayerIndex.Value + 1) % players.Count;
        ShowStatusClientRpc($"Tới lượt: {players[currentPlayerIndex.Value].GetComponent<PlayerController>().playerName}");
    }

    [ServerRpc(RequireOwnership = false)]
    public void MovePlayerToTileServerRpc(NetworkObjectReference playerRef, int tileIndex)
    {
        if (playerRef.TryGet(out NetworkObject playerObj))
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();
            Vector3 position = mapTiles[tileIndex].position;
            MovePlayerPositionClientRpc(playerObj, position);
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
            int newIndex = (player.currentTileIndex.Value + steps + mapTiles.Count) % mapTiles.Count;
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
            var others = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            foreach (PlayerController other in others)
            {
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

    public void DrawChanceCard() => cardManager.DrawCoHoiCard(GetCurrentPlayer()); // Giả sử cardManager sync
    public void DrawKhiVanCard() => cardManager.DrawKhiVanCard(GetCurrentPlayer());

    [ServerRpc(RequireOwnership = false)]
    public void NextTurnServerRpc()
    {
        currentPlayerIndex.Value = (currentPlayerIndex.Value + 1) % players.Count;
        ShowStatusClientRpc($"Tới lượt: {players[currentPlayerIndex.Value].GetComponent<PlayerController>().playerName}");
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
        if (statusText != null)
        {
            statusText.text = "";
        }
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
            PlayerController player = playerObj.GetComponent<PlayerController>();
            StartCoroutine(MoveToPosition(player.transform, target));
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
}
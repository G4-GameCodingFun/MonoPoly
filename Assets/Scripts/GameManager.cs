using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    public List<Transform> mapTiles;
    public List<GameObject> players;
    public static GameManager Instance;
    public float moveSpeed = 15f;
    public CardManager cardManager;
    public Transform jailPosition;

    public int[] currentTileIndexes;
    private int currentPlayerIndex = 0;
    private bool isMoving = false;
    public PlayerController currentPlayer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentTileIndexes = new int[players.Count];
        for (int i = 0; i < players.Count; i++)
        {
            currentTileIndexes[i] = 0;
        }
    }

    public int CurrentPlayerIndex => currentPlayerIndex;

    public PlayerController GetCurrentPlayer()
    {
        return players[currentPlayerIndex].GetComponent<PlayerController>();
    }

    public void RollDiceAndMove()
    {
        if (isMoving) return;

        currentPlayer = GetCurrentPlayer();

        // ✅ Check skip turn
        if (currentPlayer.skipNextTurn)
        {
            currentPlayer.skipNextTurn = false;
            Debug.Log($"{currentPlayer.playerName} bị mất lượt!");
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            return;
        }

        if (currentPlayer.inJail)
        {
            currentPlayer.jailTurns--;
            if (currentPlayer.jailTurns > 0)
            {
                Debug.Log($"{currentPlayer.playerName} đang ở tù. Còn {currentPlayer.jailTurns} lượt.");
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                return;
            }
            else
            {
                Debug.Log($"{currentPlayer.playerName} đã ra tù.");
                currentPlayer.inJail = false;
            }
        }

        int dice = RollDice();
        Debug.Log($"{currentPlayer.playerName} rolled: {dice}");

        StartCoroutine(MovePlayer(currentPlayerIndex, dice));
    }

    private int RollDice()
    {
        return Random.Range(1, 7) + Random.Range(1, 7);
    }

    private IEnumerator MovePlayer(int playerIndex, int steps)
    {
        isMoving = true;
        PlayerController player = players[playerIndex].GetComponent<PlayerController>();

        for (int i = 0; i < steps; i++)
        {
            currentTileIndexes[playerIndex]++;
            bool passedGo = false;

            if (currentTileIndexes[playerIndex] >= mapTiles.Count)
            {
                currentTileIndexes[playerIndex] = 0;
                passedGo = true;
            }

            Vector3 target = mapTiles[currentTileIndexes[playerIndex]].position;
            while (Vector3.Distance(player.transform.position, target) > 0.01f)
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            if (passedGo)
            {
                player.money += 200;
                Debug.Log($"💰 {player.playerName} đi qua Xuất Phát và nhận 200$");
            }
        }

        player.currentTileIndex = currentTileIndexes[playerIndex];

        Tile landedTile = mapTiles[player.currentTileIndex].GetComponent<Tile>();
        if (landedTile != null)
            landedTile.OnPlayerLanded(player);
        else
            Debug.LogWarning($"⚠️ Ô {player.currentTileIndex} không có component Tile.");

        yield return new WaitForSeconds(1f);

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        Debug.Log($"👉 Tới lượt: {players[currentPlayerIndex].GetComponent<PlayerController>().playerName}");

        isMoving = false;
    }

    public void MovePlayerToTile(PlayerController player, int tileIndex)
    {
        player.transform.position = mapTiles[tileIndex].position;
        player.currentTileIndex = tileIndex;
        currentTileIndexes[players.IndexOf(player.gameObject)] = tileIndex;

        Tile tile = mapTiles[tileIndex].GetComponent<Tile>();
        if (tile != null) tile.OnPlayerLanded(player);
    }

    public void MovePlayerBySteps(PlayerController player, int steps)
    {
        int newIndex = (player.currentTileIndex + steps + mapTiles.Count) % mapTiles.Count;
        MovePlayerToTile(player, newIndex);
    }

    public void MovePlayerToMostExpensiveProperty(PlayerController player)
    {
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
            MovePlayerToTile(player, targetIndex);
        }
        else
        {
            Debug.LogWarning("❌ Không tìm thấy ô tài sản nào.");
        }
    }

    public void MovePlayerToNearestTileWithTag(PlayerController player, string tag)
    {
        int start = player.currentTileIndex;
        for (int offset = 1; offset < mapTiles.Count; offset++)
        {
            int index = (start + offset) % mapTiles.Count;
            if (mapTiles[index].CompareTag(tag))
            {
                MovePlayerToTile(player, index);
                return;
            }
        }

        Debug.LogWarning($"❌ Không tìm thấy ô có tag: {tag}");
    }

    public void PayEveryone(PlayerController payer, int amountEach)
    {
        foreach (PlayerController other in FindObjectsOfType<PlayerController>())
        {
            if (other != payer)
            {
                if (payer.TryPay(amountEach))
                {
                    other.money += amountEach;
                    Debug.Log($"💸 {payer.playerName} trả {amountEach}$ cho {other.playerName}");
                }
                else
                {
                    Debug.Log($"❌ {payer.playerName} không đủ tiền để trả {amountEach}$ cho {other.playerName}");
                }
            }
        }
    }

    public void DrawChanceCard() => cardManager.DrawCoHoiCard(GetCurrentPlayer());
    public void DrawKhiVanCard() => cardManager.DrawKhiVanCard(GetCurrentPlayer());
    public void NextTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        Debug.Log($"Tới lượt: {players[currentPlayerIndex].GetComponent<PlayerController>().playerName}");
    }
}
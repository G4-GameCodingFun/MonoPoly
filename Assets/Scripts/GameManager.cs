using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    public List<Transform> mapTiles;
    public List<GameObject> players;

    private int[] currentTileIndexes;
    private int currentPlayerIndex = 0;
    private bool isMoving = false;
    public float moveSpeed = 5f;

    public Transform jailPosition;

    void Start()
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogError("Chưa có người chơi nào được gán!");
            return;
        }

        currentTileIndexes = new int[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            currentTileIndexes[i] = 0;
            players[i].transform.position = mapTiles[0].position;
        }
    }

    public void RollDiceAndMove()
    {
        if (isMoving) return;

        PlayerController currentPlayer = players[currentPlayerIndex].GetComponent<PlayerController>();

        // Nếu đang ở tù
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
                Debug.Log($"{currentPlayer.playerName} đã ra tù sau 3 lượt.");
                currentPlayer.inJail = false;
            }
        }

        int diceResult = RollDice();
        Debug.Log($"Player {currentPlayerIndex + 1} rolled: {diceResult}");

        StartCoroutine(MovePlayer(currentPlayerIndex, diceResult));
    }


    private IEnumerator MovePlayer(int playerIndex, int steps)
    {
        isMoving = true;

        PlayerController playerCtrl = players[playerIndex].GetComponent<PlayerController>();

        for (int i = 0; i < steps; i++)
        {
            currentTileIndexes[playerIndex]++;
            bool passedGo = false;

            if (currentTileIndexes[playerIndex] >= mapTiles.Count)
            {
                currentTileIndexes[playerIndex] = 0;
                passedGo = true;
            }

            Vector3 nextPos = mapTiles[currentTileIndexes[playerIndex]].position;

            while (Vector3.Distance(players[playerIndex].transform.position, nextPos) > 0.01f)
            {
                players[playerIndex].transform.position = Vector3.MoveTowards(
                    players[playerIndex].transform.position,
                    nextPos,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            // Tặng tiền khi qua "GO"
            if (passedGo)
            {
                playerCtrl.money += 200;
                Debug.Log($"{playerCtrl.playerName} đi qua ô GO và nhận 200$");
            }
        }

        // ✅ Khi đã di chuyển đến ô cuối cùng
        GameObject tileObject = mapTiles[currentTileIndexes[playerIndex]].gameObject;
        Tile landedTile = tileObject.GetComponent<Tile>();

        if (landedTile != null)
        {
            landedTile.OnPlayerLanded(playerCtrl); // Xử lý chung cho tất cả loại ô
        }
        else
        {
            Debug.LogWarning($"Tile tại index {currentTileIndexes[playerIndex]} không có component Tile.");
        }

        yield return new WaitForSeconds(1f); // Chờ 1s rồi mới tới lượt người kế tiếp

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        Debug.Log($"Now it's Player {currentPlayerIndex + 1}'s turn!");

        isMoving = false;
    }

    private int RollDice()
    {
        return Random.Range(1, 7) + Random.Range(1, 7); // 2 xúc xắc
    }
    public int CurrentPlayerIndex => currentPlayerIndex;
}
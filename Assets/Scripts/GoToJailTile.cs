using UnityEngine;

public class GoToJailTile : Tile
{
    public Transform jailPosition;

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null)
        {
            Debug.LogWarning("❌ Player null tại ô GoToJailTile");
            return;
        }

        if (jailPosition == null)
        {
            Debug.LogWarning("❌ jailPosition chưa được gán trên GoToJailTile");
            return;
        }

        player.GoToJail();
        player.currentTileIndex = GameManager.Instance.mapTiles.IndexOf(jailPosition);
        GameManager.Instance.currentTileIndexes[GameManager.Instance.players.IndexOf(player.gameObject)] = player.currentTileIndex;

        Debug.Log($"🚨 {player.playerName} đã bị đưa vào tù!");
    }
}
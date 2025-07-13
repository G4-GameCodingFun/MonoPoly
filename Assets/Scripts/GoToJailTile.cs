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

        player.GoToJailServerRpc();
        player.currentTileIndex.Value = GameManager.Instance.mapTiles.IndexOf(jailPosition);
        int playerIndex = GameManager.Instance.players.IndexOf(player.NetworkObject);
        GameManager.Instance.currentTileIndexes[playerIndex].Value = player.currentTileIndex.Value;

        Debug.Log($"🚨 {player.playerName} đã bị đưa vào tù!");
    }
}
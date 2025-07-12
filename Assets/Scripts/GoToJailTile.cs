using UnityEngine;

public class GoToJailTile : Tile
{
    public Transform jailPosition;

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null || jailPosition == null) return;

        player.GoToJail(jailPosition);
        Debug.Log($"{player.playerName} đã bị đưa vào tù!");
    }
}
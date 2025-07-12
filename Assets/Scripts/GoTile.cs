using UnityEngine;


public class GoTile : Tile
{
    public int rewardAmount = 200;

    public void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;

        player.money += rewardAmount;
        Debug.Log($"{player.playerName} nhận {rewardAmount}$ khi đi qua ô GO.");
    }
}

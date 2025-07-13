using UnityEngine;

public class GoTile : Tile
{
    public int rewardAmount = 200;

    public override int GetPrice() => 0; // Go không mua được
    public override int GetRent() => 0; // Go không thuê được
    public override int GetMortgageValue() => 0; // Go không thế chấp được

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;

        player.money += rewardAmount;
        Debug.Log($"{player.playerName} nhận {rewardAmount}$ khi đi qua ô GO.");
    }
}
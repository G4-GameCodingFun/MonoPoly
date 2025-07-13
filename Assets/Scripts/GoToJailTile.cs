using UnityEngine;

public class GoToJailTile : Tile
{
    public override int GetPrice() => 0; // GoToJail không mua được
    public override int GetRent() => 0; // GoToJail không thuê được
    public override int GetMortgageValue() => 0; // GoToJail không thế chấp được

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;
        player.GoToJail();
        Debug.Log($"{player.playerName} đã bị đưa vào tù!");
    }
}
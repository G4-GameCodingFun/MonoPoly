using UnityEngine;

public class AirportTile : Tile
{
    public int targetTileIndex = 10; // Ô đích mặc định

    public override int GetPrice() => 0; // Airport không mua được
    public override int GetRent() => 0; // Airport không thuê được
    public override int GetMortgageValue() => 0; // Airport không thế chấp được

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;
        
        // Di chuyển player đến ô đích (offline mode)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.MovePlayerToTile(player, targetTileIndex);
            Debug.Log($"{player.playerName} đã bay đến ô {targetTileIndex}!");
        }
    }
}
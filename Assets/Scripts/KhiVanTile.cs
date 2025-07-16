using UnityEngine;

public class KhiVanTile : Tile
{
    public override int GetPrice() => 0; // KhiVan không mua được
    public override int GetRent() => 0; // KhiVan không thuê được
    public override int GetMortgageValue() => 0; // KhiVan không thế chấp được

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;
        
        // Gọi thông qua GameManager để đảm bảo logic nhất quán
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleCommunityChestTile(player);
        }
        else
        {
            Debug.LogWarning("GameManager không được tìm thấy!");
        }
    }
}
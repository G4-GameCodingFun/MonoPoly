using UnityEngine;

public class KhiVanTile : Tile
{
    public override int GetPrice() => 0; // KhiVan không mua được
    public override int GetRent() => 0; // KhiVan không thuê được
    public override int GetMortgageValue() => 0; // KhiVan không thế chấp được

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;
        // Gọi trực tiếp CardManager để rút thẻ khi vận
        var cardManager = GameObject.FindAnyObjectByType<CardManager>();
        if (cardManager != null)
        {
            cardManager.DrawKhiVanCard(player);
        }
    }
}
using UnityEngine;

public class ChanceTile : Tile
{
    public override int GetPrice() => 0; // Chance không mua được
    public override int GetRent() => 0; // Chance không thuê được
    public override int GetMortgageValue() => 0; // Chance không thế chấp được

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;
        // Gọi trực tiếp CardManager để rút thẻ cơ hội
        //var cardManager = FindObjectOfType<CardManager>();
        //if (cardManager != null)
        //{
        //    cardManager.DrawCoHoiCard(player);
        //}
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HandleChanceTile(player);
        }
        else
        {
            Debug.LogWarning("GameManager không được tìm thấy!");
        }
    }
}

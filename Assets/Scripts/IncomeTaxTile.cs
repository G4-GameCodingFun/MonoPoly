using UnityEngine;

public class IncomeTaxTile : Tile
{
    public int taxAmount = 200;

    public override int GetPrice() => 0; // Tax không mua được
    public override int GetRent() => 0; // Tax không thuê được
    public override int GetMortgageValue() => 0; // Tax không thế chấp được

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;
        
        if (player.money >= taxAmount)
        {
            player.money -= taxAmount;
            Debug.Log($"{player.playerName} đã trả {taxAmount}$ tiền thuế thu nhập.");
        }
        else
        {
            Debug.Log($"{player.playerName} không đủ tiền trả thuế thu nhập ({taxAmount}$)");
        }
        
        if (player.IsBankrupt())
        {
            Debug.Log($"{player.playerName} đã phá sản sau khi trả thuế!");
            // Xử lý logic phá sản nếu cần
        }
    }
}
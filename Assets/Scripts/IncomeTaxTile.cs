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
            // Trả hết tiền còn lại
            int remainingMoney = player.money;
            player.money = 0;
            Debug.Log($"{player.playerName} không đủ tiền trả thuế thu nhập ({taxAmount}$). Đã trả hết {remainingMoney}$");
            
            // Kiểm tra phá sản
            if (BankruptcyManager.Instance != null)
            {
                BankruptcyManager.Instance.CheckBankruptcy(player);
            }
            else
            {
                Debug.LogWarning($"⚠️ BankruptcyManager.Instance là null! {player.playerName} có thể bị phá sản mà không được xử lý.");
            }
        }
    }
}
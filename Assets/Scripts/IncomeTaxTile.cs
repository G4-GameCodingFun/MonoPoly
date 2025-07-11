using UnityEngine;

public class IncomeTaxTile : Tile
{
    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;

        int totalAssets = player.TotalAssetValue();
        int taxAmount = Mathf.RoundToInt(totalAssets * 0.1f);

        if (player.CanPay(taxAmount))
        {
            player.money -= taxAmount;
            Debug.Log($"{player.playerName} nộp thuế thu nhập: {taxAmount}$ (10% tài sản)");
        }
        else
        {
            Debug.Log($"{player.playerName} không đủ tiền nộp thuế thu nhập ({taxAmount}$)");
            Debug.Log($"{player.playerName} phá sản vì không thể nộp {taxAmount}$ thuế thu nhập.");
            player.IsBankrupt();
        }
    }
}

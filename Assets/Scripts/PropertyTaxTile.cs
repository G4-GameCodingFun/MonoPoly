using UnityEngine;

public class PropertyTaxTile : Tile
{
    public int houseTaxAmount = 40;
    public int hotelTaxAmount = 115;

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;

        int totalTax = 0;

        foreach (var tile in player.ownedTiles)
        {
            if (tile is PropertyTile property)
            {
                if (property.hasHotel)
                    totalTax += hotelTaxAmount;
                else
                    totalTax += property.houseCount * houseTaxAmount;
            }
        }

        if (totalTax == 0)
        {
            Debug.Log($"{player.playerName} không có nhà hoặc khách sạn → không phải trả thuế tài sản.");
            return;
        }

        if (player.CanPay(totalTax))
        {
            player.money -= totalTax;
            Debug.Log($"{player.playerName} trả {totalTax}$ tiền **thuế tài sản** ({houseTaxAmount}$/nhà, {hotelTaxAmount}$/khách sạn)");
        }
        else
        {
            Debug.Log($"{player.playerName} không thể trả {totalTax}$ thuế tài sản ngay cả sau khi thế chấp.");
            Debug.Log($"{player.playerName} đã phá sản vì thuế tài sản.");
            player.IsBankrupt();
        }
    }
}
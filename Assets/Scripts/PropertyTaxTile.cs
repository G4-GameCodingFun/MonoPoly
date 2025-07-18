﻿using UnityEngine;

public class PropertyTaxTile : Tile
{
    public int taxAmount = 200;

    public override int GetPrice() => 0; // PropertyTax không mua được
    public override int GetRent() => 0; // PropertyTax không thuê được
    public override int GetMortgageValue() => 0; // PropertyTax không thế chấp được

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;

        // Tính thuế dựa trên tổng giá trị tài sản
        int totalAssetValue = CalculateTotalAssetValue(player);
        int taxAmount = Mathf.RoundToInt(totalAssetValue * 0.1f); // 10% tổng tài sản

        if (player.money >= taxAmount)
        {
            player.money -= taxAmount;
            Debug.Log($"{player.playerName} đã trả {taxAmount}$ thuế tài sản (10% tổng tài sản: {totalAssetValue}$)");
        }
        else
        {
            // Trả hết tiền còn lại
            int remainingMoney = player.money;
            player.money = 0;
            Debug.Log($"{player.playerName} không đủ tiền trả thuế tài sản ({taxAmount}$). Đã trả hết {remainingMoney}$");
            
            // Kiểm tra phá sản
            if (BankruptcyManager.Instance != null)
            {
                BankruptcyManager.Instance.CheckBankruptcy(player);
            }
            else
            {
                Debug.LogWarning($"⚠ BankruptcyManager.Instance là null! {player.playerName} có thể bị phá sản mà không được xử lý.");
            }
        }
    }

    private int CalculateTotalAssetValue(PlayerController player)
    {
        int totalValue = 0;
        
        // Tính giá trị tất cả properties
        foreach (var tile in player.ownedTiles)
        {
            if (tile is PropertyTile propertyTile)
            {
                totalValue += propertyTile.GetPrice();
                // Thêm giá trị nhà/hotel nếu có
                if (propertyTile.houseCount > 0)
                {
                    totalValue += propertyTile.houseCount * propertyTile.GetHouseCost();
                }
                if (propertyTile.hasHotel)
                {
                    totalValue += propertyTile.GetHotelCost();
                }
            }
            else if (tile is UtilityTile utilityTile)
            {
                totalValue += utilityTile.GetPrice();
            }
        }
        
        return totalValue;
    }
}
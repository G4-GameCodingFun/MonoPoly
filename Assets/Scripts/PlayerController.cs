using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerName;
    public int money = 2000;
    public List<Tile> ownedTiles = new List<Tile>();
    public bool inJail = false;
    public int jailTurns = 0;
    public void Pay(PlayerController toPlayer, int amount)
    {
        money -= amount;
        toPlayer.money += amount;
    }

    public void Buy(Tile tile)
    {
        if (money >= tile.GetPrice())
        {
            money -= tile.GetPrice();
            ownedTiles.Add(tile);
            tile.owner = this;
        }
    }

    public bool CanPay(int amount)
    {
        return money >= amount;
    }
    public void GoToJail(Transform jailPosition)
    {
        inJail = true;
        jailTurns = 3; // 3 lượt trong tù
        transform.position = jailPosition.position;
        Debug.Log($"{playerName} bị chuyển vào tù trong 3 lượt.");
    }
    public int TotalAssetValue()
    {
        int total = money;

        foreach (var tile in ownedTiles)
        {
            total += tile.GetPrice();

            if (tile is PropertyTile p)
            {
                total += p.houseCount * p.GetHouseCost();
                if (p.hasHotel) total += p.GetHotelCost();
            }
        }

        return total;
    }
    public bool TryPay(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }

        Debug.Log($"{playerName} không đủ tiền. Đang cố gắng thế chấp tài sản...");

        // Thế chấp tài sản chưa thế chấp
        foreach (var tile in ownedTiles)
        {
            if (!tile.isMortgaged)
            {
                tile.Mortgage();
                Debug.Log($"{playerName} đã thế chấp {tile.tileName} để lấy tiền.");

                if (money >= amount)
                {
                    money -= amount;
                    return true;
                }
            }
        }

        return false; // Vẫn không đủ tiền sau khi thế chấp
    }
    public void IsBankrupt(PlayerController creditor = null)
    {
        Debug.Log($"{playerName} phá sản!");

        // Chuyển tài sản cho chủ nợ nếu có
        if (creditor != null)
        {
            foreach (var tile in ownedTiles)
            {
                tile.owner = creditor;
                creditor.ownedTiles.Add(tile);
            }
        }

        ownedTiles.Clear();
        money = 0;
        gameObject.SetActive(false); // Ẩn nhân vật khỏi game
    }
}
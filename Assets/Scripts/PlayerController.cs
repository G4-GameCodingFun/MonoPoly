using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public string playerName;
    public int money = 2000;
    public int currentTileIndex;

    public bool inJail = false;
    public int jailTurns = 0;

    public bool skipNextTurn = false;
    public bool cannotBuyNextTurn = false;
    public bool canBuyDiscountProperty = false;
    public bool hasGetOutOfJailFreeCard = false;

    public List<Tile> ownedTiles = new List<Tile>();

    public bool TryPay(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            return true;
        }
        return false;
    }

    public void Pay(PlayerController receiver, int amount)
    {
        if (TryPay(amount))
        {
            receiver.money += amount;
            Debug.Log($"💸 {playerName} đã trả {amount}$ cho {receiver.playerName}");
        }
        else
        {
            Debug.LogWarning($"❌ {playerName} không đủ tiền để trả {amount}$ cho {receiver.playerName}");
        }
    }

    public bool CanPay(int amount) => money >= amount;

    public void Buy(Tile tile)
    {
        if (cannotBuyNextTurn)
        {
            Debug.LogWarning($"{playerName} không được phép mua ô này trong lượt này!");
            cannotBuyNextTurn = false; // chỉ 1 lần
            return;
        }

        int price = tile.GetPrice();

        if (canBuyDiscountProperty)
        {
            price /= 2;
            canBuyDiscountProperty = false; // chỉ áp dụng 1 lần
            Debug.Log($"{playerName} được mua {tile.tileName} với giá giảm 50%");
        }

        if (tile.owner == null && CanPay(price))
        {
            TryPay(price);
            tile.owner = this;
            ownedTiles.Add(tile);
            Debug.Log($"🏠 {playerName} đã mua {tile.tileName} với giá {price}$");
        }
        else
        {
            Debug.LogWarning($"❌ {playerName} không thể mua {tile.tileName}");
        }
    }

    public void MoveToStart() => GameManager.Instance.MovePlayerToTile(this, 0);
    public void MoveSteps(int steps) => GameManager.Instance.MovePlayerBySteps(this, steps);
    public void MoveToMostExpensiveProperty() => GameManager.Instance.MovePlayerToMostExpensiveProperty(this);
    public void MoveToNearest(string tag) => GameManager.Instance.MovePlayerToNearestTileWithTag(this, tag);

    public void GoToJail()
    {
        Transform jailTile = GameManager.Instance.jailPosition;
        if (jailTile != null)
        {
            GameManager.Instance.MovePlayerToTile(this, jailTile.GetSiblingIndex());
            inJail = true;
            jailTurns = 3;
            Debug.Log($"{playerName} vào tù 3 lượt");
        }
        else
        {
            Debug.LogWarning("❌ Jail position chưa được gán trong GameManager");
        }
    }

    public void GetOutOfJail()
    {
        inJail = false;
        jailTurns = 0;
        Debug.Log($"✅ {playerName} đã ra tù");
    }

    public void IsBankrupt(PlayerController creditor = null)
    {
        Debug.Log($"💥 {playerName} đã phá sản!");

        foreach (var tile in ownedTiles)
        {
            tile.owner = null;
        }
        ownedTiles.Clear();
        gameObject.SetActive(false);
    }

    public int TotalAssetValue()
    {
        int total = money;
        foreach (var tile in ownedTiles)
        {
            total += tile.GetPrice() / 2;
        }
        return total;
    }
}
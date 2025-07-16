using UnityEngine;

public class UtilityTile : Tile
{
    public int purchasePrice = 150;
    public int mortgageValue = 75;

    public override int GetPrice() => purchasePrice;
    public override int GetRent()
    {
        if (owner == null) return 0;
        
        // Đếm số utility tiles mà owner sở hữu
        int utilityCount = 0;
        foreach (var tile in owner.ownedTiles)
        {
            if (tile is UtilityTile) utilityCount++;
        }
        
        // Tính thuê dựa trên số utility tiles
        if (utilityCount == 1) return 4; // 4x dice roll
        if (utilityCount == 2) return 10; // 10x dice roll
        return 0;
    }
    public override int GetMortgageValue() => mortgageValue;

    public override void OnPlayerLanded(PlayerController player)
    {
        if (player == null) return;

        // Nếu chưa có chủ sở hữu
        if (owner == null)
        {
            if (player.CanPay(purchasePrice))
            {
                player.BuyProperty(this);
                Debug.Log($"{player.playerName} đã mua {tileName} với giá {purchasePrice}$");
            }
            else
            {
                Debug.Log($"{player.playerName} không đủ tiền mua {tileName} (giá {purchasePrice}$)");
            }
        }
        // Nếu đã có chủ khác
        else if (owner != player)
        {
            int rent = GetRent();
            if (rent > 0)
            {
                // Tính thuê dựa trên xúc xắc (giả sử dice roll = 6)
                int diceRoll = 6; // Có thể lấy từ GameManager
                int actualRent = rent * diceRoll;
                
                if (player.CanPay(actualRent))
                {
                    player.PayRent(owner, actualRent);
                    Debug.Log($"{player.playerName} trả {actualRent}$ tiền thuê cho {owner.playerName} (dice: {diceRoll})");
                }
                else
                {
                    player.PayRent(owner, actualRent);
                    Debug.Log($"{player.playerName} không đủ tiền trả {actualRent}$ tiền thuê cho {owner.playerName}");
                }
            }
        }
        // Nếu là chủ sở hữu
        else
        {
            Debug.Log($"{player.playerName} đang đứng trên utility của mình: {tileName}");
        }
    }
}
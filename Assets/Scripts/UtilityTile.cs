using UnityEngine;

public class UtilityTile : Tile
{
    public PropertyData data;
    public override int GetPrice() => data?.purchasePrice ?? 0;

    public override void OnPlayerLanded(PlayerController player)
    {
        if (data == null || player == null) return;

        // Nếu chưa có chủ sở hữu
        if (owner == null)
        {
            int price = GetPrice();
            if (player.CanPay(price))
            {
                player.Buy(this);
                Debug.Log($"{player.playerName} đã mua {tileName} với giá {price}$");
            }
            else
            {
                Debug.Log($"{player.playerName} không đủ tiền mua {tileName}");
            }
            return;
        }

        // Nếu đã có chủ (khác người chơi)
        if (owner != player)
        {
            int diceRoll = Random.Range(1, 7) + Random.Range(1, 7); // 2 xúc xắc

            // Đếm số lượng công ty (Utility) mà chủ sở hữu đang có
            int ownedUtilities = owner.ownedTiles.FindAll(t => t.subType == TileSubType.Utility).Count;

            int multiplier = (ownedUtilities == 2) ? 10 : 4;
            int rent = diceRoll * multiplier;

            Debug.Log($"Xúc xắc: {diceRoll} → Tiền thuê: {rent} (sở hữu {ownedUtilities} công ty)");

            if (player.CanPay(rent))
            {
                player.Pay(owner, rent);
                Debug.Log($"{player.playerName} trả {rent}$ tiền thuê cho {owner.playerName}");
            }
            else
            {
                Debug.Log($"{player.playerName} không thể trả {rent}$ tiền thuê, kể cả sau khi thế chấp.");
                Debug.Log($"{player.playerName} phá sản và bị loại khỏi trò chơi.");
                player.IsBankrupt();
            }
        }
        else
        {
            Debug.Log($"{player.playerName} đứng trên công ty của mình: {tileName}");
        }
    }
}
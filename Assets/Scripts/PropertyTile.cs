using UnityEngine;

public class PropertyTile : Tile
{
    public PropertyData data; // Gán bằng tay trong Unity

    public int houseCount = 0;
    public bool hasHotel = false;

    public override int GetPrice() => data != null ? data.purchasePrice : 0;
    public override int GetRent()
    {
        if (data == null || data.rentByHouse == null)
            return 0;

        if (hasHotel) return data.rentByHouse[5];
        return data.rentByHouse[Mathf.Clamp(houseCount, 0, 4)];
    }

    public int GetHouseCost() => data?.houseCost ?? 0;
    public int GetHotelCost() => data?.hotelCost ?? 0;
    public override int GetMortgageValue() => data?.mortgageValue ?? 0;

    public override void OnPlayerLanded(PlayerController player)
    {
        if (data == null || player == null) return;

        // Nếu chưa có chủ sở hữu
        if (owner == null)
        {
            int price = GetPrice();
            if (player.CanPay(price))
            {
                player.BuyProperty(this);
                Debug.Log($"{player.playerName} đã mua {tileName} với giá {price}$");
            }
            else
            {
                Debug.Log($"{player.playerName} không đủ tiền mua {tileName} (giá {price}$)");
            }
        }
        // Nếu đã có chủ khác
        else if (owner != player)
        {
            int rent = GetRent();
            if (player.CanPay(rent))
            {
                player.PayRent(owner, rent);
                Debug.Log($"{player.playerName} trả {rent}$ tiền thuê cho {owner.playerName}");
            }
            else
            {
                Debug.Log($"{player.playerName} không đủ tiền trả {rent}$ tiền thuê cho {owner.playerName}");
                player.GoBankrupt(owner);
            }
        }
        // Nếu là chủ sở hữu
        else
        {
            Debug.Log($"{player.playerName} đang đứng trên đất của mình: {tileName}");

            // Gợi ý xây nhà nếu có thể
            if (houseCount < 4 && player.CanPay(GetHouseCost()))
            {
                houseCount++;
                player.money -= GetHouseCost();
                Debug.Log($"{player.playerName} xây thêm nhà. Tổng số nhà: {houseCount}");
            }
            else if (houseCount == 4 && !hasHotel && player.CanPay(GetHotelCost()))
            {
                hasHotel = true;
                houseCount = 0;
                player.money -= GetHotelCost();
                Debug.Log($"{player.playerName} đã xây khách sạn tại {tileName}");
            }
            else
            {
                Debug.Log($"Không thể xây thêm nhà hoặc khách sạn tại {tileName}");
            }
        }
    }
}
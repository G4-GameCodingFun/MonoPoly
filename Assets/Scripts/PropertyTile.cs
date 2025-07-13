using Unity.Netcode;
using UnityEngine;

public class PropertyTile : Tile
{
    public PropertyData data; // Gán bằng tay trong Unity

    public NetworkVariable<int> houseCount = new NetworkVariable<int>(0);
    public NetworkVariable<bool> hasHotel = new NetworkVariable<bool>(false);

    public override int GetPrice() => data != null ? data.purchasePrice : 0;
    public override int GetRent()
    {
        if (data == null || data.rentByHouse == null)
            return 0;

        if (hasHotel.Value) return data.rentByHouse[5];
        return data.rentByHouse[Mathf.Clamp(houseCount.Value, 0, 4)];
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
                player.BuyServerRpc(this.NetworkObject);
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
                player.PayServerRpc(owner.NetworkObject, rent);
                Debug.Log($"{player.playerName} trả {rent}$ tiền thuê cho {owner.playerName}");
            }
            else
            {
                Debug.Log($"{player.playerName} không đủ tiền trả {rent}$ tiền thuê cho {owner.playerName}");
                player.IsBankruptServerRpc(owner.NetworkObject);
            }
        }
        // Nếu là chủ sở hữu
        else
        {
            Debug.Log($"{player.playerName} đang đứng trên đất của mình: {tileName}");

            // Gợi ý xây nhà nếu có thể
            if (houseCount.Value < 4 && player.CanPay(GetHouseCost()))
            {
                houseCount.Value++;
                player.money.Value -= GetHouseCost();
                Debug.Log($"{player.playerName} xây thêm nhà. Tổng số nhà: {houseCount.Value}");
            }
            else if (houseCount.Value == 4 && !hasHotel.Value && player.CanPay(GetHotelCost()))
            {
                hasHotel.Value = true;
                houseCount.Value = 0;
                player.money.Value -= GetHotelCost();
                Debug.Log($"{player.playerName} đã xây khách sạn tại {tileName}");
            }
            else
            {
                Debug.Log($"Không thể xây thêm nhà hoặc khách sạn tại {tileName}");
            }
        }
    }
}
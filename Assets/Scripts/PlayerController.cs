using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public string playerName;
    public NetworkVariable<int> money = new NetworkVariable<int>(2000); // Sync tiền, bắt đầu với 2000
    public NetworkVariable<int> currentTileIndex = new NetworkVariable<int>(0); // Sync vị trí
    public NetworkVariable<bool> inJail = new NetworkVariable<bool>(false);
    public NetworkVariable<int> jailTurns = new NetworkVariable<int>(0);
    public NetworkVariable<bool> skipNextTurn = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> cannotBuyNextTurn = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> canBuyDiscountProperty = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> hasGetOutOfJailFreeCard = new NetworkVariable<bool>(false);

    // Danh sách ownedTiles cần sync riêng, có thể dùng NetworkList nếu hỗ trợ, hoặc sync khi cần
    public List<Tile> ownedTiles = new List<Tile>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        // Nếu là owner, có thể set initial values
    }

    [ServerRpc(RequireOwnership = false)] // Cho phép client gọi, nhưng server xử lý
    public void TryPayServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        if (money.Value >= amount)
        {
            money.Value -= amount;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PayServerRpc(NetworkObjectReference receiverRef, int amount)
    {
        if (receiverRef.TryGet(out NetworkObject receiverObj))
        {
            PlayerController receiver = receiverObj.GetComponent<PlayerController>();
            if (money.Value >= amount)
            {
                money.Value -= amount;
                receiver.money.Value += amount;
                Debug.Log($"💸 {playerName} đã trả {amount}$ cho {receiver.playerName}");
            }
            else
            {
                Debug.LogWarning($"❌ {playerName} không đủ tiền để trả {amount}$ cho {receiver.playerName}");
            }
        }
    }

    public bool CanPay(int amount) => money.Value >= amount;

    [ServerRpc(RequireOwnership = false)]
    public void BuyServerRpc(NetworkObjectReference tileRef)
    {
        if (tileRef.TryGet(out NetworkObject tileObj))
        {
            Tile tile = tileObj.GetComponent<Tile>();
            if (cannotBuyNextTurn.Value)
            {
                Debug.LogWarning($"{playerName} không được phép mua ô này trong lượt này!");
                cannotBuyNextTurn.Value = false; // chỉ 1 lần
                return;
            }

            int price = tile.GetPrice();

            if (canBuyDiscountProperty.Value)
            {
                price /= 2;
                canBuyDiscountProperty.Value = false; // chỉ áp dụng 1 lần
                Debug.Log($"{playerName} được mua {tile.tileName} với giá giảm 50%");
            }

            if (tile.owner == null && CanPay(price))
            {
                money.Value -= price;
                tile.owner = this;
                ownedTiles.Add(tile);
                Debug.Log($"🏠 {playerName} đã mua {tile.tileName} với giá {price}$");
            }
            else
            {
                Debug.LogWarning($"❌ {playerName} không thể mua {tile.tileName}");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SellServerRpc(NetworkObjectReference tileRef)
    {
        if (tileRef.TryGet(out NetworkObject tileObj))
        {
            Tile tile = tileObj.GetComponent<Tile>();
            if (tile.owner == this)
            {
                int sellPrice = tile.GetPrice() / 2;
                money.Value += sellPrice;
                ownedTiles.Remove(tile);
                tile.owner = null;
                Debug.Log($"{playerName} đã bán {tile.tileName} với giá {sellPrice}$");
            }
        }
    }

    public void MoveToStart() => GameManager.Instance.MovePlayerToTileServerRpc(NetworkObject, 0);
    public void MoveSteps(int steps) => GameManager.Instance.MovePlayerByStepsServerRpc(NetworkObject, steps);
    public void MoveToMostExpensiveProperty() => GameManager.Instance.MovePlayerToMostExpensivePropertyServerRpc(NetworkObject);
    public void MoveToNearest(string tag) => GameManager.Instance.MovePlayerToNearestTileWithTagServerRpc(NetworkObject, tag);

    [ServerRpc(RequireOwnership = false)]
    public void GoToJailServerRpc()
    {
        Transform jailTile = GameManager.Instance.jailPosition;
        if (jailTile != null)
        {
            GameManager.Instance.MovePlayerToTileServerRpc(NetworkObject, jailTile.GetSiblingIndex());
            inJail.Value = true;
            jailTurns.Value = 3;
            Debug.Log($"{playerName} vào tù 3 lượt");
        }
        else
        {
            Debug.LogWarning("❌ Jail position chưa được gán trong GameManager");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetOutOfJailServerRpc()
    {
        inJail.Value = false;
        jailTurns.Value = 0;
        Debug.Log($"✅ {playerName} đã ra tù");
    }

    [ServerRpc(RequireOwnership = false)]
    public void IsBankruptServerRpc(NetworkObjectReference creditorRef = default)
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
        int total = money.Value;
        foreach (var tile in ownedTiles)
        {
            total += tile.GetPrice() / 2;
        }
        return total;
    }
}
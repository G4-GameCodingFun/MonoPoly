using Unity.Netcode;
using UnityEngine;

public enum TileType
{
    Property,
    Tax,
    Chance,
    KhiVan,
    Jail,
    Go,
    GoToJail,
    Airport,
}

public enum TileSubType
{
    None,
    RegularProperty,
    Utility,
    Railroad
}

[System.Serializable]
public class Tile : NetworkBehaviour
{
    [Header("General Info")]
    public string tileName;
    public TileType type = TileType.Property;
    public TileSubType subType = TileSubType.None;
    public NetworkVariable<bool> isMortgaged = new NetworkVariable<bool>(false); // Sử dụng NetworkVariable để sync

    [Header("Ownership")]
    public PlayerController owner;

    // Virtual methods to be overridden by subclasses
    public virtual int GetPrice() => 0;
    public virtual int GetRent() => 0;
    public virtual void OnPlayerLanded(PlayerController player) { }
    public virtual int GetMortgageValue() => 0;

    [ServerRpc(RequireOwnership = false)]
    public void MortgageServerRpc()
    {
        if (!isMortgaged.Value)
        {
            isMortgaged.Value = true;
            if (owner != null)
            {
                owner.money.Value += GetMortgageValue(); // Sync tiền qua NetworkVariable
                Debug.Log($"{owner.playerName} đã thế chấp {tileName} và nhận {GetMortgageValue()}$");
            }
        }
    }
}
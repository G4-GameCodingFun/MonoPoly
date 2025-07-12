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
public class Tile : MonoBehaviour
{
    [Header("General Info")]
    public string tileName;
    public TileType type = TileType.Property;
    public TileSubType subType = TileSubType.None;
    public bool isMortgaged = false;

    [Header("Ownership")]
    public PlayerController owner;

    // Virtual methods to be overridden by subclasses
    public virtual int GetPrice() => 0;
    public virtual int GetRent() => 0;
    public virtual void OnPlayerLanded(PlayerController player) { }
    public virtual int GetMortgageValue() => 0;
    public virtual void Mortgage()
    {
        if (!isMortgaged)
        {
            isMortgaged = true;
            owner.money += GetMortgageValue();
            Debug.Log($"{owner.playerName} đã thế chấp {tileName} và nhận {GetMortgageValue()}$");
        }
    }
}

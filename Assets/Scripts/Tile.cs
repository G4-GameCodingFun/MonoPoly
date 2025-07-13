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

public abstract class Tile : MonoBehaviour
{
    public string tileName;
    public PlayerController owner;
    public SpriteRenderer spriteRenderer;

    public abstract int GetPrice();
    public abstract int GetRent();
    public abstract int GetMortgageValue();

    public virtual void OnPlayerLanded(PlayerController player)
    {
        Debug.Log($"{player.playerName} đã đứng trên {tileName}");
    }

    public void SetOwner(PlayerController newOwner)
    {
        owner = newOwner;
        if (spriteRenderer != null)
        {
            // Thay đổi màu sắc để thể hiện chủ sở hữu
            if (newOwner != null)
            {
                // Có thể thêm logic thay đổi màu theo player
                spriteRenderer.color = Color.green;
            }
            else
            {
                spriteRenderer.color = Color.white;
            }
        }
    }

    public bool CanBeMortgaged()
    {
        return owner != null && !IsMortgaged();
    }

    public virtual bool IsMortgaged()
    {
        return false; // Override trong các class con nếu cần
    }

    public virtual void Mortgage()
    {
        if (CanBeMortgaged())
        {
            owner.money += GetMortgageValue();
            Debug.Log($"{owner.playerName} đã thế chấp {tileName} và nhận {GetMortgageValue()}$");
        }
    }

    public virtual void Unmortgage()
    {
        if (IsMortgaged() && owner.money >= GetMortgageValue() * 1.1f)
        {
            owner.money -= Mathf.RoundToInt(GetMortgageValue() * 1.1f);
            Debug.Log($"{owner.playerName} đã chuộc {tileName} với giá {Mathf.RoundToInt(GetMortgageValue() * 1.1f)}$");
        }
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "NewPropertyData", menuName = "Monopoly/PropertyData")]
public class PropertyData : ScriptableObject
{
    public string provinceName;

    public int[] rentByHouse = new int[6]; // 0-4 house + hotel
    public int mortgageValue;
    public int houseCost;
    public int hotelCost;
    public int purchasePrice;

    [Header("Phân nhóm màu")]
    public int type;        // Mã nhóm màu (ví dụ: 1 = xanh dương, 2 = đỏ, ...)
    public int groupSize;   // Số lượng ô cùng màu (ví dụ: 2, 3, 4...)
}
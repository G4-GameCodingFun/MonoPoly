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
}
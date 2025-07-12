using UnityEngine;

public class ChanceTile : Tile
{
    public override void OnPlayerLanded(PlayerController player)
    {
        GameManager.Instance.DrawChanceCard();
    }
}

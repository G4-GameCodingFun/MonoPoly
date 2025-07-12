using UnityEngine;

public class KhiVanTile : Tile
{
    public override void OnPlayerLanded(PlayerController player)
    {
        GameManager.Instance.DrawKhiVanCard();
    }
}
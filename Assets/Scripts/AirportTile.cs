using UnityEngine;

public class AirportTile : Tile
{
    public override void OnPlayerLanded(PlayerController player)
    {
        Debug.Log($"✈️ {player.playerName} đã đến sân bay. Đang chọn ô bất kỳ để bay tới...");
        GameManager.Instance.StartCoroutine(FlyToRandomTile(player));
    }

    private System.Collections.IEnumerator FlyToRandomTile(PlayerController player)
    {
        yield return new WaitForSeconds(1f); // delay để tạo cảm giác

        int randomTileIndex = Random.Range(0, GameManager.Instance.mapTiles.Count);
        Debug.Log($"🛬 {player.playerName} bay đến ô {randomTileIndex} - {GameManager.Instance.mapTiles[randomTileIndex].name}");

        GameManager.Instance.MovePlayerToTileServerRpc(player.NetworkObject, randomTileIndex);
    }
}
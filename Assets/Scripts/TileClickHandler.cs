// File: TileClickHandler.cs
using UnityEngine;

public class TileClickHandler : MonoBehaviour
{
    private Tile tile; // Reference đến component Tile trên GameObject này

    private void Start()
    {
        // Lấy component Tile từ GameObject hiện tại
        tile = GetComponent<Tile>();
        if (tile == null)
        {
            Debug.LogWarning("Tile component không tồn tại trên " + gameObject.name);
        }
    }

    private void OnMouseDown()
    {
        // Kiểm tra nếu tile hợp lệ và DetailsManager tồn tại
        if (tile != null && DetailsManager.Instance != null)
        {
            // Gọi phương thức ShowDetails để hiển thị panel
            DetailsManager.Instance.ShowDetails(tile);
            Debug.Log("Đã click vào tile: " + tile.tileName);
        }
    }
}
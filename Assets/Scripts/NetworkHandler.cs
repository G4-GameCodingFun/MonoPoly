using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkHandler : MonoBehaviour
{
    // Chuyển sang offline mode - loại bỏ mọi Netcode
    // Chỉ giữ lại các hàm cần thiết cho offline gameplay
    
    public void HandlePlayerMove(PlayerController player, int targetTileIndex)
    {
        if (player == null) return;
        // Logic di chuyển player offline
        Debug.Log($"{player.playerName} di chuyển đến ô {targetTileIndex}");
    }
    
    public void HandlePlayerAction(PlayerController player, string action)
    {
        if (player == null) return;
        // Logic xử lý hành động player offline
        Debug.Log($"{player.playerName} thực hiện hành động: {action}");
    }
}
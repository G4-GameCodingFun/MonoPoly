using Unity.Netcode;
using UnityEngine;

public class BotPlayer : PlayerController
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isBot = true;
    }

    // Optional: Override for bot-specific behavior, e.g., auto-buy or decision making
    // For example, in OnPlayerLanded (if called in Tile), but since it's in Tile, use isBot check there for auto-buy.
}
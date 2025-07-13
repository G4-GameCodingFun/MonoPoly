// File: NetworkHandler.cs (attach vào GameObject "NetworkManager")
using Unity.Netcode;
using UnityEngine;

public class NetworkHandler : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // Giữ qua scenes
        if (GetComponent<NetworkManager>() == null)
        {
            Debug.LogWarning("NetworkManager component not found. Adding it now.");
            gameObject.AddComponent<NetworkManager>(); // Tự thêm nếu thiếu
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null after adding component. Check if package Netcode is installed.");
            return;
        }

        // Set approval callback để set username khi client join
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsHost && NetworkManager.Singleton != null)
        {
            var hostPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            if (hostPlayer == null)
            {
                Debug.LogWarning("Host player not spawned automatically. Spawning manually...");
                if (NetworkManager.Singleton.NetworkConfig.PlayerPrefab == null)
                {
                    Debug.LogError("Player Prefab is not assigned in NetworkManager. Please assign a valid prefab.");
                    return; // Dừng nếu prefab null
                }
                GameObject playerObj = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab);
                if (playerObj != null)
                {
                    playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
                    hostPlayer = playerObj.GetComponent<NetworkObject>();
                }
                else
                {
                    Debug.LogError("Failed to instantiate player prefab.");
                    return;
                }
            }

            var playerController = hostPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.playerName = PlayerPrefs.GetString("Username", "Host");
                if (GameManager.Instance != null)
                {
                    if (!GameManager.Instance.players.Contains(hostPlayer))
                        GameManager.Instance.players.Add(hostPlayer);
                }
            }
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (NetworkManager.Singleton != null)
        {
            response.Approved = true;
            response.CreatePlayerObject = true;
        }
        else
        {
            Debug.LogError("NetworkManager is null during approval check.");
            response.Approved = false;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.playerName = PlayerPrefs.GetString("Username", "Player" + clientId);
                if (GameManager.Instance != null && !GameManager.Instance.players.Contains(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject))
                {
                    GameManager.Instance.players.Add(NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject);
                }
            }
        }
    }
}
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class MatchmakingManager : MonoBehaviour
{
    private Lobby currentLobby;
    private bool isHost = false;

    public async void CreateLobby()
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Player = new Player { Data = new System.Collections.Generic.Dictionary<string, PlayerDataObject> { { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerPrefs.GetString("Username")) } } },
                Data = new System.Collections.Generic.Dictionary<string, DataObject> { { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Monopoly") } }
            };
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("MonopolyRoom", 4, options); // Max 4 người
            isHost = true;
            Debug.Log("Tạo phòng thành công: " + currentLobby.Id);

            // Heartbeat để giữ lobby
            StartCoroutine(HeartbeatLobby(currentLobby.Id, 15f));

            // Poll để kiểm tra đủ người
            _ = PollLobby();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Lỗi tạo phòng: " + e.Message);
        }
    }

    public async void JoinLobby()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new System.Collections.Generic.List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.S1, "Monopoly", QueryFilter.OpOptions.EQ)
                }
            };
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            if (response.Results.Count > 0)
            {
                currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(response.Results[0].Id);
                Debug.Log("Tham gia phòng thành công: " + currentLobby.Id);
            }
            else
            {
                CreateLobby(); // Tạo mới nếu không tìm thấy
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Lỗi tham gia phòng: " + e.Message);
        }
    }

    private IEnumerator HeartbeatLobby(string lobbyId, float waitTime)
    {
        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return new WaitForSecondsRealtime(waitTime);
        }
    }

    private async Task PollLobby()
    {
        while (currentLobby != null)
        {
            currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
            if (currentLobby.Players.Count == 4)
            {
                Debug.Log("Đủ 4 người, bắt đầu game!");
                StartGame();
                break;
            }
            await Task.Delay(2000); // Kiểm tra mỗi 2 giây
        }
    }

    private void StartGame()
    {
        // Bắt đầu multiplayer với Netcode
        NetworkManager.Singleton.StartHost(); // Hoặc StartClient() tùy role
        // Chuyển sang scene game (ví dụ: LoadScene("GameScene"))
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI; // Để dùng Button

public class AccountAndMatchManager : MonoBehaviour
{
    public TMP_InputField usernameInput; // Input cho username
    public TextMeshProUGUI statusText; // Text hiển thị trạng thái
    public Button playButton; // Button Play (submit tên và ghép trận)
    public Button exitButton; // Button Exit

    private Lobby currentLobby;
    private bool isHost = false;

    private async void Start()
    {
        await UnityServices.InitializeAsync();
        ResetState(); // Reset trạng thái khi load scene
        playButton.onClick.AddListener(SubmitAndMatch); // Gán sự kiện cho Play
        exitButton.onClick.AddListener(ExitGame); // Gán sự kiện cho Exit
    }

    private void ResetState()
    {
        statusText.text = ""; // Clear status
        playButton.gameObject.SetActive(true); // Hiện Play
        usernameInput.interactable = true; // Enable input
        usernameInput.text = ""; // Clear input username
        currentLobby = null; // Reset lobby
        isHost = false; // Reset host flag
        StopAllCoroutines(); // Dừng heartbeat nếu có
    }

    public async void SubmitAndMatch() // Gọi từ Button Play
    {
        string username = usernameInput.text;
        if (string.IsNullOrEmpty(username))
        {
            statusText.text = "Vui lòng nhập tên!";
            return;
        }

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await AuthenticationService.Instance.UpdatePlayerNameAsync(username);
            PlayerPrefs.SetString("Username", username);
            statusText.text = "Chào " + username + "! Đang ghép trận...";

            playButton.gameObject.SetActive(false); // Ẩn Play sau submit
            usernameInput.interactable = false; // Disable input
            await JoinOrCreateLobby(); // Ghép trận ngay
        }
        catch (AuthenticationException ex)
        {
            statusText.text = "Lỗi: " + ex.Message;
            ResetState(); // Reset nếu lỗi để thử lại
        }
    }

    public void ExitGame() // Gọi từ Button Exit
    {
        Application.Quit();
    }

    private async Task JoinOrCreateLobby()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Filters = new System.Collections.Generic.List<Unity.Services.Lobbies.Models.QueryFilter>
                {
                    new Unity.Services.Lobbies.Models.QueryFilter(Unity.Services.Lobbies.Models.QueryFilter.FieldOptions.AvailableSlots, "0", Unity.Services.Lobbies.Models.QueryFilter.OpOptions.GT),
                    new Unity.Services.Lobbies.Models.QueryFilter(Unity.Services.Lobbies.Models.QueryFilter.FieldOptions.S1, "Monopoly", Unity.Services.Lobbies.Models.QueryFilter.OpOptions.EQ)
                }
            };
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            if (response.Results.Count > 0)
            {
                currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(response.Results[0].Id);
                statusText.text = "Tham gia phòng thành công! Đang chờ người...";
            }
            else
            {
                CreateLobbyOptions createOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Player = new Player { Data = new System.Collections.Generic.Dictionary<string, PlayerDataObject> { { "Username", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public, PlayerPrefs.GetString("Username")) } } },
                    Data = new System.Collections.Generic.Dictionary<string, DataObject> { { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Monopoly") } }
                };
                currentLobby = await LobbyService.Instance.CreateLobbyAsync("MonopolyRoom", 4, createOptions);
                isHost = true;
                statusText.text = "Tạo phòng mới! Đang chờ người...";
                StartCoroutine(HeartbeatLobby(currentLobby.Id, 15f));
            }

            _ = PollLobby(); // Bắt đầu poll
        }
        catch (LobbyServiceException e)
        {
            statusText.text = "Lỗi ghép trận: " + e.Message;
            ResetState(); // Reset nếu lỗi để thử lại
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
            statusText.text = $"Số người: {currentLobby.Players.Count}/4. Đang chờ...";
            if (currentLobby.Players.Count == 4)
            {
                statusText.text = "Đủ 4 người, bắt đầu game!";
                StartGame();
                break;
            }
            await Task.Delay(2000);
        }
    }

    private void StartGame()
    {
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene("GamePlay");
    }
}
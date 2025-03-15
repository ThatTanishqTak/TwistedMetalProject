using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbySystem : MonoBehaviour
{
    // UI Components
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private TMP_InputField maxPlayersInput;
    [SerializeField] private TMP_InputField hostNameInput;
    [SerializeField] private TMP_InputField clientNameInput;
    [SerializeField] private TMP_InputField lobbyCodeInput;

    [SerializeField] private GameObject hostCanvas;
    [SerializeField] private GameObject clientCanvas;
    [SerializeField] private GameObject lobbyListParent;
    [SerializeField] private GameObject lobbyItemPrefab;

    [SerializeField] private Transform lobbyContentParent;

    // State management
    private string playerName;
    private string lobbyCode;
    private Lobby hostLobby;
    private float heartbeatTimer = 15.0f; // Initialize with max value

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            AuthenticationService.Instance.SignedIn += HandlePlayerSignedIn;

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Unity Services initialized successfully");
        }
        catch (AuthenticationException e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
            throw;
        }
    }

    private void HandlePlayerSignedIn()
    {
        Debug.Log($"Player signed in with ID: {AuthenticationService.Instance.PlayerId}");
    }

    private void Awake()
    {
#if UNITY_EDITOR
        // Ensure debug statements are removed in builds
        Debug.LogWarning("Remember to remove debug statements before building!");
#endif
    }

    private async void Start()
    {
        try
        {
            await InitializeUnityServices();

            // Initialize UI state
            hostCanvas.SetActive(false);
            clientCanvas.SetActive(false);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to initialize lobby system: {e.Message}");
        }
    }

    private void Update()
    {
        HandleHeartbeat();

        // Update player name based on active canvas
        if (hostCanvas.activeSelf)
        {
            playerName = hostNameInput.text;
        }

        else if (clientCanvas.activeSelf)
        {
            playerName = clientNameInput.text;
        }
    }

    private async Task<bool> CreateLobby()
    {
        try
        {
            // Validate inputs
            if (!ValidateLobbyInputs())
            {
                return false;
            }

            CreateLobbyOptions createOptions = new()
            {
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    { "lobbyName", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: lobbyNameInput.text) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyNameInput.text, int.Parse(maxPlayersInput.text), createOptions);
            hostLobby = lobby;

            Debug.Log($"Created lobby {hostLobby.Name} with code: {hostLobby.LobbyCode}");
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return false;
        }
    }

    private async Task<bool> JoinLobbyByCode()
    {
        try
        {
            if (string.IsNullOrEmpty(lobbyCodeInput.text))
            {
                Debug.LogError("Lobby code cannot be empty");
                return false;
            }

            var joinOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCodeInput.text, joinOptions);

            Debug.Log($"Successfully joined lobby {joinedLobby.Name}");
            return true;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            return false;
        }
    }

    public async void ShowLobbies()
    {
        lobbyCodeInput = lobbyItemPrefab.transform.GetChild(3).GetComponent<TMP_InputField>();

        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            // Clear existing lobby items
            foreach (Transform t in lobbyContentParent)
            {
                DestroyImmediate(t.gameObject);
            }

            // Create new lobby items
            foreach (Lobby lobby in queryResponse.Results)
            {
                Transform newLobbyItem = Instantiate(lobbyItemPrefab, lobbyContentParent).transform;

                // Update lobby item UI
                newLobbyItem.GetChild(0).GetComponent<TextMeshProUGUI>().text = lobby.Name;
                newLobbyItem.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";

                // Position the lobby item
                Vector3 currentPosition = newLobbyItem.position;
                currentPosition.y += 10.0f;
                newLobbyItem.position = currentPosition;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to show lobbies: {e.Message}");
        }
    }

    private void HandleHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;

            if (heartbeatTimer <= 0f)
            {
                SendHeartbeatPing();
                heartbeatTimer = 15f; // Reset timer
            }
        }
    }

    private async void SendHeartbeatPing()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            Debug.Log("Sent heartbeat ping successfully");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to send heartbeat: {e.Message}");
        }
    }

    private bool ValidateLobbyInputs()
    {
        if (int.Parse(maxPlayersInput.text) % 2 != 0 ||
            int.Parse(maxPlayersInput.text) < 2)
        {
            Debug.LogError("Number of players must be even (minimum 4)");
            return false;
        }

        if (string.IsNullOrEmpty(lobbyNameInput.text))
        {
            Debug.LogError("Lobby name cannot be empty");
            return false;
        }

        return true;
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member, value: playerName) }
            }
        };
    }

    public async void OnCreateLobbyButtonClicked()
    {
        await CreateLobby();
    }

    public async void OnJoinLobbyButtonClicked()
    {
        await JoinLobbyByCode();
    }

    private void OnDestroy()
    {
        // Cleanup event listeners
        lobbyNameInput.onSubmit.RemoveAllListeners();
        maxPlayersInput.onSubmit.RemoveAllListeners();

        // Leave lobby if still connected
        if (hostLobby != null)
        {
            LeaveLobby();
        }
    }

    private async void LeaveLobby()
    {
        try
        {
            if (hostLobby != null)
            {
                await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id);
                hostLobby = null;

                Debug.Log("Successfully left lobby");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to leave lobby: {e.Message}");
        }
    }
}
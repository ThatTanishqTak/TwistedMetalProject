using UnityEngine;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATEMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class MultiplayerManager : NetworkBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    public event EventHandler OnPlayerDataNetworkListChanged;
    public event EventHandler OnTeamsFormed;

    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList;
    private NetworkList<TeamRoleData> teamRoleAssignments;

    private void Awake()
    {
        Instance = this;

        DontDestroyOnLoad(gameObject);

        playerDataNetworkList = new NetworkList<PlayerData>();
        teamRoleAssignments = new NetworkList<TeamRoleData>();

        playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
        teamRoleAssignments.OnListChanged += TeamRoleAssignments_OnListChanged;
    }

    private void TeamRoleAssignments_OnListChanged(NetworkListEvent<TeamRoleData> changeEvent) { }
    private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
        CheckForMaxPlayers();
    }

    private void CheckForMaxPlayers()
    {
        if (playerDataNetworkList.Count == GameLobby.Instance.GetLobby().MaxPlayers)
        {
            FormTeamsServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void FormTeamsServerRpc()
    {
        int totalTeam = GameLobby.Instance.GetLobby().MaxPlayers / 2;

        teamRoleAssignments.Clear();

        TeamType team;
        RoleType role;

        int currentPlayerIndex = 0; 

        for (int teamNumber = 0; teamNumber < totalTeam; teamNumber++)
        {
            if (teamNumber == 0)
            {
                team = TeamType.TeamA;
            }
            else if (teamNumber == 1)
            {
                team = TeamType.TeamB;
            }
            else if (teamNumber == 2)
            {
                team = TeamType.TeamC;
            }
            else if (teamNumber == 3)
            {
                team = TeamType.TeamD;
            }
            else
            {
                team = TeamType.TeamE;
            }

            for (int roleIndex = 0; roleIndex <= 1; roleIndex++)
            {
                role = (roleIndex == 0) ? RoleType.Driver : RoleType.Shooter;

                TeamRoleData newData = new TeamRoleData
                {
                    team = team,
                    role = role,
                    teamNumber = teamNumber,
                    clientId = playerDataNetworkList[currentPlayerIndex].clientID // ✅ assign correctly
                };

                teamRoleAssignments.Add(newData);

                Debug.Log("[TeamAssignment] Player " + playerDataNetworkList[currentPlayerIndex].playerName + " assigned as " + newData.team + " " + newData.role + " ClientID: " + newData.clientId);

                currentPlayerIndex++;
            }
        }

        TeamsFormedClientRpc();
    }

    [ClientRpc]
    private void TeamsFormedClientRpc(ClientRpcParams clientRpcParams = default)
    {
        OnTeamsFormed?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;

        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData { clientID = clientId });
    }

    private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelect.ToString())
        {
            response.Approved = false;
            response.Reason = "Game Has Already Started";
            return;
        }

        response.Approved = true;
    }

    public bool IsPlayerIndexConnected(int index)
    {
        return index < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
    {
        return playerDataNetworkList[playerIndex];
    }

    public List<TeamRoleData> GetAllTeamAssignments()
    {
        List<TeamRoleData> copy = new List<TeamRoleData>();
        foreach (var assignment in teamRoleAssignments)
        {
            copy.Add(assignment);
        }
        return copy;
    }
}

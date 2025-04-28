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

    int numberOfTeams = 0;

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
            numberOfTeams = GameLobby.Instance.GetLobby().MaxPlayers / 2;

            FormTeamsServerRpc();
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void FormTeamsServerRpc()
    //{
    //    int totalTeam = GameLobby.Instance.GetLobby().MaxPlayers / 2;

    //    teamRoleAssignments.Clear();

    //    TeamType team;
    //    RoleType role;

    //    int currentPlayerIndex = 0; 

    //    for (int teamNumber = 0; teamNumber < totalTeam; teamNumber++)
    //    {
    //        if (teamNumber == 0)
    //        {
    //            team = TeamType.TeamA;
    //        }
    //        else if (teamNumber == 1)
    //        {
    //            team = TeamType.TeamB;
    //        }
    //        else if (teamNumber == 2)
    //        {
    //            team = TeamType.TeamC;
    //        }
    //        else
    //        {
    //            team = TeamType.TeamD;
    //        }

    //        for (int roleIndex = 0; roleIndex <= 1; roleIndex++)
    //        {
    //            role = (roleIndex == 0) ? RoleType.Driver : RoleType.Shooter;

    //            TeamRoleData newData = new TeamRoleData
    //            {
    //                team = team,
    //                role = role,
    //                teamNumber = teamNumber,
    //                clientId = playerDataNetworkList[currentPlayerIndex].clientID // ✅ assign correctly
    //            };

    //            teamRoleAssignments.Add(newData);

    //            Debug.Log("[TeamAssignment] Player " + playerDataNetworkList[currentPlayerIndex].playerName + " assigned as " + newData.team + " " + newData.role + " ClientID: " + newData.clientId);

    //            currentPlayerIndex++;
    //        }
    //    }

    //    TeamsFormedClientRpc();
    //}

    private bool Exists(Func<TeamRoleData, bool> predicate)
    {
        foreach (var item in teamRoleAssignments)
        {
            if (predicate(item))
                return true;
        }
        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void FormTeamsServerRpc()
    {
        teamRoleAssignments.Clear();

        // Correctly copy data, avoid invalid cast
        List<PlayerData> unassignedPlayers = new List<PlayerData>();
        foreach (var playerData in playerDataNetworkList)
        {
            unassignedPlayers.Add(playerData);
        }

        // Shuffle players randomly
        for (int i = 0; i < unassignedPlayers.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, unassignedPlayers.Count);
            (unassignedPlayers[i], unassignedPlayers[randomIndex]) = (unassignedPlayers[randomIndex], unassignedPlayers[i]);
        }

        int numberOfTeams = unassignedPlayers.Count / 2;
        int currentPlayerIndex = 0;

        for (int teamNumber = 0; teamNumber < numberOfTeams; teamNumber++) // 1st loop: assign teams
        {
            TeamType team = teamNumber switch
            {
                0 => TeamType.TeamA,
                1 => TeamType.TeamB,
                2 => TeamType.TeamC,
                3 => TeamType.TeamD,
                _ => TeamType.TeamA
            };

            for (int roleIndex = 0; roleIndex < 2; roleIndex++) // 2nd nested loop: assign roles inside the team
            {
                if (currentPlayerIndex >= unassignedPlayers.Count)
                {
                    Debug.LogError("Ran out of players while forming teams!");
                    return;
                }

                var player = unassignedPlayers[currentPlayerIndex];
                currentPlayerIndex++;

                // Randomly assign role
                RoleType assignedRole = UnityEngine.Random.value > 0.5f ? RoleType.Driver : RoleType.Shooter;

                // Ensure no duplicate roles inside a team
                if (Exists(trd => trd.teamNumber == teamNumber && trd.role == assignedRole))
                {
                    assignedRole = assignedRole == RoleType.Driver ? RoleType.Shooter : RoleType.Driver;
                }

                TeamRoleData assignment = new TeamRoleData
                {
                    clientId = player.clientID,
                    team = team,
                    teamNumber = teamNumber,
                    role = assignedRole
                };

                teamRoleAssignments.Add(assignment);

                Debug.Log($"Assigned {player.playerName} to {team} as {assignedRole} (ClientID: {player.clientID})");
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

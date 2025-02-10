using Unity.Netcode;
using UnityEngine;
using static GameManager;

public class GameManager : NetworkBehaviour
{
    public enum PlayerRoles { Driver, Shooter }
    public enum Teams { Team_1, Team_2 }

    public NetworkVariable<int> currentRound = new(1);

    [SerializeField] private bool isTeamCreated = false;

    private void Update()
    {
        if (NetworkManager.ConnectedClients.Count == 4 && !isTeamCreated) { CreateTeams(); }
        else { return; }
    }

    private void CreateTeams()
    {
        Debug.Log("Creating Teams!");
        int index = 0;

        foreach(var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            PlayerController player = client.PlayerObject.GetComponent<PlayerController>();
            Debug.Log(index);

            if (index < 2) { player.teams.Value = Teams.Team_1; }
            else { player.teams.Value = Teams.Team_2; }

            if (currentRound.Value == 1) { player.roles.Value = (index % 2 == 0) ? PlayerRoles.Driver : PlayerRoles.Shooter; }
            else { player.roles.Value = (index % 2 == 0) ? PlayerRoles.Shooter : PlayerRoles.Driver; }

            index++;
        }

        isTeamCreated = true;
    }

    [ServerRpc]
    public void StartNextRoundServerRpc()
    {
        currentRound.Value++;
        CreateTeams();
    }
}
using Unity.Netcode;
using UnityEngine;
using static GameManager;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class GameManager : NetworkBehaviour
{
    [SerializeField] public enum PlayerRoles { Driver, Shooter }
    [SerializeField] public enum Teams { Team_1, Team_2 }
    //[SerializeField] public enum GameModes { GameMode, GameMode2 }

    [SerializeField] private NetworkVariable<int> maxRound = new(4);
    [SerializeField] private NetworkVariable<int> currentRound = new(1);

    [SerializeField] private bool isTeamCreated = false;

    private void Update()
    {
        if (NetworkManager.ConnectedClients.Count == 4 && !isTeamCreated) { CreateTeams(); }
        else { return; }
    }

    private void CreateTeams()
    {
        int index = 0;

        foreach(var client in NetworkManager.Singleton.ConnectedClients.Values)
        {
            PlayerController player = client.PlayerObject.GetComponent<PlayerController>();
            Debug.Log(index);

            player.teams.Value = index < 2 ? Teams.Team_1 : Teams.Team_2;
            player.roles.Value = currentRound.Value == 1 ? (index % 2 == 0 ? PlayerRoles.Driver : PlayerRoles.Shooter)
                                                         : (index % 2 == 0 ? PlayerRoles.Shooter : PlayerRoles.Driver);

            index++;
        }

        isTeamCreated = true;
    }

    [ServerRpc]
    public void StartNextRoundServerRpc()
    {
        if (currentRound.Value <= maxRound.Value)
        {
            currentRound.Value++;
            CreateTeams();
        }

        else
        {
            // End Game State
        }
    }
}
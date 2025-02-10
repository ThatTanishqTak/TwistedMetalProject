using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<GameManager.PlayerRoles> roles = new();
    public NetworkVariable<GameManager.Teams> teams = new();

    private void Update()
    {
        if (!IsOwner) { return; }

        if (roles.Value == GameManager.PlayerRoles.Driver)
        {
            PlayerDrive();
        }

        else
        {
            PlayerShoot();
        }
    }

    private void PlayerDrive()
    {
        Debug.Log($"Client ID: {NetworkManager.Singleton.LocalClientId}, Your team: {teams.Value}, your role: {roles.Value}");
    }

    private void PlayerShoot()
    {
        Debug.Log($"Client ID: {NetworkManager.Singleton.LocalClientId}, Your team: {teams.Value}, your role: {roles.Value}");
    }
}
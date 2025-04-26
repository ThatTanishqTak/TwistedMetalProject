using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class CarSpawnerManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject team1CarPrefab;
    [SerializeField] private GameObject team2CarPrefab;

    [Header("Spawn Positions")]
    [SerializeField] private Transform team1SpawnPoint;
    [SerializeField] private Transform team2SpawnPoint;

    private GameObject team1CarInstance;
    private GameObject team2CarInstance;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnCars();
        }
    }

    private void SpawnCars()
    {
        // Spawn Team 1 Car
        team1CarInstance = Instantiate(team1CarPrefab, team1SpawnPoint.position, team1SpawnPoint.rotation);
        team1CarInstance.GetComponent<NetworkObject>().Spawn();

        // Spawn Team 2 Car
        team2CarInstance = Instantiate(team2CarPrefab, team2SpawnPoint.position, team2SpawnPoint.rotation);
        team2CarInstance.GetComponent<NetworkObject>().Spawn();

        // Assign driver/shooter roles
        AssignRoles();
    }

    private void AssignRoles()
    {
        // Get team-role info from MultiplayerManager
        var teamAssignments = MultiplayerManager.Instance.GetAllTeamAssignments();

        foreach (var assignment in teamAssignments)
        {
            if (assignment.teamNumber == 1)
            {
                AssignPlayerToCar(team1CarInstance, assignment);
            }
            else if (assignment.teamNumber == 2)
            {
                AssignPlayerToCar(team2CarInstance, assignment);
            }
        }
    }

    private void AssignPlayerToCar(GameObject carInstance, TeamRoleData roleData)
    {
        var wrapper = carInstance.GetComponent<CarControllerWrapper>();
        var turret = carInstance.GetComponentInChildren<TurretController>();
        var shooter = carInstance.GetComponentInChildren<Shooter>();

        if (roleData.role == RoleType.Driver)
        {
            wrapper.AssignDriver(roleData.clientId);
        }
        else if (roleData.role == RoleType.Shooter)
        {
            turret.AssignShooter(true);
            shooter.SetShooterAuthority(true);
            shooter.SetShooterClientId(roleData.clientId);
        }
    }

}

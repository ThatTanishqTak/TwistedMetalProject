using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class CarSpawnerManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject team1CarPrefab;
    [SerializeField] private GameObject team2CarPrefab;

    private GameObject team1CarInstance;
    private GameObject team2CarInstance;

    private float spawnDistanceFromCenter = 15f; // tweak based on the size of the map

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnCars();
        }
    }

    private void SpawnCars()
    {
        // Get team role info
        var teamAssignments = MultiplayerManager.Instance.GetAllTeamAssignments();

        foreach (var assignment in teamAssignments)
        {
            if (assignment.teamNumber == 1)
            {
                team1CarInstance = SpawnCar(team1CarPrefab, GetSpawnPositionForTeam(1));
                AssignPlayerToCar(team1CarInstance, assignment);
            }
            else if (assignment.teamNumber == 2)
            {
                team2CarInstance = SpawnCar(team2CarPrefab, GetSpawnPositionForTeam(2));
                AssignPlayerToCar(team2CarInstance, assignment);
            }
        }
    }

    private GameObject SpawnCar(GameObject carPrefab, Vector3 spawnPosition)
    {
        GameObject carInstance = Instantiate(carPrefab, spawnPosition, Quaternion.identity);
        carInstance.GetComponent<NetworkObject>().Spawn();
        return carInstance;
    }

    private Vector3 GetSpawnPositionForTeam(int teamNumber)
    {
        Vector3 center = Vector3.zero;
        float offset = spawnDistanceFromCenter;

        if (teamNumber == 1)
            return center + new Vector3(-offset, 0, 0);
        else if (teamNumber == 2)
            return center + new Vector3(offset, 0, 0);
        else
            return center;
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

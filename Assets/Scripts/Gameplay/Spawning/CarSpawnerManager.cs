using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class CarSpawnerManager : NetworkBehaviour
{
    [Header("Car Prefabs")]
    [SerializeField] private GameObject team1CarPrefab;
    [SerializeField] private GameObject team2CarPrefab;

    private GameObject team1CarInstance;
    private GameObject team2CarInstance;

    private float spawnDistanceFromCenter = 15f;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnCars();
        }
    }

    private void SpawnCars()
    {
        // Spawn one car per team
        team1CarInstance = SpawnCar(team1CarPrefab, GetSpawnPositionForTeam(1));
        team2CarInstance = SpawnCar(team2CarPrefab, GetSpawnPositionForTeam(2));

        AssignRolesToPlayers();
    }

    private GameObject SpawnCar(GameObject prefab, Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        var networkObject = instance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.Spawn();
        }
        else
        {
            Debug.LogError("No NetworkObject found on car prefab!");
        }

        return instance;
    }

    private Vector3 GetSpawnPositionForTeam(int team)
    {
        Vector3 center = Vector3.zero;
        float offset = spawnDistanceFromCenter;

        if (team == 1)
            return center + new Vector3(-offset, 0, 0);
        else
            return center + new Vector3(offset, 0, 0);
    }

    private void AssignRolesToPlayers()
    {
        var teamAssignments = MultiplayerManager.Instance.GetAllTeamAssignments();

        foreach (var assignment in teamAssignments)
        {
            GameObject car = null;

            if (assignment.team == TeamType.TeamA)
                car = team1CarInstance;
            else if (assignment.team == TeamType.TeamB)
                car = team2CarInstance;

            if (car == null)
            {
                Debug.LogError($"Car not found for team {assignment.team}");
                continue;
            }

            var wrapper = car.GetComponent<CarControllerWrapper>();
            var turret = car.GetComponentInChildren<TurretController>();
            var shooter = car.GetComponentInChildren<Shooter>();

            if (assignment.role == RoleType.Driver)
            {
                wrapper.AssignDriver(assignment.clientId);
                Debug.Log($"[CarSpawner] Assigned DRIVER to Client {assignment.clientId}");
            }
            else if (assignment.role == RoleType.Shooter)
            {
                shooter.SetShooterAuthority(true);
                shooter.SetShooterClientId(assignment.clientId);
                Debug.Log($"[CarSpawner] Assigned SHOOTER to Client {assignment.clientId}");
            }
        }
    }
}

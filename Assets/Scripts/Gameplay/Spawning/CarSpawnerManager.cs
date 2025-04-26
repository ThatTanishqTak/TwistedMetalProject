using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class CarSpawnerManager : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject team1CarPrefab;
    [SerializeField] private GameObject team2CarPrefab;

    private float spawnDistanceFromCenter = 15f; // tweak based on map size

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnCars();
        }
    }

    private void SpawnCars()
    {
        var teamAssignments = MultiplayerManager.Instance.GetAllTeamAssignments();

        foreach (var assignment in teamAssignments)
        {
            Vector3 spawnPosition = GetSpawnPositionForTeam(assignment.teamNumber);

            GameObject prefab = assignment.teamNumber == 1 ? team1CarPrefab : team2CarPrefab;
            GameObject carInstance = SpawnCar(prefab, spawnPosition, assignment.clientId);

            AssignPlayerToCar(carInstance, assignment);
        }
    }

    private GameObject SpawnCar(GameObject carPrefab, Vector3 spawnPosition, ulong ownerClientId)
    {
        GameObject carInstance = Instantiate(carPrefab, spawnPosition, Quaternion.identity);

        if (carInstance.TryGetComponent(out NetworkObject netObj))
        {
            netObj.SpawnWithOwnership(ownerClientId);
        }
        else
        {
            Debug.LogError($"[CarSpawnerManager] Car prefab {carPrefab.name} missing NetworkObject!");
        }

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
            wrapper?.AssignDriver(roleData.clientId);
        }
        else if (roleData.role == RoleType.Shooter)
        {
            shooter?.SetShooterAuthority(true);
            shooter?.SetShooterClientId(roleData.clientId);
        }
    }
}

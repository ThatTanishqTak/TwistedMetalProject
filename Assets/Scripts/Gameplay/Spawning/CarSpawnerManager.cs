using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CarSpawnerManager : NetworkBehaviour
{
    [Header("Car Prefabs")]
    [SerializeField] private GameObject team1CarPrefab;
    [SerializeField] private GameObject team2CarPrefab;

    [Header("Spawn Settings")]
    [SerializeField, Tooltip("Distance from world-center to spawn each team’s car")]
    private float spawnDistanceFromCenter = 15f;

    private GameObject team1CarInstance;
    private GameObject team2CarInstance;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // 1) First spawn in the Lobby (or CharacterSelect) scene:
        SpawnCars();

        // 2) Then subscribe to the networked scene-load event:
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
    }

    private void OnLoadEventCompleted(string sceneName,
                                     LoadSceneMode loadMode,
                                     List<ulong> clientsCompleted,
                                     List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;

        // When we finish loading the Arena, clean up & respawn:
        if (sceneName == Loader.Scene.Arena.ToString())
        {
            if (team1CarInstance != null) Destroy(team1CarInstance);
            if (team2CarInstance != null) Destroy(team2CarInstance);
            SpawnCars();
        }
    }

    private void SpawnCars()
    {
        team1CarInstance = SpawnCar(team1CarPrefab, GetSpawnPositionForTeam(1));
        team2CarInstance = SpawnCar(team2CarPrefab, GetSpawnPositionForTeam(2));
        AssignRolesToPlayers();
    }

    private GameObject SpawnCar(GameObject prefab, Vector3 position)
    {
        var instance = Instantiate(prefab, position, Quaternion.identity);
        var netObj = instance.GetComponent<NetworkObject>();
        if (netObj != null) netObj.Spawn();
        else Debug.LogError("Car prefab is missing a NetworkObject!");
        return instance;
    }

    private Vector3 GetSpawnPositionForTeam(int team)
    {
        float offset = spawnDistanceFromCenter;
        return Vector3.zero + (team == 1
            ? new Vector3(-offset, 0f, 0f)
            : new Vector3(offset, 0f, 0f));
    }

    private void AssignRolesToPlayers()
    {
        var assignments = MultiplayerManager.Instance.GetAllTeamAssignments();
        foreach (var assignment in assignments)
        {
            GameObject car = assignment.team == TeamType.TeamA
                ? team1CarInstance
                : team2CarInstance;

            if (car == null)
            {
                Debug.LogError($"Car not found for team {assignment.team}");
                continue;
            }

            var wrapper = car.GetComponent<CarControllerWrapper>();
            var shooter = car.GetComponentInChildren<Shooter>();

            if (assignment.role == RoleType.Driver)
            {
                wrapper.AssignDriver(assignment.clientId);
            }
            else // Shooter
            {
                shooter.SetShooterAuthority(true);
                shooter.SetShooterClientId(assignment.clientId);
            }
        }
    }

    private new void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
    }
}

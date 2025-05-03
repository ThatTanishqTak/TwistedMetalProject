using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CarSpawnerManager : NetworkBehaviour
{
    [Header("Car Prefabs")]
    [SerializeField] private GameObject team1CarPrefab;
    [SerializeField] private GameObject team2CarPrefab;

    [Header("Layers")]
    [Tooltip("Only your ground colliders should be on this layer")]
    [SerializeField] private LayerMask groundLayerMask;
    [Tooltip("Any obstacles you want to avoid at spawn")]
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Sampling Settings")]
    [Tooltip("How many random attempts per team before fallback")]
    [SerializeField] private int maxSpawnAttempts = 10;
    [Tooltip("Radius to check around a candidate for nearby obstacles")]
    [SerializeField] private float obstacleCheckRadius = 1f;
    [Tooltip("Extra height above the map to start each raycast")]
    [SerializeField] private float rayStartHeight = 50f;

    private GameObject team1CarInstance;
    private GameObject team2CarInstance;
    private Bounds groundBounds;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        ComputeGroundBounds();
        SpawnCars();

        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadCompleted;
    }

    private void OnLoadCompleted(string sceneName,
                                 LoadSceneMode loadMode,
                                 List<ulong> clientsCompleted,
                                 List<ulong> clientsTimedOut)
    {
        if (!IsServer) return;

        
        if (team1CarInstance != null) Destroy(team1CarInstance);
        if (team2CarInstance != null) Destroy(team2CarInstance);

        ComputeGroundBounds();
        SpawnCars();
    }

    private void ComputeGroundBounds()
    {
        var groundCols = FindObjectsOfType<Collider>()
            .Where(c => ((1 << c.gameObject.layer) & groundLayerMask) != 0)
            .ToArray();

        if (groundCols.Length == 0)
        {
            Debug.LogError("CarSpawnerManager: No ground Colliders found on your ground layer!");
            groundBounds = new Bounds(Vector3.zero, Vector3.one * 100f);
            return;
        }

        groundBounds = groundCols[0].bounds;
        for (int i = 1; i < groundCols.Length; i++)
            groundBounds.Encapsulate(groundCols[i].bounds);
    }

    private void SpawnCars()
    {
        Vector3 posA = SampleSpawnPoint(TeamType.TeamA);
        Vector3 posB = SampleSpawnPoint(TeamType.TeamB);

        team1CarInstance = SpawnCar(team1CarPrefab, posA);
        team2CarInstance = SpawnCar(team2CarPrefab, posB);

        AssignRolesToPlayers();
    }

    private Vector3 SampleSpawnPoint(TeamType team)
    {
        float minX = groundBounds.min.x;
        float maxX = groundBounds.max.x;
        float midX = groundBounds.center.x;
        float minZ = groundBounds.min.z;
        float maxZ = groundBounds.max.z;

        float regionMinX = team == TeamType.TeamA ? minX : midX;
        float regionMaxX = team == TeamType.TeamA ? midX : maxX;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float x = Random.Range(regionMinX, regionMaxX);
            float z = Random.Range(minZ, maxZ);
            Vector3 origin = new Vector3(x, groundBounds.max.y + rayStartHeight, z);

            if (Physics.Raycast(origin, Vector3.down,
                                out RaycastHit hit,
                                groundBounds.size.y + 2f * rayStartHeight,
                                groundLayerMask))
            {
                Vector3 candidate = hit.point;

                if (!Physics.CheckSphere(candidate + Vector3.up * 0.5f,
                                         obstacleCheckRadius,
                                         obstacleLayerMask))
                {
                    return candidate;
                }
            }
        }

        // Fallback: center of the region
        float centerX = (regionMinX + regionMaxX) * 0.5f;
        float centerZ = (minZ + maxZ) * 0.5f;
        Vector3 fallbackOrigin = new Vector3(centerX,
                                             groundBounds.max.y + rayStartHeight,
                                             centerZ);

        if (Physics.Raycast(fallbackOrigin, Vector3.down,
                            out RaycastHit fallbackHit,
                            groundBounds.size.y + 2f * rayStartHeight,
                            groundLayerMask))
        {
            return fallbackHit.point;
        }

        return new Vector3(centerX, 0f, centerZ);
    }

    private GameObject SpawnCar(GameObject prefab, Vector3 position)
    {
        var go = Instantiate(prefab, position, Quaternion.identity);
        if (go.TryGetComponent<NetworkObject>(out var net))
            net.Spawn();
        else
            Debug.LogError("Car prefab is missing a NetworkObject!");
        return go;
    }

    private void AssignRolesToPlayers()
    {
        var assignments = MultiplayerManager.Instance.GetAllTeamAssignments();
        foreach (var a in assignments)
        {
            GameObject car = a.team == TeamType.TeamA
                ? team1CarInstance
                : team2CarInstance;

            if (car == null) continue;

            var driver = car.GetComponent<CarControllerWrapper>();
            var shooter = car.GetComponentInChildren<Shooter>();

            if (a.role == RoleType.Driver)
                driver.AssignDriver(a.clientId);
            else
            {
                shooter.SetShooterAuthority(true);
                shooter.SetShooterClientId(a.clientId);
            }
        }
    }

    private new void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadCompleted;
    }
}

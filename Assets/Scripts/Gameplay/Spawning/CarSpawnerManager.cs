using System.Linq;
using UnityEngine;
using Unity.Netcode;

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
    }

    private void ComputeGroundBounds()
    {
        // modern API: returns all Colliders (active & inactive)
        var groundCols = Object
            .FindObjectsByType<Collider>(FindObjectsSortMode.None)
            .Where(c => ((1 << c.gameObject.layer) & groundLayerMask) != 0)
            .ToArray();

        if (groundCols.Length == 0)
        {
            Debug.LogError($"[{gameObject.name} | Scene:{gameObject.scene.name}] " +
                           "No ground Colliders found on your ground layer!");
            groundBounds = new Bounds(Vector3.zero, Vector3.one * 100f);
            return;
        }

        groundBounds = groundCols[0].bounds;
        for (int i = 1; i < groundCols.Length; i++)
            groundBounds.Encapsulate(groundCols[i].bounds);
    }

    private void SpawnCars()
    {
        Debug.Log($"[CarSpawner:{gameObject.name} | Scene:{gameObject.scene.name}] SpawnCars() called");

        Vector3 posA = SampleSpawnPoint(TeamType.TeamA);
        Vector3 posB = SampleSpawnPoint(TeamType.TeamB);

        team1CarInstance = SpawnCar(team1CarPrefab, posA);
        team2CarInstance = SpawnCar(team2CarPrefab, posB);

        AssignRolesToPlayers();
    }

    private GameObject SpawnCar(GameObject prefab, Vector3 position)
    {
        Debug.Log($"[CarSpawner:{gameObject.name} | Scene:{gameObject.scene.name}] " +
                  $"Instantiating {prefab.name} at {position}");

        var go = Instantiate(prefab, position, Quaternion.identity);
        if (go.TryGetComponent<NetworkObject>(out var net))
            net.Spawn();
        else
            Debug.LogError($"[{gameObject.name}] Car prefab '{prefab.name}' missing NetworkObject!");
        return go;
    }

    private Vector3 SampleSpawnPoint(TeamType team)
    {
        float minX = groundBounds.min.x, maxX = groundBounds.max.x, midX = groundBounds.center.x;
        float minZ = groundBounds.min.z, maxZ = groundBounds.max.z;

        float regionMinX = (team == TeamType.TeamA) ? minX : midX;
        float regionMaxX = (team == TeamType.TeamA) ? midX : maxX;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float x = Random.Range(regionMinX, regionMaxX);
            float z = Random.Range(minZ, maxZ);
            var origin = new Vector3(x, groundBounds.max.y + rayStartHeight, z);

            if (Physics.Raycast(origin, Vector3.down,
                                out var hit,
                                groundBounds.size.y + 2f * rayStartHeight,
                                groundLayerMask))
            {
                var candidate = hit.point;
                if (!Physics.CheckSphere(candidate + Vector3.up * 0.5f,
                                         obstacleCheckRadius,
                                         obstacleLayerMask))
                    return candidate;
            }
        }

        // fallback: center of region, dropping to groundBounds.min.y
        float cx = (regionMinX + regionMaxX) * 0.5f;
        float cz = (minZ + maxZ) * 0.5f;
        var top = new Vector3(cx, groundBounds.max.y + rayStartHeight, cz);

        if (Physics.Raycast(top, Vector3.down,
                            out var fhit,
                            groundBounds.size.y + 2f * rayStartHeight,
                            groundLayerMask))
        {
            return fhit.point;
        }

        return new Vector3(cx, groundBounds.min.y, cz);
    }

    private void AssignRolesToPlayers()
    {
        foreach (var a in MultiplayerManager.Instance.GetAllTeamAssignments())
        {
            var car = (a.team == TeamType.TeamA)
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
        if (!IsServer) return;

        if (team1CarInstance != null
            && team1CarInstance.TryGetComponent<NetworkObject>(out var net1)
            && net1.IsSpawned)
        {
            net1.Despawn(true);
            Debug.Log($"[CarSpawner:{gameObject.name} | Scene:{gameObject.scene.name}] Despawning team1CarInstance");
        }

        if (team2CarInstance != null
            && team2CarInstance.TryGetComponent<NetworkObject>(out var net2)
            && net2.IsSpawned)
        {
            net2.Despawn(true);
            Debug.Log($"[CarSpawner:{gameObject.name} | Scene:{gameObject.scene.name}] Despawning team2CarInstance");
        }
    }
}

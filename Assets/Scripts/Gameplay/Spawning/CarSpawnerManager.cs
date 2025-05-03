using System.Collections.Generic;
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
        var groundCols = FindObjectsOfType<Collider>()
            .Where(c => ((1 << c.gameObject.layer) & groundLayerMask) != 0)
            .ToArray();

        if (groundCols.Length == 0)
        {
            Debug.LogError("CarSpawnerManager: No ground Colliders found on your ground layer!");
            // fallback to a flat ground at y = 0
            groundBounds = new Bounds(new Vector3(0, 0, 0), Vector3.one * 100f);
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
        // region bounds
        float minX = groundBounds.min.x;
        float maxX = groundBounds.max.x;
        float midX = groundBounds.center.x;
        float minZ = groundBounds.min.z;
        float maxZ = groundBounds.max.z;

        float regionMinX = team == TeamType.TeamA ? minX : midX;
        float regionMaxX = team == TeamType.TeamA ? midX : maxX;

        // try random points
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            float x = Random.Range(regionMinX, regionMaxX);
            float z = Random.Range(minZ, maxZ);
            Vector3 origin = new Vector3(x, groundBounds.max.y + rayStartHeight, z);

            if (Physics.Raycast(origin, Vector3.down,
                                out var hit,
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

        // Fallback: drop straight down at region-center but land at your ground's MINIMUM Y,
        // not at y=0
        float cx = (regionMinX + regionMaxX) * 0.5f;
        float cz = (minZ + maxZ) * 0.5f;
        Vector3 fallbackOrigin = new Vector3(cx, groundBounds.max.y + rayStartHeight, cz);

        if (Physics.Raycast(fallbackOrigin, Vector3.down,
                            out var fallbackHit,
                            groundBounds.size.y + 2f * rayStartHeight,
                            groundLayerMask))
        {
            return fallbackHit.point;
        }

        // LAST RESORT: use groundBounds.min.y instead of 0
        return new Vector3(cx, groundBounds.min.y, cz);
    }

    private GameObject SpawnCar(GameObject prefab, Vector3 position)
    {
        var go = Instantiate(prefab, position, Quaternion.identity);
        if (go.TryGetComponent<NetworkObject>(out var net))
            net.Spawn();
        else
            Debug.LogError("Car prefab missing NetworkObject!");
        return go;
    }

    private void AssignRolesToPlayers()
    {
        var assignments = MultiplayerManager.Instance.GetAllTeamAssignments();
        foreach (var a in assignments)
        {
            var car = (a.team == TeamType.TeamA) ? team1CarInstance : team2CarInstance;
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
}

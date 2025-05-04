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
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private float obstacleCheckRadius = 1f;
    [SerializeField] private float rayStartHeight = 50f;

    private GameObject team1CarInstance, team2CarInstance;
    private Bounds groundBounds;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        ComputeGroundBounds();
        SpawnCars();
    }

    private void ComputeGroundBounds()
    {
        var cols = FindObjectsOfType<Collider>()
            .Where(c => (groundLayerMask & (1 << c.gameObject.layer)) != 0)
            .ToArray();

        if (cols.Length == 0)
        {
            Debug.LogError("No ground colliders on Ground layer!");
            groundBounds = new Bounds(Vector3.zero, Vector3.one * 100f);
            return;
        }

        groundBounds = cols[0].bounds;
        foreach (var c in cols.Skip(1))
            groundBounds.Encapsulate(c.bounds);
    }

    private void SpawnCars()
    {
        team1CarInstance = SpawnCar(team1CarPrefab, SamplePoint(TeamType.TeamA));
        team2CarInstance = SpawnCar(team2CarPrefab, SamplePoint(TeamType.TeamB));
        AssignRoles();
    }

    private Vector3 SamplePoint(TeamType team)
    {
        float minX = groundBounds.min.x, maxX = groundBounds.max.x, midX = groundBounds.center.x;
        float minZ = groundBounds.min.z, maxZ = groundBounds.max.z;
        float regionMinX = team == TeamType.TeamA ? minX : midX;
        float regionMaxX = team == TeamType.TeamA ? midX : maxX;

        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            var x = Random.Range(regionMinX, regionMaxX);
            var z = Random.Range(minZ, maxZ);
            var top = new Vector3(x, groundBounds.max.y + rayStartHeight, z);
            if (Physics.Raycast(top, Vector3.down, out var hit,
                                groundBounds.size.y + 2 * rayStartHeight,
                                groundLayerMask))
            {
                var p = hit.point;
                if (!Physics.CheckSphere(p + Vector3.up * 0.5f,
                                         obstacleCheckRadius,
                                         obstacleLayerMask))
                    return p;
            }
        }

        // fallback to center
        float cx = (regionMinX + regionMaxX) / 2, cz = (minZ + maxZ) / 2;
        var ftop = new Vector3(cx, groundBounds.max.y + rayStartHeight, cz);
        if (Physics.Raycast(ftop, Vector3.down, out var fhit,
                            groundBounds.size.y + 2 * rayStartHeight,
                            groundLayerMask))
            return fhit.point;

        return new Vector3(cx, groundBounds.min.y, cz);
    }

    private GameObject SpawnCar(GameObject prefab, Vector3 pos)
    {
        var go = Instantiate(prefab, pos, Quaternion.identity);
        if (go.TryGetComponent<NetworkObject>(out var net)) net.Spawn();
        else UnityEngine.Debug.LogError("Car prefab needs a NetworkObject!");
        return go;
    }

    private void AssignRoles()
    {
        foreach (var a in MultiplayerManager.Instance.GetAllTeamAssignments())
        {
            var car = a.team == TeamType.TeamA ? team1CarInstance : team2CarInstance;
            var d = car.GetComponent<CarControllerWrapper>();
            var s = car.GetComponentInChildren<Shooter>();
            if (a.role == RoleType.Driver) d.AssignDriver(a.clientId);
            else { s.SetShooterAuthority(true); s.SetShooterClientId(a.clientId); }
        }
    }
}

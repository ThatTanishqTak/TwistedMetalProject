using UnityEngine;
using Unity.Netcode;

public class TurretController : NetworkBehaviour
{
    [Header("Turret Parts")]
    [SerializeField] private Transform cannonBase;
    [SerializeField] private Transform cannonHead;
    [SerializeField] private Transform shootPoint;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 45f;

    [Header("Shooter Reference")]
    [SerializeField] private Shooter shooter;
    [SerializeField] private GunStats gunStats;

    // cooldown timer
    private float lastFireTime;

    private NetworkVariable<Quaternion> baseRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<float> headPitch = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Update()
    {
        if (!shooter.IsShooterControlled ||
            NetworkManager.Singleton.LocalClientId != shooter.ShooterClientId)
            return;

        float mx = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
        UpdateRotationServerRpc(mx, my);

        if (Input.GetMouseButton(0))
        {
            if (Time.time >= lastFireTime + gunStats.fireRate)
            {
                lastFireTime = Time.time;
                FireServerRpc();
            }
        }
    }

    private void LateUpdate()
    {
        cannonBase.localRotation = baseRotation.Value;
        cannonHead.localEulerAngles = new Vector3(headPitch.Value, 0f, 0f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateRotationServerRpc(float yawDelta, float pitchDelta)
    {
        float newYaw = (baseRotation.Value.eulerAngles.y + yawDelta) % 360f;
        float newPitch = Mathf.Clamp(headPitch.Value - pitchDelta,
                                     minPitch, maxPitch);

        baseRotation.Value = Quaternion.Euler(0f, newYaw, 0f);
        headPitch.Value = newPitch;
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc()
    {
        if (Physics.Raycast(shootPoint.position,
                            shootPoint.forward,
                            out var hit,
                            gunStats.bulletRange))
        {
            if (hit.transform.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(gunStats.damage);
                Debug.Log($"[Server] Hit {hit.transform.name} for {gunStats.damage}  " +
                          $"(remaining {target.CurrentHealth})");
            }
            else
            {
                Debug.Log($"[Server] Hit {hit.transform.name}, not damageable");
            }
        }
        else
        {
            Debug.Log("[Server] Missed");
        }
    }
}

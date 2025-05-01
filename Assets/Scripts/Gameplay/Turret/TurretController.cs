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

    // Server‐only writes, everyone reads
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
        // only the *assigned* shooter client drives rotation/shooting
        if (!shooter.IsShooterControlled ||
            NetworkManager.Singleton.LocalClientId != shooter.ShooterClientId)
            return;

        // gather mouse deltas
        float mx = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        // send them to the server to update the NetworkVariables
        UpdateRotationServerRpc(mx, my);

        // fire if holding down
        if (Input.GetMouseButton(0))
            FireServerRpc();
    }

    private void LateUpdate()
    {
        // everyone applies the latest yaw & pitch
        cannonBase.localRotation = baseRotation.Value;
        cannonHead.localEulerAngles = new Vector3(headPitch.Value, 0f, 0f);
    }

    // Runs on the server—updates our yaw & pitch NVs from client‐sent deltas
    [ServerRpc(RequireOwnership = false)]
    private void UpdateRotationServerRpc(float yawDelta, float pitchDelta)
    {
        // apply yaw
        float newYaw = (baseRotation.Value.eulerAngles.y + yawDelta) % 360f;

        // apply and clamp pitch
        float newPitch = headPitch.Value - pitchDelta;
        newPitch = Mathf.Clamp(newPitch, minPitch, maxPitch);

        baseRotation.Value = Quaternion.Euler(0f, newYaw, 0f);
        headPitch.Value = newPitch;
    }

    // Your existing shooting RPC (no changes needed here)
    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out var hit))
            Debug.Log("Hit: " + hit.transform.name);
        else
            Debug.Log("Missed");
    }
}

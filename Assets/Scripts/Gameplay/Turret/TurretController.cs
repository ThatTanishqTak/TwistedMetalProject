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

    // Owner-written yaw on the base (around Y)
    private NetworkVariable<Quaternion> baseRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    // Owner-written pitch on the head (around X)
    private NetworkVariable<float> headPitch = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Update()
    {
        if (!shooter.IsShooterControlled ||
            NetworkManager.Singleton.LocalClientId != shooter.ShooterClientId)
            return;

        float mx = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        var current = baseRotation.Value.eulerAngles;
        float yaw = current.y + mx;

        float pitch = headPitch.Value - my;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        baseRotation.Value = Quaternion.Euler(0f, yaw, 0f);
        headPitch.Value = pitch;

        if (Input.GetMouseButton(0))
            FireServerRpc();
    }

    private void LateUpdate()
    {
        cannonBase.localRotation = baseRotation.Value;
        cannonHead.localEulerAngles = new Vector3(headPitch.Value, 0f, 0f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc()
    {
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out var hit))
            Debug.Log("Hit: " + hit.transform.name);
        else
            Debug.Log("Missed");
    }
}

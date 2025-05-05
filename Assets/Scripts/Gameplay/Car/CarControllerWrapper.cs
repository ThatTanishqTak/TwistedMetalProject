using UnityEngine;
using Unity.Netcode;

public class CarControllerWrapper : NetworkBehaviour
{
    [Header("Dependencies")]
    private ICarMovement carMovement;

    private NetworkVariable<ulong> drivingClientId = new NetworkVariable<ulong>(
        0ul,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    /// <summary>
    /// Expose for camera logic, VehicleCameraManager, etc.
    /// </summary>
    public ulong DrivingClientId => drivingClientId.Value;

    /// <summary>
    /// Called on the server, from CarSpawnerManager.AssignRolesToPlayers()
    /// </summary>
    public void AssignDriver(ulong clientId)
    {
        drivingClientId.Value = clientId;
    }

    private void Awake()
    {
        carMovement = GetComponent<ICarMovement>();
    }

    private void Update()
    {
        // Only clients should send input
        if (!IsClient) return;

        if (NetworkManager.Singleton.LocalClientId != drivingClientId.Value)
            return;

        // Gather inputs
        float throttle = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        bool handbrake = Input.GetKey(KeyCode.Space);

        SendMovementServerRpc(throttle, steering, handbrake);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMovementServerRpc(float throttle, float steering, bool handbrake)
    {
        carMovement?.Move(throttle, steering);
        carMovement?.ApplyHandbrake(handbrake);
    }
}

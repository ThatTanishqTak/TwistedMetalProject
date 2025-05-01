using UnityEngine;
using Unity.Netcode;

public class CarControllerWrapper : NetworkBehaviour
{
    [Header("Dependencies")]
    private ICarMovement carMovement;

    // Who’s allowed to drive? (set by the server in CarSpawnerManager)
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

        // Only the *assigned* driver client sends movement RPCs
        if (NetworkManager.Singleton.LocalClientId != drivingClientId.Value)
            return;

        // Gather inputs
        float throttle = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        bool handbrake = Input.GetKey(KeyCode.Space);

        // Send them to the server
        SendMovementServerRpc(throttle, steering, handbrake);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendMovementServerRpc(float throttle, float steering, bool handbrake)
    {
        // The server actually drives the car; its NetworkTransform will replicate
        carMovement?.Move(throttle, steering);
        carMovement?.ApplyHandbrake(handbrake);
    }
}

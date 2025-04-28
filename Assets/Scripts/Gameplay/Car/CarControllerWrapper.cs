using UnityEngine;
using Unity.Netcode;

public class CarControllerWrapper : NetworkBehaviour
{
    private ICarMovement carMovement;

    // Now a NetworkVariable as well:
    private NetworkVariable<ulong> drivingClientId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public ulong DrivingClientId => drivingClientId.Value;

    public override void OnNetworkSpawn()
    {
        carMovement = GetComponent<ICarMovement>();
    }

    public void AssignDriver(ulong clientId)
    {
        drivingClientId.Value = clientId;
    }

    private void Update()
    {
        if (NetworkManager.Singleton.LocalClientId != DrivingClientId)
            return;

        float throttle = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        bool handbrake = Input.GetKey(KeyCode.Space);

        carMovement?.Move(throttle, steering);
        carMovement?.ApplyHandbrake(handbrake);
    }
}
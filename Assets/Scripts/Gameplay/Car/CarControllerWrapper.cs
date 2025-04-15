using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class CarControllerWrapper : NetworkBehaviour
{
    private ICarMovement carMovement;
    private ulong drivingClientId;

    private void AssignDriver(ulong clientId) {
        drivingClientId = clientId;
    }

    private void Awake()
    {
        carMovement = GetComponent<ICarMovement>();
    }

    private void Update()
    {
        if (!IsOwner || NetworkManager.Singleton.LocalClientId != drivingClientId)
        {
            return;
        }
        else {
            float throttle = Input.GetAxis("Vertical");
            float steering = Input.GetAxis("Horizontal");

            carMovement.Move(throttle, steering);
        }
    }
}

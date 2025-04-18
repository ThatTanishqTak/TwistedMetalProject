using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class CarControllerWrapper : NetworkBehaviour
{
    private ICarMovement carMovement;
    private ulong drivingClientId;

    public void AssignDriver(ulong clientId) {
        drivingClientId = clientId;
    }

    private void Awake()
    {
        carMovement = GetComponent<ICarMovement>();
    }

    private void Update()
    {
        ///if (!IsOwner || NetworkManager.Singleton.LocalClientId != drivingClientId)
        ///{
        ///   return;
        /// }
        /// else {
        Debug.Log("Wrapper Running"); 
        float throttle = Input.GetAxis("Vertical");
        float steering = Input.GetAxis("Horizontal");
        bool handbrake = Input.GetKey(KeyCode.Space);

        carMovement?.Move(throttle, steering);
        carMovement?.ApplyHandbrake(handbrake);

        carMovement.Move(throttle, steering);
       /// }
    }
}

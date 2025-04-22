using UnityEngine;
using Unity.Netcode;

public class VehicleCameraManager : NetworkBehaviour
{
    [SerializeField] private Camera shooterCamera;
    [SerializeField] private Camera driverCamera;

    [SerializeField] private Shooter shooter;
    [SerializeField] private CarControllerWrapper driver;

    private void Start()
    {
        if (!IsOwner)
        {
            shooterCamera.gameObject.SetActive(false);
            driverCamera.gameObject.SetActive(false);
            return;
        }

        if (shooter != null && shooter.IsShooterControlled)
        {
            shooterCamera.gameObject.SetActive(true);
            driverCamera.gameObject.SetActive(false);
        }
        else if (driver != null && driver.IsDriving)
        {
            shooterCamera.gameObject.SetActive(false);
            driverCamera.gameObject.SetActive(true);
        }
    }
}

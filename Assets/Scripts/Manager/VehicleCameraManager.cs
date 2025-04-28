using UnityEngine;
using Unity.Netcode;

public class VehicleCameraManager : NetworkBehaviour
{
    [SerializeField] private Shooter shooter;
    [SerializeField] private CarControllerWrapper driver;
    [SerializeField] private GameObject shooterCamera;
    [SerializeField] private GameObject driverCamera;

    private void Awake()
    {
        //mainCamera.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (!IsOwner)
        {
            return; 
        }

        shooterCamera.SetActive(false);
        driverCamera.SetActive(false);

        if (shooter != null && shooter.IsShooterControlled && shooter.IsOwner)
        {
            shooterCamera.SetActive(true);
            driverCamera.SetActive(false);
        }
        else if (driver != null && driver.IsOwner)
        {
            driverCamera.SetActive(true);
            shooterCamera.SetActive(false);
        }
    }
}

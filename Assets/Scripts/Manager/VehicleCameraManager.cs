using UnityEngine;

public class VehicleCameraManager : MonoBehaviour
{
    [SerializeField] private Shooter shooter;
    [SerializeField] private CarControllerWrapper driver;
    [SerializeField] private GameObject shooterCamera;
    [SerializeField] private GameObject driverCamera;

    private void Start()
    {
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

using UnityEngine;

public class ManualTestSetup : MonoBehaviour
{
    public CarControllerWrapper car;
    public TurretController turret;
    public Shooter shooter;

    private void Start()
    {
        if (car == null)
        {
            Debug.LogError("car Missing");
            
        }
        else if (turret == null)
        {
            Debug.Log("Turret Missing");

        }
        else if (shooter == null) {
            Debug.Log("shooter Missing");
        }
        else
        {
            Debug.Log("All references are set.");
        }

        Debug.Log("ManualTestSetup starting");

        car.AssignDriver(0);
        turret.AssignShooter(true);
        shooter.SetShooterAuthority(true);
    }
}

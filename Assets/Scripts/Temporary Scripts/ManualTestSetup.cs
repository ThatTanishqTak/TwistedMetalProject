using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class ManualTestSetup : MonoBehaviour
{
    public CarControllerWrapper car;
    public TurretController turret;

    private void Start()
    {
        // 🚨 TEMPORARILY REMOVE multiplayer checks — just test inputs
        car.AssignDriver(0);     // 0 is fake clientId for test
        turret.AssignShooter(true); // Force shooter role
    }
}

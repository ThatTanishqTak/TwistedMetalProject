using System;
using UnityEngine;
using Unity.Netcode;

public class VehicleCameraManager : MonoBehaviour
{
    [Header("Role Components")]
    [SerializeField] private Shooter shooter;
    [SerializeField] private CarControllerWrapper driver;

    [Header("Cameras")]
    [SerializeField] private Camera lobbyCamera;
    [SerializeField] private Camera driverCamera;
    [SerializeField] private Camera shooterCamera;

    private void Awake()
    {
        // only lobby cam starts active
        if (driverCamera != null) driverCamera.gameObject.SetActive(false);
        if (shooterCamera != null) shooterCamera.gameObject.SetActive(false);
    }

    private void Start()
    {
        // in case teams were already formed before we subscribed
        if (MultiplayerManager.Instance.GetAllTeamAssignments().Count > 0)
            HandleTeamsFormed(this, EventArgs.Empty);
    }

    private void OnEnable()
    {
        MultiplayerManager.Instance.OnTeamsFormed += HandleTeamsFormed;
    }

    private void OnDisable()
    {
        MultiplayerManager.Instance.OnTeamsFormed -= HandleTeamsFormed;
    }

    private void HandleTeamsFormed(object sender, EventArgs args)
    {
        if (!NetworkManager.Singleton.IsClient) return;

        if (lobbyCamera != null)
            lobbyCamera.gameObject.SetActive(false);

        if (shooter != null
            && shooter.IsShooterControlled
            && shooter.ShooterClientId == NetworkManager.Singleton.LocalClientId)
        {
            shooterCamera.gameObject.SetActive(true);
            return;
        }

        if (driver != null
            && driver.DrivingClientId == NetworkManager.Singleton.LocalClientId)
        {
            driverCamera.gameObject.SetActive(true);
            return;
        }

    }
}

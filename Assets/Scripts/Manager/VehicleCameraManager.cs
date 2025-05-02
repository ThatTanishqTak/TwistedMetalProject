using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class VehicleCameraManager : MonoBehaviour
{
    [Header("Role Components")]
    [SerializeField] private Shooter shooter;
    [SerializeField] private CarControllerWrapper driver;

    [Header("Cameras (in-car)")]
    [SerializeField] private Camera driverCamera;
    [SerializeField] private Camera shooterCamera;

    // will be assigned at runtime by tag
    private Camera lobbyCamera;

    private void Awake()
    {
        // find the main lobby camera by tag
        var mainCamGO = GameObject.FindWithTag("CameraMain");
        if (mainCamGO != null)
            lobbyCamera = mainCamGO.GetComponent<Camera>();
        else
            Debug.LogWarning("VehicleCameraManager: no GameObject tagged 'CameraMain' found.");

        // ensure only the in-car cameras start disabled
        if (driverCamera != null) driverCamera.gameObject.SetActive(false);
        if (shooterCamera != null) shooterCamera.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        MultiplayerManager.Instance.OnTeamsFormed += HandleTeamsFormed;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        MultiplayerManager.Instance.OnTeamsFormed -= HandleTeamsFormed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // if teams had already formed before we subscribed
        if (MultiplayerManager.Instance.GetAllTeamAssignments().Count > 0)
            SetupCameras();
    }

    private void HandleTeamsFormed(object sender, EventArgs args)
    {
        // teams just got set up in the Lobby
        SetupCameras();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // when we load the Arena, shut off the main camera again
        if (scene.name == Loader.Scene.Arena.ToString())
        {
            if (lobbyCamera != null)
                lobbyCamera.gameObject.SetActive(false);
            SetupCameras();
        }
    }

    private void SetupCameras()
    {
        // only run on clients
        if (!NetworkManager.Singleton.IsClient) return;

        // 1) disable the main/lobby camera
        if (lobbyCamera != null)
            lobbyCamera.gameObject.SetActive(false);

        // 2) shooter?
        if (shooter != null
            && shooter.IsShooterControlled
            && shooter.ShooterClientId == NetworkManager.Singleton.LocalClientId)
        {
            shooterCamera?.gameObject.SetActive(true);
            driverCamera?.gameObject.SetActive(false);
            return;
        }

        // 3) driver?
        if (driver != null
            && driver.DrivingClientId == NetworkManager.Singleton.LocalClientId)
        {
            driverCamera?.gameObject.SetActive(true);
            shooterCamera?.gameObject.SetActive(false);
            return;
        }

        // 4) otherwise, disable both in-car cams
        driverCamera?.gameObject.SetActive(false);
        shooterCamera?.gameObject.SetActive(false);
    }
}

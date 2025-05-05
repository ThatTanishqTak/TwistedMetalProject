using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class HealthUIManager : MonoBehaviour
{
    [Header("Hook these up in the Inspector")]
    [SerializeField] private TextMeshProUGUI roleLabel;

    private GameObject respawnPanel;
    private Health _healthComponent;
    private bool _subscribed;

    private void Awake()
    {
        Debug.Log("[HealthUI] Awake – persisting");
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Debug.Log("[HealthUI] OnEnable – isClient? " + NetworkManager.Singleton.IsClient);
        if (!NetworkManager.Singleton.IsClient)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!_subscribed)
        {
            Debug.Log("[HealthUI] Subscribing OnTeamsFormed & sceneLoaded");
            MultiplayerManager.Instance.OnTeamsFormed += OnTeamsFormed;
            SceneManager.sceneLoaded += OnSceneLoaded;
            _subscribed = true;
        }

        TryInitialize();
    }

    private void OnDisable()
    {
        Debug.Log("[HealthUI] OnDisable – unhooking");
        if (_subscribed)
        {
            MultiplayerManager.Instance.OnTeamsFormed -= OnTeamsFormed;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _subscribed = false;
        }

        if (_healthComponent != null)
        {
            _healthComponent.OnDied -= ShowRespawnPanel;
            _healthComponent.OnRespawned -= HideRespawnPanel;
            _healthComponent = null;
        }
    }

    private void OnTeamsFormed(object _, EventArgs __)
    {
        Debug.Log("[HealthUI] OnTeamsFormed");
        TryInitialize();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[HealthUI] Scene loaded: {scene.name}");
        if (scene.name == Loader.Scene.Arena.ToString())
        {
            respawnPanel = GameObject.Find("RespawnCanvas")
                             ?.transform.Find("RespawnPanel")
                             ?.gameObject;
            if (respawnPanel == null)
                Debug.LogWarning("[HealthUI] RespawnPanel NOT found!");
            else
            {
                Debug.Log("[HealthUI] RespawnPanel found, hiding initially");
                respawnPanel.SetActive(false);
            }
        }

        TryInitialize();
    }

    private void TryInitialize()
    {
        Debug.Log("[HealthUI] TryInitialize()");
        if (_healthComponent != null)
        {
            Debug.Log("[HealthUI] Already have Health, skipping");
            return;
        }

        var myId = NetworkManager.Singleton.LocalClientId;
        var assignment = MultiplayerManager.Instance
                             .GetAllTeamAssignments()
                             .Find(a => a.clientId == myId);
        if (assignment.Equals(default(TeamRoleData)))
        {
            Debug.Log("[HealthUI] No assignment yet");
            return;
        }

        roleLabel.text = $"{assignment.team} {assignment.role}";
        gameObject.SetActive(true);

        Debug.Log("[HealthUI] Looking for PlayerCar tagged objects…");
        foreach (var car in GameObject.FindGameObjectsWithTag("PlayerCar"))
        {
            var wrapper = car.GetComponent<CarControllerWrapper>();
            if (wrapper != null && wrapper.DrivingClientId == myId)
            {
                HookInto(car);
                return;
            }

            var shooter = car.GetComponentInChildren<Shooter>();
            if (shooter != null
             && shooter.IsShooterControlled
             && shooter.ShooterClientId == myId)
            {
                HookInto(car);
                return;
            }
        }

        Debug.LogWarning("[HealthUI] No PlayerCar with my clientId found yet; will retry when teams form or next scene load.");
    }

    private void HookInto(GameObject car)
    {
        Debug.Log($"[HealthUI] Found my car ({car.name}), hooking Health…");
        _healthComponent = car.GetComponent<Health>();
        if (_healthComponent == null)
        {
            Debug.LogError("[HealthUI] Car has no Health component!");
            return;
        }

        _healthComponent.OnDied += ShowRespawnPanel;
        _healthComponent.OnRespawned += HideRespawnPanel;
    }

    private void ShowRespawnPanel()
    {
        Debug.Log("[HealthUI] ShowRespawnPanel()");
        if (respawnPanel != null)
        {
            respawnPanel.SetActive(true);
            StartCoroutine(RespawnCountdownCoroutine());
        }
    }

    private void HideRespawnPanel()
    {
        Debug.Log("[HealthUI] HideRespawnPanel()");
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator RespawnCountdownCoroutine()
    {
        float remaining = 5f;

        var timerText = respawnPanel
            .transform
            .Find("RespawningSeconds/SecondsTMP")
            ?.GetComponent<TextMeshProUGUI>();

        if (timerText == null)
        {
            Debug.LogWarning("[HealthUI] SecondsTMP not found under RespawningSeconds!");
            yield break;
        }

        while (remaining > 0f)
        {
            timerText.text = $"Respawning in {remaining:0}s";
            yield return new WaitForSeconds(1f);
            remaining -= 1f;
        }

        timerText.text = "Respawning in 0s";
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        respawnPanel.SetActive(false);
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsClient
            && _healthComponent == null
            && SceneManager.GetActiveScene().name == Loader.Scene.Arena.ToString())
        {
            Debug.Log("[HealthUI] Update(): retrying TryInitialize");
            TryInitialize();
        }
    }
}

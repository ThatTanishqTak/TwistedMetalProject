using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HealthUIManager : MonoBehaviour
{
    [Header("Hook these up in the Inspector")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI roleLabel;

    private Health _healthComponent;
    private bool _subscribed;

    private void Awake()
    {
        Debug.Log("[HealthUI] Awake – making me persistent");
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Debug.Log("[HealthUI] OnEnable – isClient? " + NetworkManager.Singleton.IsClient);
        // Only clients ever see the UI
        if (!NetworkManager.Singleton.IsClient)
        {
            Debug.Log("[HealthUI] Not a client; hiding UI");
            gameObject.SetActive(false);
            return;
        }

        // Subscribe events exactly once
        if (!_subscribed)
        {
            Debug.Log("[HealthUI] Subscribing OnTeamsFormed & sceneLoaded");
            MultiplayerManager.Instance.OnTeamsFormed += OnTeamsFormed;
            SceneManager.sceneLoaded += OnSceneLoaded;
            _subscribed = true;
        }

        // Try setup immediately in case teams are already formed
        TryInitialize();
    }

    private void OnDisable()
    {
        Debug.Log("[HealthUI] OnDisable – cleaning up");
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && _subscribed)
        {
            Debug.Log("[HealthUI] Unsubscribing events");
            MultiplayerManager.Instance.OnTeamsFormed -= OnTeamsFormed;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _subscribed = false;
        }

        if (_healthComponent != null)
        {
            Debug.Log("[HealthUI] Unhooking OnHealthChangedEvent");
            _healthComponent.OnHealthChangedEvent -= OnHealthChanged;
            _healthComponent = null;
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[HealthUI] OnDestroy");
        OnDisable();
    }

    private void OnTeamsFormed(object _, EventArgs __)
    {
        Debug.Log("[HealthUI] OnTeamsFormed");
        TryInitialize();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[HealthUI] OnSceneLoaded: {scene.name}");
        TryInitialize();
    }

    private void TryInitialize()
    {
        // already hooked?
        if (_healthComponent != null)
        {
            Debug.Log("[HealthUI] Already initialized, skipping");
            return;
        }

        // 1) find my assignment
        var myId = NetworkManager.Singleton.LocalClientId;
        var assignment = MultiplayerManager.Instance
            .GetAllTeamAssignments()
            .FirstOrDefault(a => a.clientId == myId);

        Debug.Log($"[HealthUI] My assignment: {assignment.team} / {assignment.role}");
        if (assignment.Equals(default(TeamRoleData)))
        {
            // not yet assigned, show an empty UI
            roleLabel.text = "";
            return;
        }

        // 2) show & label
        Debug.Log("[HealthUI] Showing UI and setting label");
        gameObject.SetActive(true);
        roleLabel.text = $"{assignment.team} {assignment.role}";

        // 3) find my car via the spawner
        var spawner = UnityEngine.Object.FindFirstObjectByType<CarSpawnerManager>();
        if (spawner == null)
        {
            Debug.LogError("[HealthUI] No CarSpawnerManager found!");
            return;
        }

        var car = spawner.GetCarForTeam(assignment.team);
        if (car == null)
        {
            Debug.LogError($"[HealthUI] No car for team {assignment.team}");
            return;
        }

        // 4) hook into its Health component
        _healthComponent = car.GetComponent<Health>();
        if (_healthComponent == null)
        {
            Debug.LogError("[HealthUI] Car has no Health component!");
            return;
        }

        Debug.Log("[HealthUI] Initializing slider values");
        healthSlider.maxValue = _healthComponent.MaxHealth;
        healthSlider.value = _healthComponent.CurrentHealth;

        Debug.Log("[HealthUI] Subscribing to OnHealthChangedEvent");
        _healthComponent.OnHealthChangedEvent += OnHealthChanged;
    }

    private void OnHealthChanged(float oldHp, float newHp)
    {
        Debug.Log($"[HealthUI] Health changed from {oldHp} → {newHp}");
        healthSlider.value = newHp;
    }
}

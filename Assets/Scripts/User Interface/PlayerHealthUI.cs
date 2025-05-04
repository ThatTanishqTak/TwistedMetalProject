using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;   
    [SerializeField] private TextMeshProUGUI roleLabel;

    private Health healthComponent;
    private bool initialized;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (!NetworkManager.Singleton.IsClient) return;
        MultiplayerManager.Instance.OnTeamsFormed += OnTeamsFormed;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        MultiplayerManager.Instance.OnTeamsFormed -= OnTeamsFormed;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (healthComponent != null)
            healthComponent.OnHealthChangedEvent -= OnHealthChanged;
    }

    private void OnTeamsFormed(object s, EventArgs e) => TryInitialize();
    private void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (scene.name == Loader.Scene.Arena.ToString()) TryInitialize();
        else gameObject.SetActive(false);
    }

    private void TryInitialize()
    {
        if (initialized) return;

        var myId = NetworkManager.Singleton.LocalClientId;
        var assignment = MultiplayerManager.Instance
                          .GetAllTeamAssignments()
                          .FirstOrDefault(a => a.clientId == myId);
        if (assignment.Equals(default(TeamRoleData))) return;

        gameObject.SetActive(true);
        roleLabel.text = $"{assignment.team} {assignment.role}";

        var spawner = UnityEngine.Object.FindFirstObjectByType<CarSpawnerManager>();
        var car = spawner?.GetCarForTeam(assignment.team);
        healthComponent = car?.GetComponent<Health>();
        if (healthComponent == null) return;

        healthSlider.maxValue = healthComponent.MaxHealth;
        healthSlider.value = healthComponent.CurrentHealth;
        UpdateFillColor(healthComponent.CurrentHealth);

        healthComponent.OnHealthChangedEvent += OnHealthChanged;
        initialized = true;
    }

    private void OnHealthChanged(float oldHp, float newHp)
    {
        healthSlider.value = newHp;
        UpdateFillColor(newHp);
    }

    private void UpdateFillColor(float hp)
    {
        float pct = hp / healthSlider.maxValue;
        if (pct >= 0.7f) fillImage.color = Color.green;
        else if (pct >= 0.3f) fillImage.color = Color.yellow;
        else fillImage.color = Color.red;
    }
}

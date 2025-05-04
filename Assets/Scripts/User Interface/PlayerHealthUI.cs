using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI roleLabel;

    private Health healthComponent;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsClient) return;
        StartCoroutine(InitializeNextFrame());
    }

    private IEnumerator InitializeNextFrame()
    {
        yield return null;

        ulong myId = NetworkManager.Singleton.LocalClientId;
        var assignment = MultiplayerManager
            .Instance
            .GetAllTeamAssignments()
            .FirstOrDefault(a => a.clientId == myId);

        if (assignment.Equals(default(TeamRoleData)))
        {
            Debug.LogError($"[PlayerHealthUI] No assignment for client {myId}");
            enabled = false;
            yield break;
        }

        roleLabel.text = $"{assignment.team} {assignment.role}";

        var spawner = Object.FindAnyObjectByType<CarSpawnerManager>();
        if (spawner == null)
        {
            Debug.LogError("[PlayerHealthUI] No CarSpawnerManager in scene!");
            enabled = false;
            yield break;
        }

        var car = spawner.GetCarForTeam(assignment.team);
        if (car == null)
        {
            Debug.LogError($"[PlayerHealthUI] No car for {assignment.team}");
            enabled = false;
            yield break;
        }

        healthComponent = car.GetComponent<Health>();
        if (healthComponent == null)
        {
            Debug.LogError("[PlayerHealthUI] Car missing Health!");
            enabled = false;
            yield break;
        }

        healthSlider.maxValue = healthComponent.MaxHealth;
        healthSlider.value = healthComponent.CurrentHealth;
        UpdateFillColor(healthComponent.CurrentHealth);

        healthComponent.OnHealthChangedEvent += OnHealthChanged;
    }

    private void OnHealthChanged(float oldHp, float newHp)
    {
        healthSlider.value = newHp;
        UpdateFillColor(newHp);
    }

    private void UpdateFillColor(float hp)
    {
        float pct = hp / healthSlider.maxValue;
        if (pct >= 0.7f)
            fillImage.color = Color.green;
        else if (pct >= 0.3f)
            fillImage.color = Color.yellow;
        else
            fillImage.color = Color.red;
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
            healthComponent.OnHealthChangedEvent -= OnHealthChanged;
    }
}

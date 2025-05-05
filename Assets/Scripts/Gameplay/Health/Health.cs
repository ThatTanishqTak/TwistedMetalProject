using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : NetworkBehaviour, IDamageable
{
    [Header("Health Settings")]
    [Tooltip("Max HP for this object")]
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;

    [Tooltip("Seconds to wait before respawning in Arena")]
    [SerializeField] private float respawnDelay = 5f;

    public event Action<float, float> OnHealthChangedEvent;
    public event Action OnDied;
    public event Action OnRespawned;

    public NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Vector3 spawnPos;
    private Quaternion spawnRot;
    private bool visualsHidden = false;
    private bool healthEnabled = false;

    public float CurrentHealth => currentHealth.Value;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            spawnPos = transform.position;
            spawnRot = transform.rotation;
            currentHealth.Value = maxHealth;
        }

        currentHealth.OnValueChanged += HandleHealthChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        healthEnabled = (scene.name == Loader.Scene.Arena.ToString());
        if (healthEnabled && IsServer)
        {
            currentHealth.Value = maxHealth;
            SetVisuals(true);
        }
    }

    private void HandleHealthChanged(float oldHp, float newHp)
    {
        OnHealthChangedEvent?.Invoke(oldHp, newHp);
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer || !healthEnabled) return;

        float oldHp = currentHealth.Value;
        float newHp = Mathf.Max(0f, oldHp - amount);
        currentHealth.Value = newHp;

        OnHealthChangedEvent?.Invoke(oldHp, newHp);
        HealthChangedClientRpc(oldHp, newHp);

        if (newHp <= 0f)
            Die();
    }

    private void Die()
    {
        DeathClientRpc();
        if (SceneManager.GetActiveScene().name == Loader.Scene.Arena.ToString())
            StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        if (!healthEnabled) yield break;

        currentHealth.Value = maxHealth;
        transform.position = spawnPos;
        transform.rotation = spawnRot;
        RespawnClientRpc();
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        SetVisuals(false);
        OnDied?.Invoke();
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        SetVisuals(true);
        OnRespawned?.Invoke();
    }

    [ClientRpc]
    private void HealthChangedClientRpc(float oldHp, float newHp)
    {
        OnHealthChangedEvent?.Invoke(oldHp, newHp);
    }

    private void SetVisuals(bool active)
    {
        if (visualsHidden == !active) return;
        visualsHidden = !active;

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = active;
        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = active;
    }

    private new void OnDestroy()
    {
        currentHealth.OnValueChanged -= HandleHealthChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

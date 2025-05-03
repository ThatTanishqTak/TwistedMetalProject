using System.Collections;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class Health : NetworkBehaviour, IDamageable
{
    [Header("Health Settings")]
    [Tooltip("Max HP for this object")]
    [SerializeField] private float maxHealth = 100f;
    [Tooltip("Seconds to wait before respawning in Arena")]
    [SerializeField] private float respawnDelay = 5f;

    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Vector3 spawnPos;
    private Quaternion spawnRot;
    private bool visualsHidden = false;

    public float CurrentHealth => currentHealth.Value;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            spawnPos = transform.position;
            spawnRot = transform.rotation;
            currentHealth.Value = maxHealth;
        }
        currentHealth.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(float oldHp, float newHp)
    {
        //Left for healthbar UI
    }

    public void TakeDamage(float amount)
    {
        if (!IsServer) return;

        currentHealth.Value = Mathf.Max(0f, currentHealth.Value - amount);
        if (currentHealth.Value <= 0f)
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

        currentHealth.Value = maxHealth;

        transform.position = spawnPos;
        transform.rotation = spawnRot;

        RespawnClientRpc();
    }

    [ClientRpc]
    private void DeathClientRpc()
    {
        SetVisuals(false);
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        SetVisuals(true);
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
        currentHealth.OnValueChanged -= OnHealthChanged;
    }
}

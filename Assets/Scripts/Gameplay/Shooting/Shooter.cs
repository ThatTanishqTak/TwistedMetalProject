using UnityEngine;
using Unity.Netcode;

public class Shooter : NetworkBehaviour
{
    [SerializeField] private Gun gun;

    // These are now NetworkVariables, written only by the server
    private NetworkVariable<ulong> shooterClientId = new NetworkVariable<ulong>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> isShooterControlled = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public ulong ShooterClientId => shooterClientId.Value;
    public bool IsShooterControlled => isShooterControlled.Value;

    // Called by CarSpawnerManager *on the server* to set up authority
    public void SetShooterAuthority(bool value)
    {
        isShooterControlled.Value = value;
    }

    public void SetShooterClientId(ulong clientId)
    {
        shooterClientId.Value = clientId;
    }

    private void Update()
    {
        // We no longer fire here — all firing goes through TurretController.
        // But if you want client‐side effects you can still use this:
        if (!IsShooterControlled || NetworkManager.Singleton.LocalClientId != ShooterClientId)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            gun.Fire();
        }
    }
}
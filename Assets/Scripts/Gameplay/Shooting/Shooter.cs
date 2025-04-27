using UnityEngine;
using Unity.Netcode;

public class Shooter : NetworkBehaviour
{
    [SerializeField] private Gun gun;
    private ulong shooterClientId;
    private bool isShooterControlled;

    public ulong ShooterClientId => shooterClientId;
    public bool IsShooterControlled => isShooterControlled;

    public void SetShooterAuthority(bool value)
    {
        isShooterControlled = value;
    }

    public void SetShooterClientId(ulong clientId)
    {
        shooterClientId = clientId;
    }

    private void Update()
    {
        if (!IsOwner || NetworkManager.Singleton.LocalClientId != shooterClientId) return;
        if (!isShooterControlled) return;

        if (Input.GetMouseButton(0))
        {
            gun.Fire();
        }
    }
}

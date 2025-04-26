using UnityEngine;
using Unity.Netcode;

public class Shooter : NetworkBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private bool isShooterControlled;

    public bool IsShooterControlled => isShooterControlled;

    private ulong shooterClientId;

    private void Update()
    {
        if (!IsOwner || NetworkManager.Singleton.LocalClientId != shooterClientId)
        {
            return;
        }

        if (!isShooterControlled) return;

        if (Input.GetMouseButton(0))
        {
            Debug.Log("Mouse Clicked");
            gun.Fire();
        }
    }

    public void SetShooterAuthority(bool value)
    {
        isShooterControlled = value;
    }

    public void SetShooterClientId(ulong clientId)
    {
        shooterClientId = clientId;
    }
}

using Unity.Netcode;
using UnityEngine;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<GameManager.PlayerRoles> roles = new();
    public NetworkVariable<GameManager.Teams> teams = new();

    [SerializeField] private CharacterController characterController;

    [SerializeField] private float speed;

    [SerializeField] private CarDataScriptableObject carDataSO;

    private void Update()
    {
        if (!IsOwner) { return; }

        MovePlayer();

        if (roles.Value == GameManager.PlayerRoles.Driver)
        {
            PlayerDrive();
        }

        else
        {
            PlayerShoot();
        }
    }

    private void PlayerDrive()
    {
        Debug.Log($"Client ID: {NetworkManager.Singleton.LocalClientId}, Your team: {teams.Value}, your role: {roles.Value}");
    }

    private void PlayerShoot()
    {
        Debug.Log($"Client ID: {NetworkManager.Singleton.LocalClientId}, Your team: {teams.Value}, your role: {roles.Value}");
    }

    private void MovePlayer() // This is for the lobby only
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = Time.deltaTime * speed * new Vector3(horizontal, 0.0f, vertical);

        characterController.Move(movement);
        if (movement != Vector3.zero) { this.gameObject.transform.forward = movement; }
    }
}
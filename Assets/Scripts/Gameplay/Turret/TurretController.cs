using UnityEngine;
using Unity.Netcode;

public class TurretController : NetworkBehaviour
{
    [Header("Turret Transforms")]
    [SerializeField] private Transform cannonBase;
    [SerializeField] private Transform cannonHead;
    [SerializeField] private Transform shootPoint;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float minVerticalAngle = -10f;
    [SerializeField] private float maxVerticalAngle = 45f;

    [Header("Projectile Settings")]
    [SerializeField] private Shooter shooter;  // Reference to the Shooter component

    private void Update()
    {
        // 1) Must be flagged as the shooter
        // 2) Must be *this* client’s ID
        if (shooter == null
            || !shooter.IsShooterControlled
            || NetworkManager.Singleton.LocalClientId != shooter.ShooterClientId)
            return;

        // Only the assigned shooter on their own client may rotate
        RotateTurret();

        // And only the assigned shooter may fire
        if (Input.GetMouseButton(0))
        {
            FireServerRpc();
        }
    }

    private void RotateTurret()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        cannonBase.Rotate(Vector3.up * mouseX * rotationSpeed * Time.deltaTime);

        Vector3 headRotation = cannonHead.localEulerAngles;
        headRotation.x -= mouseY * rotationSpeed * Time.deltaTime;
        headRotation.x = ClampAngle(headRotation.x, minVerticalAngle, maxVerticalAngle);
        cannonHead.localEulerAngles = headRotation;
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit))
            Debug.Log("Hit: " + hit.transform.gameObject.name);
        else
            Debug.Log("Missed");
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
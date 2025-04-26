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
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireForce = 500f;

    [SerializeField] private Shooter shooter;  // Shooter script reference

    private void Update()
    {
        if (!IsOwner) return;   // <--- Super important (only owner controls turret)

        if (shooter != null && shooter.IsShooterControlled)
        {
            RotateTurret();

            if (Input.GetMouseButton(0))  // Hold mouse button to fire
            {
                FireServerRpc();
            }
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

    [ServerRpc]
    private void FireServerRpc(ServerRpcParams rpcParams = default)
    {
        GameObject bullet = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        var rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shootPoint.forward * fireForce;
        }
        bullet.GetComponent<NetworkObject>().Spawn();
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}

using UnityEngine;
using Unity.Netcode;

public class TurretController : NetworkBehaviour
{
    [Header("Turret Transforms")]
    [SerializeField] private Transform cannonBase;      // rotates Y (left/right)
    [SerializeField] private Transform cannonHead;      // rotates X (up/down)
    [SerializeField] private Transform shootPoint;      // where the bullet spawns

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float minVerticalAngle = -10f;
    [SerializeField] private float maxVerticalAngle = 45f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireForce = 500f;

    private bool isShooter;

    public void AssignShooter(bool shooterAuthority)
    {
        isShooter = shooterAuthority;
    }

    private void Update()
    {
        //if (!IsOwner || !isShooter) return;

        RotateTurret();

        if (Input.GetMouseButtonDown(0))
        {
            FireServerRpc();
        }
    }

    private void RotateTurret()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Horizontal (Y-axis)
        cannonBase.Rotate(Vector3.up * mouseX * rotationSpeed * Time.deltaTime);

        // Vertical (X-axis)
        Vector3 currentEuler = cannonHead.localEulerAngles;
        currentEuler.x -= mouseY * rotationSpeed * Time.deltaTime;
        currentEuler.x = ClampAngle(currentEuler.x, minVerticalAngle, maxVerticalAngle);
        cannonHead.localEulerAngles = currentEuler;
    }

    [ServerRpc]
    private void FireServerRpc()
    {
        GameObject bullet = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(shootPoint.forward * fireForce, ForceMode.Impulse);
        bullet.GetComponent<NetworkObject>().Spawn();
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}

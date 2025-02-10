using Unity.Netcode;
using UnityEngine;

public class Shooting : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed;

    private void Update()
    {
        if (!IsOwner) { return; }

        //Shoot();
    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            // Apply force to the bullet to make it move forward
            bulletRb.linearVelocity = firePoint.forward * bulletSpeed;

            // Optional: Destroy the bullet after a certain time to clean up
            Destroy(bullet, 3f);
        }
    }
}
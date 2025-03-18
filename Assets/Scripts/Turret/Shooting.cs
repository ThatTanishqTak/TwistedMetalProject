using Unity.Netcode;
using UnityEngine;

/* --FINAL FIXED SHOOTING SYSTEM-- */

public class Shooting : NetworkBehaviour
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] public float fireRate = 1.0f; // ✅ Slower default fire rate (1 bullet per second)

    private bool canShoot = true; // ✅ Prevents spam

    private void Start()
    {
        if (bullet == null)
        {
            Debug.LogError("Bullet prefab not found! Make sure it's assigned in the Inspector.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && canShoot) // ✅ Fire only once per click
        {
            Shoot();
            StartCoroutine(FireCooldown());
        }
    }

    private void Shoot()
    {
        Vector3 shootDirection = shootPoint.forward;
        InstantiateBullet(shootPoint.position, shootDirection);
    }

    private void InstantiateBullet(Vector3 spawnPosition, Vector3 direction)
    {
        GameObject bulletPrefab = Instantiate(bullet, spawnPosition, Quaternion.LookRotation(direction));

        Rigidbody bulletRb = bulletPrefab.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            bulletRb.useGravity = false;
            bulletRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            bulletRb.AddForce(direction * bulletSpeed, ForceMode.VelocityChange);
        }

        Destroy(bulletPrefab, 3f); // ✅ Destroy bullet after 3 seconds
    }

    private System.Collections.IEnumerator FireCooldown()
    {
        canShoot = false; // ✅ Prevents multiple shots per click
        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    public void DoubleFireRate(float duration)
    {
        Debug.Log("Power-up activated! Fire rate before: " + fireRate);

        fireRate /= 5f; // ✅ Now fires 5x faster after power-up
        fireRate = Mathf.Max(fireRate, 0.1f); // ✅ Prevents fire rate from going too low

        Debug.Log("Fire rate BOOSTED to: " + fireRate);

        StartCoroutine(ResetFireRateAfterDelay(duration));
    }

    private System.Collections.IEnumerator ResetFireRateAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);

        fireRate *= 5f; // ✅ Reset fire rate to normal
        Debug.Log("Power-up expired. Fire rate reset to: " + fireRate);
    }
}

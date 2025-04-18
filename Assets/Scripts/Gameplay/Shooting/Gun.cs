using UnityEngine;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class Gun : MonoBehaviour, IShooter
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GunStats gunStats;

    private float lastFireTime;

    public event Action OnFire;

    public void Fire()
    {
        if (Time.time < lastFireTime + gunStats.fireRate)
        {
            return;
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("bulletPrefab Missing");
        }
        else if (shootPoint == null) {
            Debug.Log("shootPoint Missing");
        }

            GameObject bullet = Instantiate(gunStats.projectilePrefab, shootPoint.position, shootPoint.rotation);

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.AddForce(shootPoint.forward * gunStats.fireRate, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Bullet prefab does not have a Rigidbody component.");
        }

        lastFireTime = Time.time;

        Debug.Log("Bullet Fired!");
        OnFire?.Invoke(); // For future muzzle flash, sounds etc.
    }

    public void SetStats(GunStats newStats) {
        gunStats = newStats;
    }
}

using UnityEngine;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class Gun : MonoBehaviour, IShooter
{
    [SerializeField] private Transform shootPoint;
    //[SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GunStats gunStats;

    private float lastFireTime;

    public event Action OnFire;

    public void Fire()
    {
        if (Time.time < lastFireTime + gunStats.fireRate)
            return;

        //GameObject bullet = Instantiate(gunStats.projectilePrefab, shootPoint.position, shootPoint.rotation);

        //if (bullet.TryGetComponent(out Rigidbody rb))
        //{
        //    rb.linearVelocity = shootPoint.forward * gunStats.fireForce; // ← correct usage
        //}
        //else
        //{
        //    Debug.LogWarning("no Rigidbody on the bullet found");
        //}

        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit))
        {
            Debug.Log("Hit: " + hit.transform.gameObject.name);
        }
        else
        {
            Debug.Log("Missed");
        }

        lastFireTime = Time.time;
        OnFire?.Invoke();
    }


    public void SetStats(GunStats newStats) {
        gunStats = newStats;
    }
}

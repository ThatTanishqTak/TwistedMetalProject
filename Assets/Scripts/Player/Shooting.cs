using Unity.Netcode;
using UnityEngine;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class Shooting : NetworkBehaviour
{
   // [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform shootPoint;
   // [SerializeField] private float bulletSpeed;


    private void Update()
    {
        if (!IsOwner) { return; }

        //Shoot();
    }

    private void Shoot() 
    {
        if (Input.GetMouseButtonDown(0)) //Raycast hit and debug the object name.
        {
            RaycastHit hit;
            Physics.Raycast(shootPoint.position, transform.forward, out hit, 100f);
            Debug.DrawLine(shootPoint.position, hit.point, Color.red, 2f);
            // GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

            //Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();

            // Apply force to the bullet to make it move forward
            //bulletRb.linearVelocity = firePoint.forward * bulletSpeed;

            // Optional: Destroy the bullet after a certain time to clean up
            // Destroy(bullet, 3f);
        }
    }
}
using Unity.Netcode;
using UnityEngine;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class Shooting : NetworkBehaviour
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float bulletSpeed;

    private void Start()
    {
        //bullet = Resources.Load<GameObject>("SM_Bullet_03");
        if(bullet == null)
        {
            Debug.LogError("Bullet prefab not found! Make sure it's in the Resources folder.");
        }
        shootPoint = GetComponent<Transform>();
        Debug.Log("Start Called");
    }
    private void Update()
    {
        Shoot();
    }

    private void Shoot() 
    {
        if (Input.GetMouseButtonDown(0)) //Raycast hit and debug the object name.
        {
            RaycastHit hit;
            Vector3 shootDirection = shootPoint.forward;

            Debug.Log("Shoot Direction: " + shootDirection);
            InstantiateBullet(shootPoint.position, shootDirection);

            if (Physics.Raycast(shootPoint.position, shootDirection, out hit, 100f))
            {
                Debug.Log("Raycast hit calling Instantiate()");
                Debug.DrawRay(shootPoint.position, shootDirection * 100f, Color.green, 2f);
                Debug.Log("Hit: " + hit.transform.name);
            }
            else
            {
                Debug.DrawRay(shootPoint.position, shootDirection * 100f, Color.blue, 2f);
                Debug.Log("Missed");
            }
        }
    }
    private void InstantiateBullet(Vector3 hitPosition, Vector3 direction)
    {
        Debug.Log("Function Called");
        GameObject bulletPrefab = Instantiate(bullet, shootPoint.position, Quaternion.LookRotation(direction));
        bulletPrefab.transform.forward = direction;
        Rigidbody bulletRb = bulletPrefab.GetComponent<Rigidbody>();
        if(bulletRb != null)
        {
            //bulletRb.AddForce(direction * bulletSpeed, ForceMode.Impulse);
            bulletRb.AddForce(bulletPrefab.transform.forward * bulletSpeed, ForceMode.VelocityChange);
            Debug.Log("Bullet Instantiated");
        }
        else
        {
            Debug.Log("Rigidbody not found!");
        }
        Destroy(bulletPrefab, 3f);
    }
}
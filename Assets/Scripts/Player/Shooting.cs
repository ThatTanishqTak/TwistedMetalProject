using Unity.Netcode;
using UnityEngine;

public class Shooting : NetworkBehaviour
{
    [SerializeField] private GameObject bullet;

    public void Update()
    {
        if (!IsOwner) { return; }

        Shoot();
    }

    private void Shoot()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 direction = Vector3.forward;

            Instantiate(bullet);
            bullet.GetComponent<Rigidbody>().AddForce(direction, ForceMode.Impulse);
        }
    }
}
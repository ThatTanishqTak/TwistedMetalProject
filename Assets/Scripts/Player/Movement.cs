using Unity.Netcode;
using UnityEngine;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class Movement : NetworkBehaviour
{
    // Make sure you specify private, public and protected so that it's easy for both of us to understand
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private float speed = 15f;

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    // TODO: Functionality for the car's control
    // What I have done is for testing only (if possible use the new input system)
    private void Update()
    {
        //if (!IsOwner) { return; } // This make sure that a player cannot affect others
        if (carRigidbody == null)
        {
            Debug.Log("Car Rigidbody not found");
        }
        else
        {
            Debug.Log("Car Rigidbody found");
        }
        Debug.Log("Update called");
        MovePlayer();
    }

    private void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(vertical, 0, -horizontal) * speed;
        carRigidbody.MovePosition(carRigidbody.position + movement * speed * Time.deltaTime);
    }
}
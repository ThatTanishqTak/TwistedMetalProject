using Unity.Netcode;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    // Make sure you specify private, public and protected so that it's easy for both of us to understand
    [SerializeField] private CharacterController characterController;

    [SerializeField] private float speed;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // TODO: Functionality for the car's control
    // What I have done is for testing only (if possible use the new input system)
    private void Update()
    {
        if (!IsOwner) { return; } // This make sure that a player cannot affect others

        MovePlayer();
    }

    private void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = Time.deltaTime * speed * new Vector3(horizontal, 0.0f, vertical); // We have to use Runner.DeltaTime instead of Time.DeltaTime

        characterController.Move(movement);

        if (movement != Vector3.zero)
        {
            this.gameObject.transform.forward = movement;
        }
    }
}
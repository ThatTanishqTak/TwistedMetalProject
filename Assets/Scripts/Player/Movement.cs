using Fusion;
using UnityEngine;

public class Movement : NetworkBehaviour
{
    // Make sure you specify private, public and protected so that it's easy for both of us to understand
    [SerializeField] private CharacterController characterController;

    [SerializeField] private float Speed;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    // TODO: Functionality for the car's control
    // What I have done is for testing only (if possible use the new input system)
    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) { return; } // This make sure that a player cannot affect others

        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        Vector3 Movement = Runner.DeltaTime * Speed * new Vector3(Horizontal, 0.0f, Vertical); // We have to use Runner.DeltaTime instead of Time.DeltaTime

        characterController.Move(Movement);

        if (Movement != Vector3.zero)
        {
            this.gameObject.transform.forward = Movement;
        }
    }
}
using UnityEngine;

public class PlayerMovement_Test : MonoBehaviour
{
    private CharacterController characterController;
    private float moveSpeed = 2.0f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 move = new(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        characterController.Move(moveSpeed * Time.deltaTime * move);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }
    }
}
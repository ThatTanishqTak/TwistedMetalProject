using UnityEngine;

public class TornadoController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 60f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float rayHeight = 10f;
    [SerializeField] private float rayDistance = 20f;

    private void Update()
    {
        Vector3 origin = transform.position + Vector3.up * rayHeight;
        if (Physics.Raycast(origin, Vector3.down, out var hit, rayDistance, groundLayerMask))
        {
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}

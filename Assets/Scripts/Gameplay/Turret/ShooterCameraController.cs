using UnityEngine;

public class ShooterCameraController : MonoBehaviour
{
    [Header("Turret Rotation Targets")]
    [SerializeField] private Transform cannonBase; 
    [SerializeField] private Transform cannonHead; 

    [Header("Rotation Settings")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -10f;
    [SerializeField] private float maxVerticalAngle = 45f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yRotation += mouseX;
        cannonBase.localRotation = Quaternion.Euler(0f, yRotation, 0f);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);
        cannonHead.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}

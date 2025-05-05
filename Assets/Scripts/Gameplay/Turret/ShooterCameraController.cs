using UnityEngine;

public class ShooterCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cannonBase;
    [SerializeField] private Transform cannonHead;       
    [SerializeField] private Transform cameraPivot;      

    [Header("Rotation Settings")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float rotationSmoothSpeed = 5f;
    [SerializeField] private float minVerticalAngle = -10f;
    [SerializeField] private float maxVerticalAngle = 45f;

    private float yaw;  
    private float pitch; 

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        RotateTurret();
        UpdateCamera();
    }

    private void RotateTurret()
    {
        Quaternion baseTargetRotation = Quaternion.Euler(0f, yaw, 0f);
        cannonBase.localRotation = Quaternion.Slerp(cannonBase.localRotation, baseTargetRotation, rotationSmoothSpeed * Time.deltaTime);

        Quaternion headTargetRotation = Quaternion.Euler(pitch, 0f, 0f);
        cannonHead.localRotation = Quaternion.Slerp(cannonHead.localRotation, headTargetRotation, rotationSmoothSpeed * Time.deltaTime);
    }

    private void UpdateCamera()
    {
        if (cameraPivot != null)
        {
            cameraPivot.position = cannonHead.position;
            cameraPivot.rotation = cannonHead.rotation;
        }
    }
}

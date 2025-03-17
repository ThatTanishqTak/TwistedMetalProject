using UnityEngine;

public class RotateTurret : MonoBehaviour
{
    [SerializeField] private Transform turretBase; // Y-axis rotation (left/right)
    [SerializeField] private Transform gunTransform; // X-axis rotation (up/down)
    [SerializeField] private float rotationSpeed = 100f; // Adjust as needed
    [SerializeField] private float currentGunRotationX = 0f;
    [SerializeField] private float minGunAngle;
    [SerializeField] private float maxGunAngle;

    void Start()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        turretBase = GetComponent<Transform>();
        gunTransform = turretBase.Find("canon_lvl_1");

        if (turretBase == null)
        {
            turretBase = transform; // Assign self if not set
        }

        if (gunTransform == null)
        {
            Debug.LogError("Gun Transform not found! Check if it's named correctly.");
        }
        else
        {
            Debug.Log("Gun Transform found.");
        }
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        // Rotate base turret on Y-axis (horizontal rotation)
        if (mouseX != 0)
        {
            Quaternion rotationY = Quaternion.Euler(0, mouseX, 0);
            turretBase.rotation *= rotationY; // Rotate the correct transform
        }

        // Rotate gun on X-axis (vertical rotation)
        if (mouseY != 0 && gunTransform != null)
        {
            currentGunRotationX -= mouseY;
            currentGunRotationX = Mathf.Clamp(currentGunRotationX, minGunAngle, maxGunAngle);
       
            gunTransform.localRotation = Quaternion.Euler(currentGunRotationX, 0, 0);
        }
    }
}

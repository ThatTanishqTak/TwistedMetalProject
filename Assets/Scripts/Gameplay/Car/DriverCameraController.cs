using UnityEngine;

public class DriverCameraController : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 3f, -6f);

    private void LateUpdate()
    {
        if (followTarget == null) {
            return;
        } 

        Vector3 desiredPos = followTarget.position + followTarget.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, followTarget.rotation, followSpeed * Time.deltaTime);
    }
}

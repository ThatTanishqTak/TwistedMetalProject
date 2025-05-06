using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CarControllerWrapper))]
public class StormController : MonoBehaviour
{
    [Header("Storm Multipliers")]
    [Tooltip("Mass will become Mass × this")]
    [SerializeField, Range(0.1f, 2f)]
    private float massMultiplier = 0.5f;

    [Tooltip("Linear damping will become original × this")]
    [SerializeField, Range(1f, 5f)]
    private float linearDampingMultiplier = 2f;

    [Tooltip("Angular damping will become original × this")]
    [SerializeField, Range(1f, 5f)]
    private float angularDampingMultiplier = 2f;

    [Tooltip("Throttle input will be scaled by this during storm")]
    [SerializeField, Range(0f, 1f)]
    private float throttleMultiplier = 0.3f;

    [Tooltip("Steering input will be scaled by this during storm")]
    [SerializeField, Range(0f, 1f)]
    private float steeringMultiplier = 0.2f;

    private Rigidbody rb;
    private CarControllerWrapper wrapper;

    private float originalMass,
                  originalLinearDamping,
                  originalAngularDamping,
                  originalThrottle,
                  originalSteering;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        wrapper = GetComponent<CarControllerWrapper>();

        originalMass = rb.mass;
        originalLinearDamping = rb.linearDamping;
        originalAngularDamping = rb.angularDamping;
        originalThrottle = wrapper.ThrottleMultiplier;
        originalSteering = wrapper.SteeringMultiplier;
    }

    public static void StormBeganClientStatic()
    {
        var controllers = Object.FindObjectsByType<StormController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        foreach (var ctrl in controllers)
            ctrl.ApplyStorm();
    }

    public static void StormEndedClientStatic()
    {
        var controllers = Object.FindObjectsByType<StormController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        foreach (var ctrl in controllers)
            ctrl.EndStorm();
    }

    private void ApplyStorm()
    {
        rb.mass = originalMass * massMultiplier;
        rb.linearDamping = originalLinearDamping * linearDampingMultiplier;
        rb.angularDamping = originalAngularDamping * angularDampingMultiplier;
        wrapper.ThrottleMultiplier = throttleMultiplier;
        wrapper.SteeringMultiplier = steeringMultiplier;

        Debug.Log($"[StormController] STORM ON → mass={rb.mass:F1}, " +
                  $"linDamp={rb.linearDamping:F1}, angDamp={rb.angularDamping:F1}, " +
                  $"throttle×{throttleMultiplier:F2}, steer×{steeringMultiplier:F2}");
    }

    private void EndStorm()
    {
        rb.mass = originalMass;
        rb.linearDamping = originalLinearDamping;
        rb.angularDamping = originalAngularDamping;
        wrapper.ThrottleMultiplier = originalThrottle;
        wrapper.SteeringMultiplier = originalSteering;

        Debug.Log("[StormController] STORM OFF → restored defaults");
    }
}

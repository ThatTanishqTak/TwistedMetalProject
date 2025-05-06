using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody), typeof(CarControllerWrapper))]
public class StormController : MonoBehaviour
{
    [Header("Storm Multipliers")]
    [SerializeField, Range(0.1f, 2f)]
    private float massMultiplier = 0.8f;
    [SerializeField, Range(0f, 1f)]
    private float throttleMultiplier = 0.6f;
    [SerializeField, Range(0f, 1f)]
    private float steeringMultiplier = 0.7f;

    private Rigidbody rb;
    private CarControllerWrapper wrapper;
    private float originalMass, originalThrottle, originalSteering;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        wrapper = GetComponent<CarControllerWrapper>();

        originalMass = rb.mass;
        originalThrottle = wrapper.ThrottleMultiplier;
        originalSteering = wrapper.SteeringMultiplier;
    }

    public static void StormBeganClientStatic()
    {
        foreach (var ctrl in Object.FindObjectsByType<StormController>(
                    FindObjectsInactive.Exclude,      // whether to include inactive
                    FindObjectsSortMode.None         // how to sort the results
             ))
            ctrl.ApplyStorm();
    }

    public static void StormEndedClientStatic()
    {
        foreach (var ctrl in Object.FindObjectsByType<StormController>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
             ))
            ctrl.EndStorm();
    }

    private void ApplyStorm()
    {
        rb.mass = originalMass * massMultiplier;
        wrapper.ThrottleMultiplier = throttleMultiplier;
        wrapper.SteeringMultiplier = steeringMultiplier;
        Debug.Log($"[StormController] Storm ON: mass={rb.mass}, throttle×{throttleMultiplier}, steer×{steeringMultiplier}");
    }

    private void EndStorm()
    {
        rb.mass = originalMass;
        wrapper.ThrottleMultiplier = originalThrottle;
        wrapper.SteeringMultiplier = originalSteering;
        Debug.Log("[StormController] Storm OFF: restored defaults");
    }
}

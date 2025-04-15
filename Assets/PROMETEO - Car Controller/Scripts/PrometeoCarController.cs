// MODIFIED VERSION FOR MULTIPLAYER USE
// Cleaned up input handling, kept core driving and audio logic

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PrometeoCarController : MonoBehaviour
{
    [Header("Car Settings")]
    public int maxSpeed = 90;
    public int maxReverseSpeed = 45;
    public int accelerationMultiplier = 2;
    public int maxSteeringAngle = 27;
    public float steeringSpeed = 0.5f;
    public int brakeForce = 350;
    public int decelerationMultiplier = 2;
    public int handbrakeDriftMultiplier = 5;
    public Vector3 bodyMassCenter;

    [Header("Wheel References")]
    public GameObject frontLeftMesh;
    public WheelCollider frontLeftCollider;
    public GameObject frontRightMesh;
    public WheelCollider frontRightCollider;
    public GameObject rearLeftMesh;
    public WheelCollider rearLeftCollider;
    public GameObject rearRightMesh;
    public WheelCollider rearRightCollider;

    [Header("Particle Effects (Optional)")]
    public bool useEffects = false;
    public ParticleSystem RLWParticleSystem;
    public ParticleSystem RRWParticleSystem;
    public TrailRenderer RLWTireSkid;
    public TrailRenderer RRWTireSkid;

    [Header("Audio")]
    public bool useSounds = false;
    public AudioSource carEngineSound;
    public AudioSource tireScreechSound;
    private float initialCarEngineSoundPitch;

    [HideInInspector] public float carSpeed;
    [HideInInspector] public bool isDrifting;
    [HideInInspector] public bool isTractionLocked;

    private Rigidbody carRigidbody;
    private float steeringAxis;
    private float throttleAxis;
    private float driftingAxis;
    private float localVelocityZ;
    private float localVelocityX;
    private bool deceleratingCar;

    private WheelFrictionCurve FLwheelFriction;
    private float FLWextremumSlip;
    private WheelFrictionCurve FRwheelFriction;
    private float FRWextremumSlip;
    private WheelFrictionCurve RLwheelFriction;
    private float RLWextremumSlip;
    private WheelFrictionCurve RRwheelFriction;
    private float RRWextremumSlip;

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        SetupFrictionValues();

        if (useSounds && carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
            InvokeRepeating(nameof(CarSounds), 0f, 0.1f);
        }

        if (!useEffects)
        {
            StopAllEffects();
        }
    }

    private void Update()
    {
        carSpeed = (2 * Mathf.PI * frontLeftCollider.radius * frontLeftCollider.rpm * 60) / 1000;
        localVelocityX = transform.InverseTransformDirection(carRigidbody.linearVelocity).x;
        localVelocityZ = transform.InverseTransformDirection(carRigidbody.linearVelocity).z;

        AnimateWheelMeshes();
    }

    public void Move(float throttle, float steering)
    {
        HandleSteering(steering);

        if (throttle > 0.1f)
        {
            CancelInvoke(nameof(DecelerateCar));
            deceleratingCar = false;
            GoForward();
        }
        else if (throttle < -0.1f)
        {
            CancelInvoke(nameof(DecelerateCar));
            deceleratingCar = false;
            GoReverse();
        }
        else
        {
            if (!deceleratingCar)
            {
                InvokeRepeating(nameof(DecelerateCar), 0f, 0.1f);
                deceleratingCar = true;
            }
        }
    }

    public void ApplyHandbrake(bool active)
    {
        if (active)
        {
            CancelInvoke(nameof(DecelerateCar));
            Handbrake();
        }
        else
        {
            RecoverTraction();
        }
    }

    private void HandleSteering(float steerInput)
    {
        steeringAxis = Mathf.Clamp(steerInput, -1f, 1f);
        float targetAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, targetAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, targetAngle, steeringSpeed);
    }

    private void CarSounds()
    {
        if (!useSounds) return;

        if (carEngineSound != null)
        {
            float pitch = initialCarEngineSoundPitch + (Mathf.Abs(carRigidbody.linearVelocity.magnitude) / 25f);
            carEngineSound.pitch = pitch;
            if (!carEngineSound.isPlaying)
                carEngineSound.Play();
        }

        if ((isDrifting || isTractionLocked) && Mathf.Abs(carSpeed) > 12f)
        {
            if (!tireScreechSound.isPlaying)
                tireScreechSound.Play();
        }
        else if (tireScreechSound.isPlaying)
        {
            tireScreechSound.Stop();
        }
    }

    private void SetupFrictionValues() { /* ... */ }
    private void StopAllEffects() { /* ... */ }
    private void GoForward() { /* ... */ }
    private void GoReverse() { /* ... */ }
    private void DecelerateCar() { /* ... */ }
    private void Brakes() { /* ... */ }
    private void Handbrake() { /* ... */ }
    private void RecoverTraction() { /* ... */ }
    private void AnimateWheelMeshes() { /* ... */ }
    private void DriftCarPS() { /* ... */ }
}

// Input is still handled externally through CarControllerWrapper
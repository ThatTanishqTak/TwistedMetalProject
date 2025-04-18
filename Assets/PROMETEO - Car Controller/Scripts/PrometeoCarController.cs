using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PrometeoCarController : MonoBehaviour, ICarMovement
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
    private float localVelocityZ;
    private float localVelocityX;
    private bool deceleratingCar;

    private WheelFrictionCurve FLFriction, FRFriction, RLFriction, RRFriction;
    private float FLSlip, FRSlip, RLSlip, RRSlip;

    private void Start()
    {
        Debug.Log("Car Controller Initialized");

        carRigidbody = GetComponent<Rigidbody>();
        carRigidbody.centerOfMass = bodyMassCenter;

        SetupFrictionValues();

        if (useSounds && carEngineSound != null)
        {
            initialCarEngineSoundPitch = carEngineSound.pitch;
            InvokeRepeating(nameof(CarSounds), 0f, 0.1f);
        }

        if (!useEffects) StopAllEffects();
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
    private void HandleSteering(float steerInput)
    {
        steeringAxis = Mathf.Clamp(steerInput, -1f, 1f);
        float targetAngle = steeringAxis * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, targetAngle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, targetAngle, steeringSpeed);
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

    public void Steer(float value)
    {
        float angle = Mathf.Clamp(value, -1f, 1f) * maxSteeringAngle;
        frontLeftCollider.steerAngle = Mathf.Lerp(frontLeftCollider.steerAngle, angle, steeringSpeed);
        frontRightCollider.steerAngle = Mathf.Lerp(frontRightCollider.steerAngle, angle, steeringSpeed);
    }

    public void GoForward()
    {
        if (Mathf.RoundToInt(carSpeed) < maxSpeed)
        {
            float force = accelerationMultiplier * 50f;
            ApplyMotorTorque(force);
        }
        else
        {
            ApplyMotorTorque(0);
        }
    }

    public void GoReverse()
    {
        if (Mathf.Abs(Mathf.RoundToInt(carSpeed)) < maxReverseSpeed)
        {
            float force = -accelerationMultiplier * 50f;
            ApplyMotorTorque(force);
        }
        else
        {
            ApplyMotorTorque(0);
        }
    }

    public void Brakes()
    {
        ApplyBrakeTorque(brakeForce);
    }

    private void ApplyMotorTorque(float torque)
    {
        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;

        ApplyBrakeTorque(0);
    }

    private void ApplyBrakeTorque(float brake)
    {
        frontLeftCollider.brakeTorque = brake;
        frontRightCollider.brakeTorque = brake;
        rearLeftCollider.brakeTorque = brake;
        rearRightCollider.brakeTorque = brake;
    }

    private void DecelerateCar()
    {
        throttleAxis = Mathf.MoveTowards(throttleAxis, 0f, Time.deltaTime * 5f);
        carRigidbody.linearVelocity *= 1f / (1f + (0.025f * decelerationMultiplier));

        ApplyMotorTorque(0);

        if (carRigidbody.linearVelocity.magnitude < 0.25f)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            CancelInvoke(nameof(DecelerateCar));
        }
    }

    private void SetupFrictionValues()
    {
        FLFriction = frontLeftCollider.sidewaysFriction;
        FLSlip = FLFriction.extremumSlip;

        FRFriction = frontRightCollider.sidewaysFriction;
        FRSlip = FRFriction.extremumSlip;

        RLFriction = rearLeftCollider.sidewaysFriction;
        RLSlip = RLFriction.extremumSlip;

        RRFriction = rearRightCollider.sidewaysFriction;
        RRSlip = RRFriction.extremumSlip;
    }

    public void Handbrake()
    {
        isTractionLocked = true;
        ApplyFriction(handbrakeDriftMultiplier);
    }

    public void RecoverTraction()
    {
        isTractionLocked = false;
        ApplyFriction(1f);
    }

    private void ApplyFriction(float multiplier)
    {
        FLFriction.extremumSlip = FLSlip * multiplier;
        frontLeftCollider.sidewaysFriction = FLFriction;

        FRFriction.extremumSlip = FRSlip * multiplier;
        frontRightCollider.sidewaysFriction = FRFriction;

        RLFriction.extremumSlip = RLSlip * multiplier;
        rearLeftCollider.sidewaysFriction = RLFriction;

        RRFriction.extremumSlip = RRSlip * multiplier;
        rearRightCollider.sidewaysFriction = RRFriction;
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

    private void AnimateWheelMeshes()
    {
        frontLeftCollider.GetWorldPose(out var pos1, out var rot1);
        frontLeftMesh.transform.SetPositionAndRotation(pos1, rot1);

        frontRightCollider.GetWorldPose(out var pos2, out var rot2);
        frontRightMesh.transform.SetPositionAndRotation(pos2, rot2);

        rearLeftCollider.GetWorldPose(out var pos3, out var rot3);
        rearLeftMesh.transform.SetPositionAndRotation(pos3, rot3);

        rearRightCollider.GetWorldPose(out var pos4, out var rot4);
        rearRightMesh.transform.SetPositionAndRotation(pos4, rot4);
    }

    private void StopAllEffects()
    {
        RLWParticleSystem?.Stop();
        RRWParticleSystem?.Stop();
        if (RLWTireSkid != null) RLWTireSkid.emitting = false;
        if (RRWTireSkid != null) RRWTireSkid.emitting = false;
    }
}

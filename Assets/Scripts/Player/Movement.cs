using System.Linq;
using Unity.Netcode;
using UnityEditor.Rendering;
using UnityEngine;

/* --MAKE SURE WE REMOVE ALL THE DEBUG STATMENTS BEFORE WE BUILD THE FINAL VERSION-- */

public class Movement : NetworkBehaviour
{
    // Make sure you specify private, public and protected so that it's easy for both of us to understand
    [Header("Car Physics Settings")]
    [SerializeField] private WheelCollider[] wheelColliders = new WheelCollider[4];
    [SerializeField] private Transform[] tireMeshes = new Transform[4];
    [SerializeField] private Transform[] rimMeshes = new Transform[4];
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private Vector3 centerOfMass;

    [Space(5)]

    [Header("Car Movement Settings")]
    [SerializeField] private float motorSpeed = 1000f;
    [SerializeField] private float brakeTorque = 1500f;
    [SerializeField] private float lightBrakeTorque = 100f;
    [SerializeField] private float steerAngle = 30f;
    [SerializeField] private bool isBraking = false;
    [SerializeField] private float currentMotorTorque = 0;
    [SerializeField] private float accelerationSmoothness = 3f;

    [Space(5)]

    [Header("Custom Key Bindings")]
    [SerializeField] private KeyCode brakeKey = KeyCode.Space;

    private void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
        wheelColliders = GetComponentsInChildren<WheelCollider>();
        Transform[] meshesFound = GetComponentsInChildren<Transform>();
        carRigidbody.centerOfMass = centerOfMass;

        //Debug.Log("centre of mass" + centerOfMass);
        foreach (WheelCollider wheel in wheelColliders)
        {
            if (wheel.name.Contains("FL")) wheelColliders[0] = wheel;
            else if (wheel.name.Contains("FR")) wheelColliders[1] = wheel;
            else if (wheel.name.Contains("RL")) wheelColliders[2] = wheel;
            else if (wheel.name.Contains("RR")) wheelColliders[3] = wheel;

        }

        foreach (Transform mesh in meshesFound)
        {
            if (mesh.name.Contains("Tire"))
            {
                if (mesh.parent.name.Contains("FL")) tireMeshes[0] = mesh;
                else if (mesh.parent.name.Contains("FR")) tireMeshes[1] = mesh;
                else if (mesh.parent.name.Contains("RL")) tireMeshes[2] = mesh;
                else if (mesh.parent.name.Contains("RR")) tireMeshes[3] = mesh;
            }
            if (mesh.name.Contains("Rim"))
            {
                if (mesh.parent.name.Contains("FL")) rimMeshes[0] = mesh;
                else if (mesh.parent.name.Contains("FR")) rimMeshes[1] = mesh;
                else if (mesh.parent.name.Contains("RL")) rimMeshes[2] = mesh;
                else if (mesh.parent.name.Contains("RR")) rimMeshes[3] = mesh;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            Debug.Log($"Tire {i}: {tireMeshes[i]?.name}, Rim {i}: {rimMeshes[i]?.name}");
        }
    }
    // TODO: Functionality for the car's control
    // What I have done is for testing only (if possible use the new input system)
    private void Update()
    {
        isBraking = Input.GetKey(brakeKey);
        MovePlayer();
        UpdateWheels();
        ApplyBrake();
        ApplyDeceleration();

    }

    private void MovePlayer()
    {
        float steering = Input.GetAxis("Horizontal");
        float acceleration = Input.GetAxis("Vertical");
        currentMotorTorque = Mathf.Lerp(currentMotorTorque,acceleration*motorSpeed, Time.deltaTime * accelerationSmoothness);
        wheelColliders[2].motorTorque = currentMotorTorque;
        wheelColliders[3].motorTorque = currentMotorTorque;
        wheelColliders[0].steerAngle = steering * steerAngle;
        wheelColliders[1].steerAngle = steering * steerAngle;
    }
    private void UpdateWheels()
    {
        for(int i = 0; i < wheelColliders.Length; ++i)
        {
            Quaternion rot;
            Vector3 pos;
            wheelColliders[i].GetWorldPose(out pos, out rot);

            if(tireMeshes[i] != null)
            {
                tireMeshes[i].rotation = rot;
                tireMeshes[i].position = pos;
            }
            if (rimMeshes[i] != null)
            {
                rimMeshes[i].rotation = rot;
                rimMeshes[i].position = pos;
            }
        }
    }

    private void ApplyBrake()
    {
        for (int i = 0; i < wheelColliders.Length; ++i)
        {
            if (isBraking)
            {
                wheelColliders[i].brakeTorque = brakeTorque;
                //Debug.Log($"Braking {isBraking}");
            }
            else
            {
                wheelColliders[i].brakeTorque = 0;
                //Debug.Log($"not braking {isBraking}");
            }
        }
    }

    private void ApplyDeceleration()
    {
        if (Input.GetAxis("Vertical") == 0 && carRigidbody.angularVelocity.magnitude < 0.2f)
        {
            carRigidbody.angularVelocity = Vector3.zero;
            Debug.Log(carRigidbody.angularVelocity);
        }
        else if (Input.GetAxis("Vertical") == 0)
        {

            for (int i = 0; i < wheelColliders.Length; ++i)
            {
                wheelColliders[i].brakeTorque = Mathf.Lerp(wheelColliders[i].brakeTorque, 100f, Time.deltaTime * 2f);
                //Debug.Log($"Deceleration applied{carRigidbody.angularVelocity},{lightBrakeTorque}");
            }
        }
        else
        {
            for (int i = 0; i < wheelColliders.Length; ++i)
            {
                wheelColliders[i].brakeTorque = Mathf.Lerp(wheelColliders[i].brakeTorque, 0, Time.deltaTime * 2f);
                //Debug.Log($"Deceleration not applied{carRigidbody.angularVelocity}");
            }
        }
    }
}
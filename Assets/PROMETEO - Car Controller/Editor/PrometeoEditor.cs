using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PrometeoCarController))]
public class PrometeoEditor : Editor
{
    private SerializedObject SO;

    private SerializedProperty maxSpeed;
    private SerializedProperty maxReverseSpeed;
    private SerializedProperty accelerationMultiplier;
    private SerializedProperty maxSteeringAngle;
    private SerializedProperty steeringSpeed;
    private SerializedProperty brakeForce;
    private SerializedProperty decelerationMultiplier;
    private SerializedProperty handbrakeDriftMultiplier;
    private SerializedProperty bodyMassCenter;

    private SerializedProperty frontLeftMesh;
    private SerializedProperty frontLeftCollider;
    private SerializedProperty frontRightMesh;
    private SerializedProperty frontRightCollider;
    private SerializedProperty rearLeftMesh;
    private SerializedProperty rearLeftCollider;
    private SerializedProperty rearRightMesh;
    private SerializedProperty rearRightCollider;

    private SerializedProperty useEffects;
    private SerializedProperty RLWParticleSystem;
    private SerializedProperty RRWParticleSystem;
    private SerializedProperty RLWTireSkid;
    private SerializedProperty RRWTireSkid;

    private SerializedProperty useSounds;
    private SerializedProperty carEngineSound;
    private SerializedProperty tireScreechSound;

    private void OnEnable()
    {
        SO = new SerializedObject(target);

        maxSpeed = SO.FindProperty("maxSpeed");
        maxReverseSpeed = SO.FindProperty("maxReverseSpeed");
        accelerationMultiplier = SO.FindProperty("accelerationMultiplier");
        maxSteeringAngle = SO.FindProperty("maxSteeringAngle");
        steeringSpeed = SO.FindProperty("steeringSpeed");
        brakeForce = SO.FindProperty("brakeForce");
        decelerationMultiplier = SO.FindProperty("decelerationMultiplier");
        handbrakeDriftMultiplier = SO.FindProperty("handbrakeDriftMultiplier");
        bodyMassCenter = SO.FindProperty("bodyMassCenter");

        frontLeftMesh = SO.FindProperty("frontLeftMesh");
        frontLeftCollider = SO.FindProperty("frontLeftCollider");
        frontRightMesh = SO.FindProperty("frontRightMesh");
        frontRightCollider = SO.FindProperty("frontRightCollider");
        rearLeftMesh = SO.FindProperty("rearLeftMesh");
        rearLeftCollider = SO.FindProperty("rearLeftCollider");
        rearRightMesh = SO.FindProperty("rearRightMesh");
        rearRightCollider = SO.FindProperty("rearRightCollider");

        useEffects = SO.FindProperty("useEffects");
        RLWParticleSystem = SO.FindProperty("RLWParticleSystem");
        RRWParticleSystem = SO.FindProperty("RRWParticleSystem");
        RLWTireSkid = SO.FindProperty("RLWTireSkid");
        RRWTireSkid = SO.FindProperty("RRWTireSkid");

        useSounds = SO.FindProperty("useSounds");
        carEngineSound = SO.FindProperty("carEngineSound");
        tireScreechSound = SO.FindProperty("tireScreechSound");
    }

    public override void OnInspectorGUI()
    {
        SO.Update();

        GUILayout.Space(10);
        GUILayout.Label("Car Configuration", EditorStyles.boldLabel);

        maxSpeed.intValue = EditorGUILayout.IntSlider("Max Speed", maxSpeed.intValue, 20, 190);
        maxReverseSpeed.intValue = EditorGUILayout.IntSlider("Max Reverse Speed", maxReverseSpeed.intValue, 10, 120);
        accelerationMultiplier.intValue = EditorGUILayout.IntSlider("Acceleration Multiplier", accelerationMultiplier.intValue, 1, 10);
        maxSteeringAngle.intValue = EditorGUILayout.IntSlider("Max Steering Angle", maxSteeringAngle.intValue, 10, 45);
        steeringSpeed.floatValue = EditorGUILayout.Slider("Steering Speed", steeringSpeed.floatValue, 0.1f, 1f);
        brakeForce.intValue = EditorGUILayout.IntSlider("Brake Force", brakeForce.intValue, 100, 600);
        decelerationMultiplier.intValue = EditorGUILayout.IntSlider("Deceleration Multiplier", decelerationMultiplier.intValue, 1, 10);
        handbrakeDriftMultiplier.intValue = EditorGUILayout.IntSlider("Drift Multiplier", handbrakeDriftMultiplier.intValue, 1, 10);
        EditorGUILayout.PropertyField(bodyMassCenter, new GUIContent("Body Mass Center"));

        GUILayout.Space(10);
        GUILayout.Label("Wheels", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(frontLeftMesh);
        EditorGUILayout.PropertyField(frontLeftCollider);
        EditorGUILayout.PropertyField(frontRightMesh);
        EditorGUILayout.PropertyField(frontRightCollider);
        EditorGUILayout.PropertyField(rearLeftMesh);
        EditorGUILayout.PropertyField(rearLeftCollider);
        EditorGUILayout.PropertyField(rearRightMesh);
        EditorGUILayout.PropertyField(rearRightCollider);

        GUILayout.Space(10);
        GUILayout.Label("Effects", EditorStyles.boldLabel);

        useEffects.boolValue = EditorGUILayout.Toggle("Use Effects", useEffects.boolValue);
        if (useEffects.boolValue)
        {
            EditorGUILayout.PropertyField(RLWParticleSystem);
            EditorGUILayout.PropertyField(RRWParticleSystem);
            EditorGUILayout.PropertyField(RLWTireSkid);
            EditorGUILayout.PropertyField(RRWTireSkid);
        }

        GUILayout.Space(10);
        GUILayout.Label("Sounds", EditorStyles.boldLabel);

        useSounds.boolValue = EditorGUILayout.Toggle("Use Sounds", useSounds.boolValue);
        if (useSounds.boolValue)
        {
            EditorGUILayout.PropertyField(carEngineSound);
            EditorGUILayout.PropertyField(tireScreechSound);
        }

        SO.ApplyModifiedProperties();
    }
}
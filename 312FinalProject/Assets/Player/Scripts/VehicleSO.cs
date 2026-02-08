using UnityEngine;

[CreateAssetMenu(fileName = "New VehicleSO", menuName = "ScriptableObjects/VehicleSO")]
public class VehicleSO : ScriptableObject
{
    [Header("Movement Controls")]
    public float acceleration = 25f;
    public float maxSpeed = 100f;
    public float steerStrength = 15f;
    public AnimationCurve turningCurve;
    public float dragCoefficient = 1f;
    public float minSideSkidVelocity = 10f;

    [Header("Wheel Settings")]
    public float springStiffness;
    public float damperStiffness;
    public float restLength;
    public float springTravel;
    public float wheelRadius;
    public float tireRotationSpeed = 3000f;
    public float maxSteeringAngle = 30f;
}

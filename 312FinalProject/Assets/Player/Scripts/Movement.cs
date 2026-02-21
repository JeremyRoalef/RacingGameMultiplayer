using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : NetworkBehaviour
{
    public Action<float> OnMove;
    public Action OnSkidStart;
    public Action OnSkidStop;

    [SerializeField] Rigidbody carRB;
    [SerializeField] VehicleInput vehicleInput;
    [SerializeField] Vehicle vehicle;
    [SerializeField] Transform accelerationPoint;

    public float CarVelocityRatio {  get; private set; }
   
    Vector3 currentCarLocalVelocity = Vector3.zero;
    bool isSkidding = false;

    void Awake()
    {
        // Initialize RB
        carRB = GetComponent<Rigidbody>();
        carRB.isKinematic = true;
    }

    void FixedUpdate()
    {
        //Only server & owner can control vehicle
        if (IsServer)
        {
            CalculateCarVelocity();
            Move();
            HandleSkidding();
        }
    }

    // Calculate car velocity as the ration between it's current forward speed and maximum speed.
    private void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.linearVelocity);
        CarVelocityRatio = currentCarLocalVelocity.z / vehicle.VehicleSettings.maxSpeed;
    }

    // Handle movement
    private void Move()
    {
        if (!vehicle.IsGrounded) return;

        //Apply acceleration
        carRB.AddForceAtPosition(
            vehicle.VehicleSettings.acceleration * vehicleInput.Movement.y * transform.forward, 
            accelerationPoint.position, 
            ForceMode.Acceleration);

        //Apply torque
        carRB.AddTorque(
            vehicle.VehicleSettings.steerStrength * vehicleInput.Movement.x * vehicle.VehicleSettings.turningCurve.Evaluate(CarVelocityRatio) * Mathf.Sign(CarVelocityRatio) * transform.up, 
            ForceMode.Acceleration
            );

        //Apply side drag to determine how "slippy" the tires are. 
        float currentSidewaysSpeed = currentCarLocalVelocity.x;
        float dragMagnitude = -currentSidewaysSpeed * vehicle.VehicleSettings.dragCoefficient;
        Vector3 dragForce = transform.right * dragMagnitude;

        carRB.AddForceAtPosition(dragForce, carRB.worldCenterOfMass, ForceMode.Acceleration);
        OnMoveRpc(carRB.linearVelocity.sqrMagnitude);
    }

    private void HandleSkidding()
    {
        // If we're grounded and our car has sideways momentum "skidding"
        if (vehicle.IsGrounded && Mathf.Abs(currentCarLocalVelocity.x) > vehicle.VehicleSettings.minSideSkidVelocity)
        {
            if (!isSkidding)
            {
                OnSkidStartRpc();
            }
            isSkidding = true;
        }
        else if (isSkidding)
        {
            OnSkidStopRpc();
            isSkidding = false;
        }
    }

    public void ApplyForceAtPosition(Vector3 forceAmount, Vector3 position)
    => carRB.AddForceAtPosition(forceAmount, position);
    public Vector3 GetPointVelocity(Vector3 pos) => carRB.GetPointVelocity(pos);

    [Rpc(SendTo.Everyone)]
    void OnMoveRpc(float moveValue)
    {
        CalculateCarVelocity();
        OnMove?.Invoke(moveValue);
    }

    [Rpc(SendTo.Everyone)]
    void OnSkidStartRpc()
    {
        OnSkidStart?.Invoke();
    }
    
    [Rpc(SendTo.Everyone)]
    void OnSkidStopRpc()
    {
        OnSkidStop?.Invoke();
    }
}

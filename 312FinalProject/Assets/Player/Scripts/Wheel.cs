using System;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    public Action<Wheel> OnWheelGrounded;
    public Action<Wheel> OnWheelUngrounded;

    [SerializeField] Movement movement;
    [SerializeField] VehicleInput vehicleInput;
    [SerializeField] Vehicle vehicle;
    [SerializeField] GameObject tire;
    [SerializeField] Transform suspensionRayPoint;
    [SerializeField] LayerMask driveableLayerMask;
    [SerializeField] bool isFrontTire;

    public bool IsGrounded { get; private set; }

    private void Update()
    {
        RaycastHit hit;
        float maxLength = vehicle.VehicleSettings.restLength + vehicle.VehicleSettings.springTravel;

        if (!Physics.Raycast(
            suspensionRayPoint.position, 
            -suspensionRayPoint.up, 
            out hit, 
            maxLength + vehicle.VehicleSettings.wheelRadius, 
            driveableLayerMask)
            )
        {
            if (IsGrounded)
            {
                //Wheel isn't grounded anymore
                IsGrounded = false;
                OnWheelUngrounded?.Invoke(this);
            }

            SetTirePosition(tire, suspensionRayPoint.position - suspensionRayPoint.up * maxLength);

            Debug.DrawLine(
                suspensionRayPoint.position, 
                suspensionRayPoint.position + (vehicle.VehicleSettings.wheelRadius + maxLength) * -suspensionRayPoint.up, 
                Color.green
                );
            return;
        }

        if (!IsGrounded)
        {
            //Wheel is now grounded
            IsGrounded = true;
            OnWheelGrounded?.Invoke(this);
        }

        HandleSuspension(hit);
        UpdateWheelVisual();
    }

    private void UpdateWheelVisual()
    {
        float steeringAngle = vehicle.VehicleSettings.maxSteeringAngle * vehicleInput.Movement.x;

        // for the front tires we rotate forward/backwards and rotate on y-axis for steering
        if (isFrontTire)
        {
            tire.transform.Rotate(Vector3.right, vehicle.VehicleSettings.tireRotationSpeed * movement.CarVelocityRatio * Time.deltaTime, Space.Self);

            tire.transform.localEulerAngles = new Vector3(
                tire.transform.localEulerAngles.x, 
                steeringAngle, 
                tire.transform.localEulerAngles.z
                );
        }
        // rear tires just rotate forward and backwards.
        else
        {
            tire.transform.Rotate(
                Vector3.right, 
                vehicle.VehicleSettings.tireRotationSpeed * vehicleInput.Movement.y * Time.deltaTime, 
                Space.Self
                );
        }
    }

    private void HandleSuspension(RaycastHit hit)
    {
        Vector3 springDir = suspensionRayPoint.up;   // direction of the spring

        // Effective suspension length (pivot to wheel contact)
        float currentLength = hit.distance - vehicle.VehicleSettings.wheelRadius;

        // Extension relative to rest
        float displacement = vehicle.VehicleSettings.restLength - currentLength;

        // Velocity along the spring direction
        float vel = Vector3.Dot(movement.GetPointVelocity(suspensionRayPoint.position), springDir);

        // Hooke’s law + damping
        float force = (vehicle.VehicleSettings.springStiffness * displacement) - (vehicle.VehicleSettings.damperStiffness * vel);

        // Clamp so suspension doesn't "pull" downward
        if (force < 0) force = 0;

        // Add the calculated spring force
        movement.ApplyForceAtPosition(springDir * force, suspensionRayPoint.position);

        // Adjust the visual tire position
        SetTirePosition(tire, hit.point + suspensionRayPoint.up * vehicle.VehicleSettings.wheelRadius);

        Debug.DrawLine(suspensionRayPoint.position, hit.point, Color.red);
    }

    // Adjust tire visuals
    private void SetTirePosition(GameObject tire, Vector3 targetPosition)
    {
        tire.transform.position = targetPosition;
    }
}

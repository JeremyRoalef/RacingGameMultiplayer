using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Vehicle : NetworkBehaviour
{
    [SerializeField]
    InputActionReference restartAtCheckpoint;

    [SerializeField]
    Transform cameraFollowLookAtTarget;

    public static List<Vehicle> Instances = new List<Vehicle>();

    [SerializeField] VehicleSO vehicleSettings;
    [SerializeField] Wheel[] wheels;

    [SerializeField] BoxCollider vehicleCollider;
    [SerializeField] Transform[] parentRendererAndFX;
    [SerializeField] Rigidbody rb;

    Vector3 startingPos;
    Quaternion startingRotation;

    public bool IsGrounded { get { return groundedWheels.Count >= 2; } }
    public VehicleSO VehicleSettings { get { return vehicleSettings; } }

    List<Wheel> groundedWheels = new List<Wheel>();

    private void OnEnable()
    {
        Instances.Add(this);

        //Subscribe for wheel grounded check
        foreach (Wheel wheel in wheels)
        {
            wheel.OnWheelGrounded += HandleWheelGrounded;
            wheel.OnWheelUngrounded += HandleWheelUngrounded;
        }

        if (vehicleSettings == null)
        {
            Debug.LogError("Null vehicle settings");
        }

        RaceManager.Instance.OnClientFinishedRace += HandleThisClientFinishedRace;
        restartAtCheckpoint.action.performed += RestartAtCheckpoint;

        startingPos = transform.position;
        startingRotation = transform.rotation;
    }

    private void RestartAtCheckpoint(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        Debug.Log("Owner wants to restart at checkpoint");

        //Reset rigidbody velocity and rotation
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();

        //Restart the owner at the last checkpoint they reached (set their position to the last checkpoint)
        Transform lastCheckpointTransform = CheckpointManager.Instance.GetCurrentCheckpoint().transform;
        if (lastCheckpointTransform != null)
        {
            rb.position = lastCheckpointTransform.position;
            rb.rotation = lastCheckpointTransform.rotation;
        }
        else
        {
            rb.position = startingPos;
            rb.rotation = startingRotation;
        }

        rb.WakeUp();
    }

    private void HandleThisClientFinishedRace(ulong clientID)
    {
        if (OwnerClientId != clientID) return;
        
        //The vehicle is no longer needed; Hide & disable collision
        vehicleCollider.enabled = false;
        foreach(Transform rendererOrFX in parentRendererAndFX)
        {
            rendererOrFX.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        Instances.Remove(this);

        //Unsubscribe from wheel grounded check
        foreach (Wheel wheel in wheels)
        {
            wheel.OnWheelGrounded -= HandleWheelGrounded;
            wheel.OnWheelUngrounded -= HandleWheelUngrounded;
        }
    }

    public ulong GetOwnerClientID()
    {
        return OwnerClientId;
    }

    public Transform GetCameraFollowLookAtTransform()
    {
        return cameraFollowLookAtTarget;
    }

    private void HandleWheelUngrounded(Wheel wheel)
    {
        groundedWheels.Remove(wheel);
    }

    private void HandleWheelGrounded(Wheel wheel)
    {
        if (groundedWheels.Contains(wheel)) return;
        groundedWheels.Add(wheel);
    }

    [Rpc(SendTo.Server)]
    public void UpdateCheckpointServerRpc(int checkpointIndex)
    {
        //Tell the server that the client has hit a new checkpoint
        RaceManager.Instance.HandleClientHitCheckpoint(OwnerClientId, checkpointIndex);
    }
}

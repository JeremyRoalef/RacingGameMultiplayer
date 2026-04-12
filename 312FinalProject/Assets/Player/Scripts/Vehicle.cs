using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Vehicle : NetworkBehaviour
{
    public static List<Vehicle> Instances = new List<Vehicle>();

    [Header("Input References")]
    [SerializeField]
    InputActionReference restartAtCheckpoint;

    [Header("Object/Component References")]
    [SerializeField] Transform cameraFollowLookAtTarget;

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
        //Check for vehicle settings
        if (vehicleSettings == null)
        {
            Debug.LogError("Null vehicle settings");
        }

        //New vehicle added to the scene
        Instances.Add(this);

        startingPos = transform.position;
        startingRotation = transform.rotation;


        //Subscribe to events
        foreach (Wheel wheel in wheels)
        {
            wheel.OnWheelGrounded += HandleWheelGrounded;
            wheel.OnWheelUngrounded += HandleWheelUngrounded;
        }

        RaceManager.Instance.OnClientFinishedRace += HandleThisClientFinishedRace;
        restartAtCheckpoint.action.performed += RestartAtCheckpoint;
    }

    private void OnDisable()
    {
        //Vehicle has been removed from the game
        Instances.Remove(this);

        //Unsubscribe from events
        foreach (Wheel wheel in wheels)
        {
            wheel.OnWheelGrounded -= HandleWheelGrounded;
            wheel.OnWheelUngrounded -= HandleWheelUngrounded;
        }

        if (RaceManager.Instance != null)
        {
            RaceManager.Instance.OnClientFinishedRace -= HandleThisClientFinishedRace;
        }
        restartAtCheckpoint.action.performed -= RestartAtCheckpoint;
    }

    public ulong GetOwnerClientID() => OwnerClientId;

    public Transform GetCameraFollowLookAtTransform() => cameraFollowLookAtTarget;

    private void RestartAtCheckpoint(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        //Debug.Log("Owner wants to restart at checkpoint");

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
        //Check if this player finished the race
        if (OwnerClientId != clientID) return;

        //The vehicle is no longer needed; Hide & disable collision
        vehicleCollider.enabled = false;
        foreach (Transform rendererOrFX in parentRendererAndFX)
        {
            rendererOrFX.gameObject.SetActive(false);
        }
    }

    private void HandleWheelUngrounded(Wheel wheel) => groundedWheels.Remove(wheel);

    private void HandleWheelGrounded(Wheel wheel)
    {
        //Don't add the wheel if it was already added to the ground
        if (groundedWheels.Contains(wheel)) return;
        groundedWheels.Add(wheel);
    }

    [Rpc(SendTo.Server)]
    public void UpdateCheckpointServerRpc(int checkpointIndex) => RaceManager.Instance.HandleClientHitCheckpoint(OwnerClientId, checkpointIndex);
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Vehicle : NetworkBehaviour
{
    [SerializeField]
    Transform cameraFollowLookAtTarget;

    public static List<Vehicle> Instances = new List<Vehicle>();

    [SerializeField] VehicleSO vehicleSettings;
    [SerializeField] Wheel[] wheels;

    [SerializeField] BoxCollider vehicleCollider;
    [SerializeField] Transform[] parentRendererAndFX;

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

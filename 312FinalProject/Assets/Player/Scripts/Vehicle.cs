using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Vehicle : NetworkBehaviour
{
    [SerializeField] VehicleSO vehicleSettings;
    [SerializeField] Wheel[] wheels;

    public bool IsGrounded { get { return groundedWheels.Count >= 2; } }
    public VehicleSO VehicleSettings { get { return vehicleSettings; } }

    List<Wheel> groundedWheels = new List<Wheel>();
    private void OnEnable()
    {
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
    }

    private void OnDisable()
    {
        //Unsubscribe from wheel grounded check
        foreach (Wheel wheel in wheels)
        {
            wheel.OnWheelGrounded -= HandleWheelGrounded;
            wheel.OnWheelUngrounded -= HandleWheelUngrounded;
        }
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

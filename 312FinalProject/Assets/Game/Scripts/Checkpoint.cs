using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Action<Checkpoint, Vehicle> OnPlayerTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Vehicle>(out Vehicle vehicle))
        {
            if (vehicle.OwnerClientId != NetworkManager.Singleton.LocalClientId) return;

            //The client has hit the new checkpoint
            OnPlayerTriggerEnter?.Invoke(this, vehicle);
        }
    }
}

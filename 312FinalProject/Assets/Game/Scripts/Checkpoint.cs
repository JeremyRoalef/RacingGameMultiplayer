using System;
using Unity.Netcode;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Action<Checkpoint, Vehicle> OnPlayerTriggerEnter;

    private void OnTriggerEnter(Collider other)
    {
        //Check if a client's vehicle entered this collider
        if (other.TryGetComponent<Vehicle>(out Vehicle vehicle))
        {
            //Only the owner of this machine can invoke collisions
            if (vehicle.OwnerClientId != NetworkManager.Singleton.LocalClientId) return;

            //The client has hit the new checkpoint
            OnPlayerTriggerEnter?.Invoke(this, vehicle);
        }
    }
}

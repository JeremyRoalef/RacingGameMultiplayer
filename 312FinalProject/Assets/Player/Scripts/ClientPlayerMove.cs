using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField]
    PlayerInput playerInput;

    [SerializeField]
    VehicleInput vehicleInput;

    [SerializeField]
    Rigidbody rb;

    private void Awake()
    {
        playerInput.enabled = false;
        vehicleInput.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            //Allow the owner to send input to their vehicle
            playerInput.enabled = true;
            vehicleInput.enabled = true;
        }
        else
        {
            //Remove player input scripts from scene if owner doesn't own it
            Destroy(playerInput);
        }
    }
}

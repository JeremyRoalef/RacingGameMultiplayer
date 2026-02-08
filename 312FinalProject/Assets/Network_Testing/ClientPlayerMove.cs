using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField]
    PlayerInput playerInput;

    [SerializeField]
    Movement movement;

    [SerializeField]
    VehicleInput vehicleInput;

    private void Awake()
    {
        playerInput.enabled = false;
        movement.enabled = false;
        vehicleInput.enabled = false;
    }

    private void LateUpdate()
    {
        if (!IsOwner) return;

        //Pass player input to the server
        UpdateInputServerRpc(vehicleInput.Movement);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            playerInput.enabled = true;
            movement.enabled = true;
        }
        if (IsServer)
        {
            movement.enabled = true;
        }
    }

    [Rpc(SendTo.Server)]
    void UpdateInputServerRpc(Vector2 move)
    {
        //Update input server-side so the player can move their character
        vehicleInput.Movement = move;
    }
}

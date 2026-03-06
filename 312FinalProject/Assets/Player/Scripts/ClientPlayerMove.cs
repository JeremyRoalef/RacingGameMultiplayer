using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClientPlayerMove : NetworkBehaviour
{
    [SerializeField]
    PlayerInput playerInput;

    [SerializeField]
    VehicleInput vehicleInput;

    private void Awake()
    {
        playerInput.enabled = false;
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
            vehicleInput.enabled = true;
        }
    }

    [Rpc(SendTo.Server)]
    void UpdateInputServerRpc(Vector2 move)
    {
        //Update input server-side so the player can move their character
        vehicleInput.Movement = move;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class VehicleInput : MonoBehaviour
{
    //Note: input will be enabled client end. While the movmenet script will look for movement, movement is inhereintly disbaled
    //client end. Thus, movement will only occur on server-end. No duplate movement input.

    [SerializeField]
    InputActionReference moveInput;

    private void Awake()
    {
        moveInput.action.performed += HandleMovementPerformed;
        moveInput.action.canceled += HandleMovementPerformed;
    }

    private Vector2 movement;
    public Vector2 Movement
    {
        get { return movement; }
        set { movement = value; }
    }

    private void HandleMovementPerformed(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }
}

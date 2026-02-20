using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraSetup : NetworkBehaviour
{
    [SerializeField]
    Transform cameraFollowTarget;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;

        //Setup camera
        CinemachineCamera cinemachineCamera = Object.FindFirstObjectByType<CinemachineCamera>();

        //Lookat parameters
        cinemachineCamera.Follow = cameraFollowTarget;
        cinemachineCamera.LookAt = cameraFollowTarget;

        //Handle player input controls
        CinemachineInputAxisController input = cinemachineCamera.GetComponent<CinemachineInputAxisController>();
        input.enabled = true;
    }
}

using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraSetup : NetworkBehaviour
{
    [SerializeField]
    Vehicle ownerVehicle;

    CinemachineCamera cinemachineCamera;
    ulong clientBeingFollowed;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;

        //Setup camera
        cinemachineCamera = Object.FindFirstObjectByType<CinemachineCamera>();

        //Lookat parameters
        cinemachineCamera.Follow = ownerVehicle.GetCameraFollowLookAtTransform();
        cinemachineCamera.LookAt = ownerVehicle.GetCameraFollowLookAtTransform();

        //Handle player input controls
        CinemachineInputAxisController input = cinemachineCamera.GetComponent<CinemachineInputAxisController>();
        input.enabled = true;

        //Set the client being followed to the owner
        clientBeingFollowed = OwnerClientId;

        RaceManager.Instance.OnClientFinishedRace += HandleClientFinishedRace;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsOwner) return;

        RaceManager.Instance.OnClientFinishedRace -= HandleClientFinishedRace;
    }

    private void HandleClientFinishedRace(ulong clientID)
    {
        //Only the owner object on the local session should run this
        if (!IsOwner) return;

        //If the client being followed by this camera was the one to finish the race, then get a new follow target
        if (clientBeingFollowed == clientID)
        {
            FindNewFollowTarget();
        }
    }

    private void FindNewFollowTarget()
    {
        bool foundNewFollowTarget = false;

        //Find another player in the game to watch while waiting for the game to finish
        foreach (Vehicle clientVehicle in Vehicle.Instances)
        {
            //If the client who owns the vehicle hasn't finished the race, spectate them
            if (!RaceManager.Instance.clientsWhoFinishedRace.Contains(clientVehicle.GetOwnerClientID()))
            {
                cinemachineCamera.Follow = clientVehicle.GetCameraFollowLookAtTransform();
                cinemachineCamera.LookAt = clientVehicle.GetCameraFollowLookAtTransform();
                clientBeingFollowed = clientVehicle.GetOwnerClientID();
                foundNewFollowTarget = true;
            }
        }
        
        if (!foundNewFollowTarget)
        {
            cinemachineCamera.Follow = null;
            cinemachineCamera.LookAt = null;
        }
    }
}

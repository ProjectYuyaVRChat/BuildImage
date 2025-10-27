using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;

public class Respawn : UdonSharpBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform respawnPoint2;
    public PassMidPoint mid;

    [SerializeField] private float respawnRotateY;
    private Quaternion startRespawnRotation;
    private Quaternion midRespawnRotation;

    private void Start()
    {
        startRespawnRotation = Quaternion.Euler(
            respawnPoint.rotation.eulerAngles.x,
            respawnRotateY, 
            respawnPoint.rotation.eulerAngles.z
        );
        
        midRespawnRotation = Quaternion.Euler(
            respawnPoint2.rotation.eulerAngles.x,
            respawnRotateY, 
            respawnPoint2.rotation.eulerAngles.z
        );
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log("passssssssssssss");
        if (!mid.isOn)
        {
            player.TeleportTo(respawnPoint.position, startRespawnRotation);
        }
        else
        {
            player.TeleportTo(respawnPoint2.position, midRespawnRotation);
        }
    }
}

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

    [SerializeField] private float respawnRotateY = 0;
    private Quaternion startRespawnRotation;
    private Quaternion midRespawnRotation;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Debug.Log("passssssssssssss");
        if (!mid.isOn)
        {
            player.TeleportTo(respawnPoint.position, Quaternion.Euler(0, respawnRotateY, 0));
        }
        else
        {
            player.TeleportTo(respawnPoint2.position, Quaternion.Euler(0, respawnRotateY, 0));
        }
    }
}

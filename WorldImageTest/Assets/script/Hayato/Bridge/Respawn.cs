using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Respawn : UdonSharpBehaviour
{
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform respawnPoint2;
    public PassMidPoint mid;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (!mid.isOn)
        {
            player.TeleportTo(respawnPoint.position, respawnPoint.rotation);
        }
        else
        {
            player.TeleportTo(respawnPoint2.position, respawnPoint2.rotation);
        }
    }
}

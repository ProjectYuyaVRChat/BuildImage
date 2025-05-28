using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Respawn : UdonSharpBehaviour
{
    [SerializeField] private Transform respawnPoint;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        Networking.LocalPlayer.TeleportTo(respawnPoint.position, respawnPoint.rotation);
    }
}

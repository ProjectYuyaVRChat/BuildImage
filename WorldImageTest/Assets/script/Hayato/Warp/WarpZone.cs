using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WarpZone : UdonSharpBehaviour
{
    [Header("ワープ先")]
    [SerializeField] private Transform warpPoint;
    private Quaternion PlayerRotate;
    [SerializeField] private float warpYQuaternion = 180f;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player != null && player.isLocal)
        {
            player.TeleportTo(
                warpPoint.transform.position,
                Quaternion.Euler(0, warpYQuaternion, 0)
            );
        }
    }
}
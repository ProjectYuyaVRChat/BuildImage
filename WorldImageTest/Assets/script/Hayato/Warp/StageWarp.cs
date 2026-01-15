using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class StageWarp : UdonSharpBehaviour
{
    [UdonSynced]
    private int interactingPlayerId = -1;
    [SerializeField] private GameObject warpPoint;
    [SerializeField] private float warpYQuaternion = 180f;
    private bool isSequenceRunning = false;

    public override void Interact()
    {
        
        // このオブジェクトのオーナーを自分自身に設定し、同期変数を変更する権限を得る
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        
        this.interactingPlayerId = Networking.LocalPlayer.playerId;
        
        Warp();
    }
    
    public void Warp()
    {
        VRCPlayerApi playerToWarp = VRCPlayerApi.GetPlayerById(interactingPlayerId);
        if (playerToWarp != null && playerToWarp.isLocal)
        {
            playerToWarp.TeleportTo(
                warpPoint.transform.position,
                Quaternion.Euler(0, warpYQuaternion, 0)
            );
        }
    }

}

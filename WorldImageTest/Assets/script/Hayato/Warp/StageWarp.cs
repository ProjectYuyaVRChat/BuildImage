using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class StageWarp : UdonSharpBehaviour
{
    [UdonSynced]
    public bool isOn = false;
    [UdonSynced]
    private int interactingPlayerId = -1;
    [SerializeField] private GameObject warpPoint;
    [SerializeField] private float warpYQuaternion = 180f;
    private bool isSequenceRunning = false;

    public override void Interact()
    {
        if (isOn) return;
        
        // このオブジェクトのオーナーを自分自身に設定し、同期変数を変更する権限を得る
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        
        // 状態を更新
        this.isOn = true;
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

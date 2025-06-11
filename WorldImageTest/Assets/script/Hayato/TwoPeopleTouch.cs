using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class TwoPeopleTouch : UdonSharpBehaviour
{
    [SerializeField] private WarpBlock PastWarpBlock;
    [SerializeField] private WarpBlock FutureWarpBlock;
    
    private bool isSequenceActive = false;

    void Start()
    {
        if (PastWarpBlock == null)
        {
            Debug.LogWarning("PastWarpBlockが入ってない");
        }

        if (FutureWarpBlock == null)
        {
            Debug.LogWarning("FutureWarpBlockが入ってない");
        }
    }
    
    void Update()
    {
        if (isSequenceActive)
        {
            return;
        }
        
        if (PastWarpBlock.isOn && FutureWarpBlock.isOn)
        {
            isSequenceActive = true; // 即座に再判定されないようにロックする
            
            // 全プレイヤーに対して「ワープを開始せよ」という命令を送る
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(TriggerAllClients));

            // --- 状態のリセット ---
            // 3秒後にマスターが各ブロックの状態をリセットする
            SendCustomEventDelayedSeconds(nameof(MasterResetBlocks), 3.0f);
        }
    }
    
    public void TriggerAllClients()
    {
        // 各ブロックにワープ開始を命令
        // ブロック内部のロジックにより、実際にワープするのは操作した本人のみ
        if (PastWarpBlock != null) PastWarpBlock.Warp();
        if (FutureWarpBlock != null) FutureWarpBlock.Warp();
    }
    
    // マスターのみで実行されるリセット処理
    public void MasterResetBlocks()
    {
        // マスターが各ブロックのオーナーになり、リセットを命令する
        if (PastWarpBlock != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, PastWarpBlock.gameObject);
            PastWarpBlock.ResetState();
        }
        if (FutureWarpBlock != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, FutureWarpBlock.gameObject);
            FutureWarpBlock.ResetState();
        }
        
        isSequenceActive = false; // 再度、判定できるようにロックを解除する
    }
}

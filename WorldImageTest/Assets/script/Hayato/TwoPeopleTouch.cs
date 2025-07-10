using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class TwoPeopleTouch : UdonSharpBehaviour
{
    [FormerlySerializedAs("PastWarpBlock")] [SerializeField] private WarpBlockTwoPeople pastWarpBlockTwoPeople;
    [FormerlySerializedAs("FutureWarpBlock")] [SerializeField] private WarpBlockTwoPeople futureWarpBlockTwoPeople;
    
    private bool isSequenceActive = false;

    void Start()
    {
        if (pastWarpBlockTwoPeople == null)
        {
            Debug.LogWarning("PastWarpBlockが入ってない");
        }

        if (futureWarpBlockTwoPeople == null)
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
        
        if (pastWarpBlockTwoPeople.isOn && futureWarpBlockTwoPeople.isOn)
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
        if (pastWarpBlockTwoPeople != null) pastWarpBlockTwoPeople.Warp();
        if (futureWarpBlockTwoPeople != null) futureWarpBlockTwoPeople.Warp();
    }
    
    // マスターのみで実行されるリセット処理
    public void MasterResetBlocks()
    {
        // マスターが各ブロックのオーナーになり、リセットを命令する
        if (pastWarpBlockTwoPeople != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, pastWarpBlockTwoPeople.gameObject);
            pastWarpBlockTwoPeople.ResetState();
        }
        if (futureWarpBlockTwoPeople != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, futureWarpBlockTwoPeople.gameObject);
            futureWarpBlockTwoPeople.ResetState();
        }
        
        isSequenceActive = false; // 再度、判定できるようにロックを解除する
    }
}

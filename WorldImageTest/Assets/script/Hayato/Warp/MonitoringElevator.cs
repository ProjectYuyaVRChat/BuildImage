using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class MonitoringElevator : UdonSharpBehaviour
{
    [SerializeField] private ElevatorWarp entrance1;
    [SerializeField] private ElevatorWarp entrance2;
    
    private bool isSequenceActive = false;

    void Start()
    {
        if (entrance1 == null)
        {
            Debug.LogWarning("PastWarpBlockが入ってない");
        }

        if (entrance2 == null)
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
        
        if (entrance1.isOn && entrance2.isOn)
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
        if (entrance1 != null) entrance1.Warp();
        if (entrance2 != null) entrance2.Warp();
    }
    
    // マスターのみで実行されるリセット処理
    public void MasterResetBlocks()
    {
        // マスターが各ブロックのオーナーになり、リセットを命令する
        if (entrance1 != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, entrance1.gameObject);
            entrance1.ResetState();
        }
        if (entrance2 != null)
        {
            Networking.SetOwner(Networking.LocalPlayer, entrance2.gameObject);
            entrance2.ResetState();
        }
        
        isSequenceActive = false; // 再度、判定できるようにロックを解除する
    }
}

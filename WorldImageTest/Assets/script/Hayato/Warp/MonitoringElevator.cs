using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class MonitoringElevator : UdonSharpBehaviour
{
    [SerializeField] private ElevatorWarp entrance1;
    [SerializeField] private ElevatorWarp entrance2;
    
    // 二重発火防止のためのローカルタイマー
    private float _triggerCooldown = 0f;

    void Start()
    {
        if (entrance1 == null || entrance2 == null)
        {
            Debug.LogWarning("WarpBlockが設定されていません");
        }
    }
    
    void Update()
    {
        // 1. このオブジェクトのオーナー（通常はインスタンスマスターまたは権限保持者）だけが処理を行う
        // これにより、全員が一斉に命令を送るネットワーク競合を防ぎます
        if (!Networking.IsOwner(this.gameObject)) return;

        // クールダウン中は判定しない
        if (_triggerCooldown > 0f)
        {
            _triggerCooldown -= Time.deltaTime;
            return;
        }
        
        // 2. isOn だけでなく、IDが正しく同期されているか(IsReady)を確認する
        // これにより「スイッチはONだが誰が押したか不明(-1)」という状態での発火を防ぎます
        if (entrance1.IsReady && entrance2.IsReady)
        {
            // 全員にワープ命令を送信
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(TriggerAllClients));
            
            // 連続実行を防ぐためクールダウンを設定（例えば5秒）
            _triggerCooldown = 5f;
        }
    }
    
    public void TriggerAllClients()
    {
        // 受け取った全員が自分のエレベーターを確認して、自分が乗っていればワープする
        if (entrance1 != null) entrance1.Warp();
        if (entrance2 != null) entrance2.Warp();
    }
}

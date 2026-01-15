using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GimmickManager : UdonSharpBehaviour
{
    [Header("必要なクリア数")]
    [SerializeField] private int requiredClearCount = 3;

    [Header("すべてクリアした時の処理対象")]
    [SerializeField] private GameObject completeObject; // 例：開くドア、表示される文字など

    // 現在のクリア数を同期変数として管理（途中参加者対応のため）
    [UdonSynced(UdonSyncMode.None)] private int _currentClearCount = 0;

    void Start()
    {
        // 最初は完了オブジェクトを非表示にしておく等の初期化
        if (completeObject != null) completeObject.SetActive(false);
    }

    // 各ギミックから呼ばれる関数
    public void ReportClear()
    {
        // オーナー（親機）だけが計算を行う
        if (!Networking.IsOwner(gameObject))
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ReportClear));
            return;
        }

        _currentClearCount++;
        RequestSerialization(); // 変数の変更を全員に通知

        CheckCompletion();
    }

    // クリア判定
    public void CheckCompletion()
    {
        if (_currentClearCount >= requiredClearCount)
        {
            // 全員に対して完了アクションを実行
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(OnAllCleared));
        }
    }

    // 全員実行される完了時の処理
    public void OnAllCleared()
    {
        Debug.Log("すべてのギミックがクリアされました！");
        
        // ここに「次の処理」を書く
        if (completeObject != null) completeObject.SetActive(true);
    }
    
    // 途中参加などで変数が同期された時に呼ばれる
    public override void OnDeserialization()
    {
        // すでにクリア済みなら状態を反映する
        if (_currentClearCount >= requiredClearCount)
        {
            if (completeObject != null) completeObject.SetActive(true);
        }
    }
}

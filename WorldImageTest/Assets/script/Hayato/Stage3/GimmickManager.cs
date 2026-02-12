using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GimmickManager : UdonSharpBehaviour
{
    [Header("必要なクリア数")]
    [SerializeField] private int requiredClearCount = 3;

    [Header("すべてクリアした時の処理対象")]
    [SerializeField] private GameObject completeObject;
    [SerializeField] private GameObject completeObject2;// 例：開くドア、表示される文字など

    // 現在のクリア数を同期変数として管理（途中参加者対応のため）
    [UdonSynced(UdonSyncMode.None)] private int _currentClearCount = 0;

    

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
            //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(OnAllCleared));
            OnAllCleared();
        }
    }

    // 全員実行される完了時の処理
    public void OnAllCleared()
    {
        Debug.Log("すべてのギミックがクリアされました！");
        
        // ここに「次の処理」を書く
        if (completeObject != null) completeObject.SetActive(true);
        if (completeObject2 != null) completeObject2.SetActive(true);
    }
}

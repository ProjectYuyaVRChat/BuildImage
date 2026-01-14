using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UpperLimiter : UdonSharpBehaviour
{
    [Tooltip("これ以上の高さに行かないようにする制限値（Y座標）")]
    [SerializeField] private float upperLimit;

    [Tooltip("追従対象のオブジェクト（手など）")]
    [SerializeField] private GameObject receiver;
    
    [Tooltip("追従対象からの高さオフセット")]
    [SerializeField] private float receiverHeightDistance = 0.05f;

    // syncObjは回転計算の安定化により不要になったため削除しましたが、
    // 既存のUnity設定を壊さないよう残しておきたい場合は private GameObject syncObj; として残してもOKです。

    private void Update()
    {
        if (receiver == null) return;

        // --- 1. 位置の計算 ---
        Vector3 targetPos = receiver.transform.position;
        targetPos.y -= receiverHeightDistance;

        // 上限チェック：超えていたら上限値に書き換え、そうでなければそのまま
        if (targetPos.y > upperLimit)
        {
            targetPos.y = upperLimit;
        }

        // --- 2. 回転の計算（ベクトル方式） ---
        // receiverの「正面の向き」を取得
        Vector3 forwardDir = receiver.transform.forward;
        
        // Y成分を0にして、水平方向の成分だけにする（これで傾きを無視できる）
        forwardDir.y = 0;

        // 計算エラー防止：もし真上/真下を向いてベクトルが0になってしまったら、前のフレームの回転を維持
        Quaternion finalRot;
        if (forwardDir.sqrMagnitude > 0.001f)
        {
            // 水平化したベクトルから、新しい回転を作成（Y軸回転のみの状態になる）
            finalRot = Quaternion.LookRotation(forwardDir, Vector3.up);
        }
        else
        {
            // ベクトルが取れない（真上などを向いている）場合は回転しない
            finalRot = transform.rotation;
        }

        // --- 3. 適用 ---
        transform.SetPositionAndRotation(targetPos, finalRot);
    }
}

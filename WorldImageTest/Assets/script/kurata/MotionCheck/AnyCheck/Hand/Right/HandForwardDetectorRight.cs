using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 右手を体の前に突き出したか判定する
/// </summary>
public class HandForwardDetectorRight : MotionDetectorBase
{
    [SerializeField] private float forwardThreshold = 0.3f; // 判定のしきい値[m]
    private bool isForward = false;
    private bool initialized = false;

    // 外部から状態を取得するためのプロパティ
    public bool IsForward => isForward;
    
    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    
    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

    protected override void DetectMotion()
    {
        // 体基準のローカル座標に変換
        Vector3 localHand = Quaternion.Inverse(baseRot) * (rightHandPos - basePos);

        // 初回のみ現在の状態で初期化
        if (!initialized)
        {
            isForward = (localHand.z > forwardThreshold);
            initialized = true;
            return;
        }

        bool wasForward = isForward;

        if (!isForward && localHand.z > forwardThreshold)
        {
            isForward = true;
            ShowMotionMessage("右手を前に突き出した");
        }
        else if (isForward && localHand.z <= forwardThreshold)
        {
            isForward = false;
            ShowMotionMessage("右手を前から戻した");
        }
        
        // 状態が変化したらシステムに通知
        if (wasForward != isForward)
        {
            // 中央集権型システムが設定されている場合はそちらに送信
            if (centralizedDetector != null)
            {
                centralizedDetector.SetRightHandForwardState(isForward);
            }
            // 従来の個別システムにも送信（後方互換性）
            else if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetRightHandForwardState(isForward);
            }
        }
    }
}
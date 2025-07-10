using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 右手を体の上に上げたか判定する
/// </summary>
public class HandUpDetectorRight : MotionDetectorBase
{
    [SerializeField] private float upThreshold = 0.1f;
    private bool isUp = false;
    private bool initialized = false;

    // 外部から状態を取得するためのプロパティ
    public bool IsUp => isUp;
    
    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    
    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

    protected override void DetectMotion()
    {
        float handDelta = rightHandPos.y - baseRightHandPos.y;

        // デバッグ情報を表示
        if (debugText != null)
        {
            debugText.text = $"右手高さΔ: {handDelta:F3}\n閾値: {upThreshold:F3}\n状態: {(isUp ? "上げた" : "下げた")}";
        }

        if (!initialized)
        {
            isUp = (handDelta > upThreshold);
            initialized = true;
            return;
        }

        bool wasUp = isUp;

        if (!isUp && handDelta > upThreshold)
        {
            isUp = true;
            ShowMotionMessage("右手を上に上げた");
        }
        else if (isUp && handDelta <= upThreshold)
        {
            isUp = false;
            ShowMotionMessage("右手を上から戻した");
        }
        
        // 状態が変化したらシステムに通知
        if (wasUp != isUp)
        {
            if (centralizedDetector != null)
            {
                centralizedDetector.SetRightHandUpState(isUp);
            }
            else if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetRightHandUpState(isUp);
            }
        }
    }
}
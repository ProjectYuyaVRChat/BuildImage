using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 右手を体の上に上げたか判定する（ローカル基準版）
/// </summary>
public class HandUpDetectorRight : MotionDetectorBase
{
    [SerializeField] private float upThreshold = 0.1f;
    private bool isUp = false;
    private bool initialized = false;

    public bool IsUp => isUp;

    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

    protected override void DetectMotion()
    {
        // 右手の位置を体ローカルに変換
        Vector3 localHand = Quaternion.Inverse(baseRot) * (rightHandPos - basePos);

        // デバッグ
        if (debugText != null)
        {
            debugText.text = $"右手位置(ローカルY): {localHand.y:F3}\n" +
                             $"閾値: {upThreshold:F3}\n" +
                             $"状態: {(isUp ? "上げた" : "下げた")}";
        }

        if (!initialized)
        {
            isUp = (localHand.y > upThreshold);
            initialized = true;
            return;
        }

        bool wasUp = isUp;

        // 上げた
        if (!isUp && localHand.y > upThreshold)
        {
            isUp = true;
            ShowMotionMessage("右手を上に上げた");
        }
        // 下げた
        else if (isUp && localHand.y <= upThreshold)
        {
            isUp = false;
            ShowMotionMessage("右手を上から戻した");
        }

        // 状態変化を通知
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
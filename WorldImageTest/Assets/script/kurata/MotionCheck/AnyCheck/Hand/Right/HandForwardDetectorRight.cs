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
    }
}
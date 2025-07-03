using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 左手を体の前に突き出したか判定する
/// </summary>
public class HandForwardDetectorLeft : MotionDetectorBase
{
    [SerializeField] private float forwardThreshold = 0.3f;
    private bool isForward = false;
    private bool initialized = false;

    // 外部から状態を取得するためのプロパティ
    public bool IsForward => isForward;
    
    // DoorGimmickSystemへの参照
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

    protected override void DetectMotion()
    {
        Vector3 localHand = Quaternion.Inverse(baseRot) * (leftHandPos - basePos);

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
            ShowMotionMessage("左手を前に突き出した");
        }
        else if (isForward && localHand.z <= forwardThreshold)
        {
            isForward = false;
            ShowMotionMessage("左手を前から戻した");
        }
        
        // 状態が変化したらDoorGimmickSystemに通知
        if (wasForward != isForward && doorGimmickSystem != null)
        {
            doorGimmickSystem.SetLeftHandForwardState(isForward);
        }
    }
}
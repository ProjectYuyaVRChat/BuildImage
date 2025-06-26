using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 左手を体の真横に広げたか判定する
/// </summary>
public class HandSideDetectorLeft : MotionDetectorBase
{
    [SerializeField] private float sideThreshold = 0.3f;
    private bool isSide = false;
    private bool initialized = false;

    // 外部から状態を取得するためのプロパティ
    public bool IsSide => isSide;
    
    // DoorGimmickSystemへの参照
    [SerializeField] private DoorGimmickSystem doorGimmickSystem;

    protected override void DetectMotion()
    {
        Vector3 localHand = Quaternion.Inverse(baseRot) * (leftHandPos - basePos);

        if (!initialized)
        {
            isSide = (Mathf.Abs(localHand.x) > sideThreshold);
            initialized = true;
            return;
        }

        bool wasSide = isSide;

        if (!isSide && Mathf.Abs(localHand.x) > sideThreshold)
        {
            isSide = true;
            ShowMotionMessage("左手を真横に広げた");
        }
        else if (isSide && Mathf.Abs(localHand.x) <= sideThreshold)
        {
            isSide = false;
            ShowMotionMessage("左手を横から戻した");
        }
        
        // 状態が変化したらDoorGimmickSystemに通知
        if (wasSide != isSide && doorGimmickSystem != null)
        {
            doorGimmickSystem.SetLeftHandSideState(isSide);
        }
    }
}
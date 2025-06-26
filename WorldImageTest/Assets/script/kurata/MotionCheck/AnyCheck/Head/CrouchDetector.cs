using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

/// <summary>
/// 伏せの時も反応しちゃうから必要になったらやろう
/// </summary>
public class CrouchDetector : MotionDetectorBase
{
    private bool isCrouching = false;
    //private float baseHeadHeight = 0f;
    private float crouchThreshold = 0.3f; // 基準高さからどれだけ下がったらしゃがみか
    private float HeadHeight = 0f;

    private bool initialized = false;

    private float CrouchVelocity = 0.5f;

    //private int calibrationFrameCount = 0;
    private const int calibrationFramesNeeded = 30;
    
    // DoorGimmickSystemへの参照
    [SerializeField] private DoorGimmickSystem doorGimmickSystem;
    
    // 外部から状態を取得するためのプロパティ
    public bool IsCrouching => isCrouching;

    protected override void DetectMotion()
    {
        if (!localPlayer.IsPlayerGrounded()) return;

        float currentHeadHeight = headPos.y - basePos.y;
        float heightDiff = baseHeadHeight - currentHeadHeight;
        float verticalVelocity = (currentHeadHeight - lastHeadHeight) / Time.deltaTime;

        bool wasCrouching = isCrouching;

        if (heightDiff > crouchThreshold && !isCrouching && Mathf.Abs(verticalVelocity) < CrouchVelocity)
        {
            isCrouching = true;
            ShowMotionMessage("しゃがみ");
        }
        else if (heightDiff <= crouchThreshold && isCrouching && Mathf.Abs(verticalVelocity) < CrouchVelocity)
        {
            isCrouching = false;
            ShowMotionMessage("立ち上がり");
        }
        
        // 状態が変化したらDoorGimmickSystemに通知
        if (wasCrouching != isCrouching && doorGimmickSystem != null)
        {
            doorGimmickSystem.SetCrouchState(isCrouching);
        }

        lastHeadHeight = currentHeadHeight;
    }

}

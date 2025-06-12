using UdonSharp;
using UnityEngine;
using VRC.SDKBase;


/// <summary>
/// 伏せの時も反応しちゃうから必要になったらやろう
/// </summary>
public class CrouchDetector : MotionDetectorBase
{
    private bool isCrouching = false;
    private float baseHeadHeight = 0f;
    private float crouchThreshold = 0.3f; // 基準高さからどれだけ下がったらしゃがみか
    private float HeadHight = 0f;

    private bool initialized = false;

    private float CrouchVelocity = 0.5f;
    protected override void DetectMotion()
    {
        if (!localPlayer.IsPlayerGrounded()) return;

        Vector3 headPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        Vector3 basePos = localPlayer.GetPosition();
        float CurrentheadHeight = headPosition.y - basePos.y;

        // 最初の1回だけ基準を記録
        if (!initialized)
        {
            baseHeadHeight = CurrentheadHeight;
            HeadHight = CurrentheadHeight;
            initialized = true;
            return;
        }

        float heightDiff = baseHeadHeight - CurrentheadHeight;
        float verticalvelocity = (CurrentheadHeight - HeadHight) / Time.deltaTime;

        if (heightDiff > crouchThreshold && !isCrouching && Mathf.Abs(verticalvelocity) < CrouchVelocity)
        {
            isCrouching = true;
            ShowMotionMessage("しゃがみ");
        }
        else if (heightDiff <= crouchThreshold && isCrouching && Mathf.Abs(verticalvelocity) < CrouchVelocity)
        {
            isCrouching = false;
            ShowMotionMessage("立ち上がり");
        }
        HeadHight = CurrentheadHeight;
    }
}

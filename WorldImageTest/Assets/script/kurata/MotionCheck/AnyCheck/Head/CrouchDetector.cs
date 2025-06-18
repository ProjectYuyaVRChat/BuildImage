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
    protected override void DetectMotion()
    {
        if (!localPlayer.IsPlayerGrounded()) return;

        float currentHeadHeight = headPos.y - basePos.y;
        float heightDiff = baseHeadHeight - currentHeadHeight;
        float verticalVelocity = (currentHeadHeight - lastHeadHeight) / Time.deltaTime;

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

        lastHeadHeight = currentHeadHeight;
    }

}

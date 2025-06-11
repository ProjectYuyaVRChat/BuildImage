using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class CrouchDetector : MotionDetectorBase
{
    private bool isCrouching = false;
    private float baseHeadHeight = 0f;
    private float crouchThreshold = 0.3f; // 基準高さからどれだけ下がったらしゃがみか

    private bool initialized = false;

    protected override void DetectMotion()
    {
        if (!localPlayer.IsPlayerGrounded()) return;

        Vector3 headPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        float headHeight = headPosition.y - localPlayer.GetPosition().y;

        // 最初の1回だけ基準を記録
        if (!initialized)
        {
            baseHeadHeight = headHeight;
            initialized = true;
            return;
        }

        float heightDiff = baseHeadHeight - headHeight;

        if (heightDiff > crouchThreshold && !isCrouching)
        {
            isCrouching = true;
            ShowMotionMessage("しゃがみ");
        }
        else if (heightDiff <= crouchThreshold && isCrouching)
        {
            isCrouching = false;
            ShowMotionMessage("立ち上がり");
        }
    }
}

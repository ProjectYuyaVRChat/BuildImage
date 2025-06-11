using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class ProneDetector : MotionDetectorBase
{
    private bool isProne = false;
    private float baseHeadHeight = 0f;
    private float proneThreshold = 0.7f;    // しゃがみ=0.3

    private bool initialized = false;

    protected override void DetectMotion()
    {
        if (!localPlayer.IsPlayerGrounded()) return;

        Vector3 headPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        float headHeight = headPosition.y - localPlayer.GetPosition().y;

        if (!initialized)
        {
            baseHeadHeight = headHeight;
            initialized = true;
            return;
        }

        float heightDiff = baseHeadHeight - headHeight;

        if (heightDiff > proneThreshold && !isProne)
        {
            isProne = true;
            ShowMotionMessage("伏せ");
        }
        else if (heightDiff <= proneThreshold && isProne)
        {
            isProne = false;
            ShowMotionMessage("伏せ解除");
        }
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class ProneDetector : MotionDetectorBase
{
    private bool isProne = false;
    private float proneThreshold = 0.3f;

    private bool initialized = false;
    private float proneBaseHeight = 0f; // 名前を変えて重複を避ける

    protected override void DetectMotion()
    {
        if (!localPlayer.IsPlayerGrounded()) return;

        float currentHeight = headPos.y - basePos.y;

        if (!initialized)
        {
            proneBaseHeight = currentHeight;
            initialized = true;
            return;
        }

        float heightDiff = proneBaseHeight - currentHeight;

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

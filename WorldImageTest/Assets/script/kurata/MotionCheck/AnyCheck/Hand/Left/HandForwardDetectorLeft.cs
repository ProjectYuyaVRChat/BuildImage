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

    protected override void DetectMotion()
    {
        Vector3 localHand = Quaternion.Inverse(baseRot) * (leftHandPos - basePos);

        if (!initialized)
        {
            isForward = (localHand.z > forwardThreshold);
            initialized = true;
            return;
        }

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
    }
}
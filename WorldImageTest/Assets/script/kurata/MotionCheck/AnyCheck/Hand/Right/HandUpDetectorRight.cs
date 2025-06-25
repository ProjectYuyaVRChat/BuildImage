using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 右手を体の上に上げたか判定する
/// </summary>
public class HandUpDetectorRight : MotionDetectorBase
{
    [SerializeField] private float upThreshold = 0.3f;
    private bool isUp = false;
    private bool initialized = false;

    protected override void DetectMotion()
    {
        Vector3 localHand = Quaternion.Inverse(baseRot) * (rightHandPos - basePos);

        if (!initialized)
        {
            isUp = (localHand.y > upThreshold);
            initialized = true;
            return;
        }

        if (!isUp && localHand.y > upThreshold)
        {
            isUp = true;
            ShowMotionMessage("右手を上に上げた");
        }
        else if (isUp && localHand.y <= upThreshold)
        {
            isUp = false;
            ShowMotionMessage("右手を上から戻した");
        }
    }
}
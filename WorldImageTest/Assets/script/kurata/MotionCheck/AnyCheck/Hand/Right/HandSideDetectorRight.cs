using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 右手を体の真横に広げたか判定する
/// </summary>
public class HandSideDetectorRight : MotionDetectorBase
{
    [SerializeField] private float sideThreshold = 0.3f;
    private bool isSide = false;
    private bool initialized = false;

    protected override void DetectMotion()
    {
        Vector3 localHand = Quaternion.Inverse(baseRot) * (rightHandPos - basePos);

        if (!initialized)
        {
            isSide = (Mathf.Abs(localHand.x) > sideThreshold);
            initialized = true;
            return;
        }

        if (!isSide && Mathf.Abs(localHand.x) > sideThreshold)
        {
            isSide = true;
            ShowMotionMessage("右手を真横に広げた");
        }
        else if (isSide && Mathf.Abs(localHand.x) <= sideThreshold)
        {
            isSide = false;
            ShowMotionMessage("右手を横から戻した");
        }
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public enum NeckTiltState
{
    Default,
    TiltLeft,
    TiltRight,
    TiltForward,
    TiltBackward
}
public class HeadTiltAllDirectionDetector : MotionDetectorBase
{
    private NeckTiltState currentState = NeckTiltState.Default;

    // 傾き判定の閾値（度数）
    [SerializeField] private float tiltThreshold = 15f;

    protected override void DetectMotion()
    {
        Vector3 euler = headRot.eulerAngles;

        // Euler角は0〜360なので -180〜180 に変換
        float roll = euler.z > 180f ? euler.z - 360f : euler.z;   // 左右傾き
        float pitch = euler.x > 180f ? euler.x - 360f : euler.x;  // 前後傾き

        NeckTiltState newState = NeckTiltState.Default;

        if (roll > tiltThreshold)
        {
            newState = NeckTiltState.TiltRight;
        }
        else if (roll < -tiltThreshold)
        {
            newState = NeckTiltState.TiltLeft;
        }
        else if (pitch > tiltThreshold)
        {
            newState = NeckTiltState.TiltForward;
        }
        else if (pitch < -tiltThreshold)
        {
            newState = NeckTiltState.TiltBackward;
        }

        if (newState != currentState)
        {
            currentState = newState;
            switch (currentState)
            {
                case NeckTiltState.TiltLeft:
                    ShowMotionMessage("首を左に傾けた");
                    break;
                case NeckTiltState.TiltRight:
                    ShowMotionMessage("首を右に傾けた");
                    break;
                case NeckTiltState.TiltForward:
                    ShowMotionMessage("首を前に傾けた");
                    break;
                case NeckTiltState.TiltBackward:
                    ShowMotionMessage("首を後ろに傾けた");
                    break;
                case NeckTiltState.Default:
                    ShowMotionMessage("首を戻した");
                    break;
            }
        }
    }
}
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
    [SerializeField] private float tiltThresholdRoll = 15f;
    [SerializeField] private float tiltThresholdPitch = 15f;

    // 外部から状態を取得するためのプロパティ
    public NeckTiltState CurrentState => currentState;
    public bool IsTiltLeft => currentState == NeckTiltState.TiltLeft;
    public bool IsTiltRight => currentState == NeckTiltState.TiltRight;
    public bool IsTiltForward => currentState == NeckTiltState.TiltForward;
    public bool IsTiltBackward => currentState == NeckTiltState.TiltBackward;
    
    // DoorGimmickSystemへの参照
    [SerializeField] private DoorGimmickSystem doorGimmickSystem;

    protected override void DetectMotion()
    {
        Vector3 euler = headRot.eulerAngles;

        // Euler角は0〜360なので -180〜180 に変換
        float roll = euler.z > 180f ? euler.z - 360f : euler.z;   // 左右傾き
        float pitch = euler.x > 180f ? euler.x - 360f : euler.x;  // 前後傾き

        NeckTiltState newState = NeckTiltState.Default;

        if (roll > tiltThresholdRoll)
        {
            newState = NeckTiltState.TiltRight;
        }
        else if (roll < -tiltThresholdRoll)
        {
            newState = NeckTiltState.TiltLeft;
        }
        else if (pitch > tiltThresholdPitch)
        {
            newState = NeckTiltState.TiltForward;
        }
        else if (pitch < -tiltThresholdPitch)
        {
            newState = NeckTiltState.TiltBackward;
        }

        if (newState != currentState)
        {
            NeckTiltState previousState = currentState;
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
            
            // 状態が変化したらDoorGimmickSystemに通知
            if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetHeadTiltLeftState(currentState == NeckTiltState.TiltLeft);
                doorGimmickSystem.SetHeadTiltRightState(currentState == NeckTiltState.TiltRight);
                doorGimmickSystem.SetHeadTiltForwardState(currentState == NeckTiltState.TiltForward);
                doorGimmickSystem.SetHeadTiltBackwardState(currentState == NeckTiltState.TiltBackward);
            }
        }
    }
}
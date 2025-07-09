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
    
    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    
    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

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
            
            // 状態が変化したらシステムに通知
            if (previousState != currentState)
            {
                // 中央集権型システムが設定されている場合はそちらに送信
                if (centralizedDetector != null)
                {
                    centralizedDetector.SetHeadTiltLeftState(currentState == NeckTiltState.TiltLeft);
                    centralizedDetector.SetHeadTiltRightState(currentState == NeckTiltState.TiltRight);
                    centralizedDetector.SetHeadTiltForwardState(currentState == NeckTiltState.TiltForward);
                    centralizedDetector.SetHeadTiltBackwardState(currentState == NeckTiltState.TiltBackward);
                }
                // 従来の個別システムにも送信（後方互換性）
                else if (doorGimmickSystem != null)
                {
                    doorGimmickSystem.SetHeadTiltLeftState(currentState == NeckTiltState.TiltLeft);
                    doorGimmickSystem.SetHeadTiltRightState(currentState == NeckTiltState.TiltRight);
                    doorGimmickSystem.SetHeadTiltForwardState(currentState == NeckTiltState.TiltForward);
                    doorGimmickSystem.SetHeadTiltBackwardState(currentState == NeckTiltState.TiltBackward);
                }
            }
        }
    }
}
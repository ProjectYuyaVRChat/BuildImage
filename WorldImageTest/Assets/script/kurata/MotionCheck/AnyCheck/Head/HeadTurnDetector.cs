using UdonSharp;
using UnityEngine;
using VRC.SDKBase;


public enum HeadTurnState
{
    Default,
    LookingLeft,
    LookingRight
}

public class HeadTurnDetector : MotionDetectorBase
{
    private HeadTurnState currentState = HeadTurnState.Default;

    [SerializeField] private float turnThreshold = 20f; // Yaw差

    protected override void DetectMotion()
    {
        float bodyYaw = bodyRot.eulerAngles.y;
        float headYaw = headRot.eulerAngles.y;

        // 差分を-180〜180の範囲にする
        float yawDiff = Mathf.DeltaAngle(bodyYaw, headYaw);

        HeadTurnState newState = HeadTurnState.Default;

        if (yawDiff > turnThreshold)
        {
            newState = HeadTurnState.LookingRight;
        }
        else if (yawDiff < -turnThreshold)
        {
            newState = HeadTurnState.LookingLeft;
        }

        if (newState != currentState)
        {
            currentState = newState;
            switch (currentState)
            {
                case HeadTurnState.LookingLeft:
                    ShowMotionMessage("首を左に向けた");
                    break;
                case HeadTurnState.LookingRight:
                    ShowMotionMessage("首を右に向けた");
                    break;
                case HeadTurnState.Default:
                    ShowMotionMessage("首の向きを戻した");
                    break;
            }
        }

        // 戻したときもNeutralに戻す（±turnThresholdの範囲内）
        if (currentState != HeadTurnState.Default && Mathf.Abs(yawDiff) <= turnThreshold)
        {
            currentState = HeadTurnState.Default;
            ShowMotionMessage("首の向きを戻した");
        }
    }
}

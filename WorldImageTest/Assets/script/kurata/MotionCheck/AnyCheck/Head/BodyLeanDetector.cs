using UdonSharp;
using UnityEngine;
using VRC.SDKBase;


public enum LeanState
{
    Default,
    Bowing,         // 前傾
    LeaningBack,    // 後傾
    LeaningLeft,
    LeaningRight
}
public class BodyLeanDetector : MotionDetectorBase
{
    private LeanState currentState = LeanState.Default;

    [SerializeField] private float forwardBackwardThreshold = 0.15f; // 閾値(m)
    [SerializeField] private float leftRightThreshold = 0.10f; ///

    protected override void DetectMotion()
    {
        // プレイヤーの体の回転を考慮してローカル座標系に変換
        Vector3 localHeadOffset = Quaternion.Inverse(baseRot) * (headPos - basePos);

        LeanState newState = LeanState.Default;

        if (localHeadOffset.z > forwardBackwardThreshold)
        {
            newState = LeanState.Bowing;
        }
        else if (localHeadOffset.z < -forwardBackwardThreshold)
        {
            newState = LeanState.LeaningBack;
        }
        else if (localHeadOffset.x > leftRightThreshold)
        {
            newState = LeanState.LeaningRight;
        }
        else if (localHeadOffset.x < -leftRightThreshold)
        {
            newState = LeanState.LeaningLeft;
        }

        if (newState != currentState)
        {
            currentState = newState;
            switch (currentState)
            {
                case LeanState.Bowing:
                    ShowMotionMessage("お辞儀している");
                    break;
                case LeanState.LeaningBack:
                    ShowMotionMessage("のけぞっている");
                    break;
                case LeanState.LeaningLeft:
                    ShowMotionMessage("左に傾けている");
                    break;
                case LeanState.LeaningRight:
                    ShowMotionMessage("右に傾けている");
                    break;
                case LeanState.Default:
                    ShowMotionMessage("姿勢を戻した");
                    break;
            }
        }

        // Defaultに戻す条件
        if (currentState != LeanState.Default)
        {
            bool nearNeutral =
                Mathf.Abs(localHeadOffset.z) <= forwardBackwardThreshold &&
                Mathf.Abs(localHeadOffset.x) <= leftRightThreshold;

            if (nearNeutral)
            {
                currentState = LeanState.Default;
                ShowMotionMessage("姿勢を戻した");
            }
        }
    }
}

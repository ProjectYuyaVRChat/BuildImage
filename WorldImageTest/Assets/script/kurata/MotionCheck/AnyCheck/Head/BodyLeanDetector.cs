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
    [SerializeField] private float returnCooldown = 1.0f;

    private float lastLeanChangeTime = 0f;
    
    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    
    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;
    
    // 外部から状態を取得するためのプロパティ
    public LeanState CurrentState => currentState;
    public bool IsLeanLeft => currentState == LeanState.LeaningLeft;
    public bool IsLeanRight => currentState == LeanState.LeaningRight;
    public bool IsLeanForward => currentState == LeanState.Bowing;
    public bool IsLeanBackward => currentState == LeanState.LeaningBack;
    
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

        LeanState previousState = currentState;

        if (newState != currentState && newState != LeanState.Default)
        {
            currentState = newState;
            lastLeanChangeTime = Time.time;
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

            if (nearNeutral && Time.time - lastLeanChangeTime > returnCooldown)
            {
                currentState = LeanState.Default;
                ShowMotionMessage("姿勢を戻した");
            }
        }
        
        // 状態が変化したらシステムに通知
        if (previousState != currentState)
        {
            // 中央集権型システムが設定されている場合はそちらに送信
            if (centralizedDetector != null)
            {
                centralizedDetector.SetBodyLeanLeftState(currentState == LeanState.LeaningLeft);
                centralizedDetector.SetBodyLeanRightState(currentState == LeanState.LeaningRight);
                centralizedDetector.SetBodyLeanForwardState(currentState == LeanState.Bowing);
                centralizedDetector.SetBodyLeanBackwardState(currentState == LeanState.LeaningBack);
            }
            // 従来の個別システムにも送信（後方互換性）
            else if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetBodyLeanLeftState(currentState == LeanState.LeaningLeft);
                doorGimmickSystem.SetBodyLeanRightState(currentState == LeanState.LeaningRight);
                doorGimmickSystem.SetBodyLeanForwardState(currentState == LeanState.Bowing);
                doorGimmickSystem.SetBodyLeanBackwardState(currentState == LeanState.LeaningBack);
            }
        }
    }
}

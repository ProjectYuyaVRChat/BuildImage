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

    [SerializeField] private float returnCooldown = 1.0f; // Yaw差

    private float lastTurnTime = 0f;
    
    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    
    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;
    [SerializeField] private DoorAreaTrigger areaTrigger;
    
    // 外部から状態を取得するためのプロパティ
    public HeadTurnState CurrentState => currentState;
    public bool IsTurnLeft => currentState == HeadTurnState.LookingLeft;
    public bool IsTurnRight => currentState == HeadTurnState.LookingRight;
    
    protected override void DetectMotion()
    {
        float bodyYaw = baseRot.eulerAngles.y;
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

        HeadTurnState previousState = currentState;

        if (newState != currentState && newState != HeadTurnState.Default)
        {
            currentState = newState;
            lastTurnTime = Time.time;

            switch (currentState)
            {
                case HeadTurnState.LookingLeft:
                    ShowMotionMessage("首を左に向けた");
                    break;
                case HeadTurnState.LookingRight:
                    ShowMotionMessage("首を右に向けた");
                    break;
            }
        }

        // 戻したときもNeutralに戻す（±turnThresholdの範囲内）
        if (currentState != HeadTurnState.Default && Mathf.Abs(yawDiff) <= turnThreshold && Time.time - lastTurnTime > returnCooldown)
        {
            currentState = HeadTurnState.Default;
            ShowMotionMessage("首の向きを戻した");
        }
        
        // 状態が変化したらシステムに通知
        if (previousState != currentState)
        {
            // 中央集権型システムが設定されている場合はそちらに送信
            if (centralizedDetector != null)
            {
                centralizedDetector.SetHeadTurnLeftState(currentState == HeadTurnState.LookingLeft);
                centralizedDetector.SetHeadTurnRightState(currentState == HeadTurnState.LookingRight);
            }
            // 従来の個別システムにも送信（後方互換性）
            else if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetHeadTurnLeftState(currentState == HeadTurnState.LookingLeft);
                doorGimmickSystem.SetHeadTurnRightState(currentState == HeadTurnState.LookingRight);
            }
        }
    }
    
    // デバッグ用の状態管理
    private bool lastMotionDetectionState = false;
    
    // 範囲検知システムとの連携
    protected override bool IsMotionDetectionEnabled()
    {
        // 範囲検知システムが設定されていない場合は常に有効
        if (areaTrigger == null)
        {
            return true;
        }
        
        // 範囲検知システムがアクティブな場合のみモーション検知を有効にする
        bool isEnabled = areaTrigger.IsAreaActive;
        
        // デバッグ情報を表示（状態が変化した時のみ）
        if (doorGimmickSystem != null && doorGimmickSystem.showDebugInfo)
        {
            if (lastMotionDetectionState != isEnabled)
            {
                Debug.Log($"[HeadTurnDetector] モーション検知: {(isEnabled ? "有効" : "無効")} (エリア: {areaTrigger.AreaName})");
                lastMotionDetectionState = isEnabled;
            }
        }
        
        return isEnabled;
    }
}

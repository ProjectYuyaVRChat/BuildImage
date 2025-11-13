using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

/// <summary>
/// 伏せの時も反応しちゃうから必要になったらやろう
/// </summary>
public class CrouchDetector : MotionDetectorBase
{
    private bool isCrouching = false;
    //private float baseHeadHeight = 0f;
    [SerializeField] private float crouchThreshold = 0.3f; // 基準高さからどれだけ下がったらしゃがみか
    private float HeadHeight = 0f;

    private bool initialized = false;

    private float CrouchVelocity = 0.5f;

    //private int calibrationFrameCount = 0;
    private const int calibrationFramesNeeded = 30;
    
    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    
    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;
    [SerializeField] private DoorAreaTrigger areaTrigger;
    
    // 外部から状態を取得するためのプロパティ
    public bool IsCrouching => isCrouching;

    protected override void DetectMotion()
    {
        if (!localPlayer.IsPlayerGrounded()) return;

        float currentHeadHeight = headPos.y - basePos.y;
        float heightDiff = baseHeadHeight - currentHeadHeight;
        float verticalVelocity = (currentHeadHeight - lastHeadHeight) / Time.deltaTime;

        bool wasCrouching = isCrouching;

        if (heightDiff > crouchThreshold && !isCrouching && Mathf.Abs(verticalVelocity) < CrouchVelocity)
        {
            isCrouching = true;
            ShowMotionMessage("しゃがみ");
        }
        else if (heightDiff <= crouchThreshold && isCrouching && Mathf.Abs(verticalVelocity) < CrouchVelocity)
        {
            isCrouching = false;
            ShowMotionMessage("立ち上がり");
        }
        
        // 状態が変化したらシステムに通知
        if (wasCrouching != isCrouching)
        {
            // 中央集権型システムが設定されている場合はそちらに送信
            if (centralizedDetector != null)
            {
                centralizedDetector.SetCrouchState(isCrouching);
            }
            // 従来の個別システムにも送信（後方互換性）
            else if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetCrouchState(isCrouching);
            }
        }

        lastHeadHeight = currentHeadHeight;
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
                Debug.Log($"[CrouchDetector] モーション検知: {(isEnabled ? "有効" : "無効")} (エリア: {areaTrigger.AreaName})");
                lastMotionDetectionState = isEnabled;
            }
        }
        
        return isEnabled;
    }

}

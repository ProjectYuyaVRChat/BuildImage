using UdonSharp;
using TMPro;
using VRC.SDKBase;
using UnityEngine;

public class JumpDetector : MotionDetectorBase
{
    private bool isJumping = false;

    private float lastHeadHight = 0f;
    private bool initialized = false;
    private float JumpVelocity = 1.2f;  //　閾値

    //[SerializeField] private TextMeshProUGUI debugtext; // 子クラスでInspectorからセット

    // 外部から状態を取得するためのプロパティ
    public bool IsJumping => isJumping;
    
    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    
    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;
    [SerializeField] private DoorAreaTrigger areaTrigger;

    protected override void DetectMotion()
    {
        bool grounded = localPlayer.IsPlayerGrounded();
        float currentHeadHeight = headPos.y - basePos.y;

        if (!initialized)
        {
            lastHeadHight = currentHeadHeight;
            initialized = true;
            return;
        }

        float verticalVelocity = (currentHeadHeight - lastHeadHight) / Time.deltaTime;

        bool wasJumping = isJumping;

        if (verticalVelocity > JumpVelocity && !isJumping)
        {
            isJumping = true;
            ShowMotionMessage("ジャンプAAA");
        }
        else if (verticalVelocity > 0.1f && !isJumping)
        {
            isJumping = false;
            ShowMotionMessage("着地AAA");

        }
        else if (!grounded && !isJumping)
        {
            isJumping = true;
            ShowMotionMessage("ジャンプ");
        }
        else if (grounded && isJumping)
        {
            isJumping = false;
            ShowMotionMessage("着地");
        }
        
        // 状態が変化したらシステムに通知
        if (wasJumping != isJumping)
        {
            // 中央集権型システムが設定されている場合はそちらに送信
            if (centralizedDetector != null)
            {
                centralizedDetector.SetJumpState(isJumping);
            }
            // 従来の個別システムにも送信（後方互換性）
            else if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetJumpState(isJumping);
            }
        }
        
        lastHeadHight = currentHeadHeight;
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
                Debug.Log($"[JumpDetector] モーション検知: {(isEnabled ? "有効" : "無効")} (エリア: {areaTrigger.AreaName})");
                lastMotionDetectionState = isEnabled;
            }
        }
        
        return isEnabled;
    }
}

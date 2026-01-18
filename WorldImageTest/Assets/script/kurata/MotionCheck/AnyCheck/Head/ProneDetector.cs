using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class ProneDetector : MotionDetectorBase
{
    private bool isProne = false;
    private float proneThreshold = 0.3f;

    private bool initialized = false;
    private float proneBaseHeight = 0f; // 名前を変えて重複を避ける
    
    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    
    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;
    [SerializeField] private DoorAreaTrigger areaTrigger;
    
    // 外部から状態を取得するためのプロパティ
    public bool IsProne => isProne;

    protected override void DetectMotion()
    {
        if (!localPlayer.IsPlayerGrounded()) return;

        float currentHeight = headPos.y - basePos.y;

        if (!initialized)
        {
            proneBaseHeight = currentHeight;
            initialized = true;
            return;
        }

        float heightDiff = proneBaseHeight - currentHeight;

        bool wasProne = isProne;

        if (heightDiff > proneThreshold && !isProne)
        {
            isProne = true;
            ShowMotionMessage("伏せ");
        }
        else if (heightDiff <= proneThreshold && isProne)
        {
            isProne = false;
            ShowMotionMessage("伏せ解除");
        }
        
        // 状態が変化したらシステムに通知
        if (wasProne != isProne)
        {
            // 中央集権型システムが設定されている場合はそちらに送信
            if (centralizedDetector != null)
            {
                centralizedDetector.SetProneState(isProne);
            }
            // 従来の個別システムにも送信（後方互換性）
            else if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetProneState(isProne);
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
        
        // エリアがアクティブかつ、ローカルプレイヤーがエリア内にいる場合のみモーション検知を有効にする
        bool isAreaActive = areaTrigger.IsAreaActive;
        bool isLocalPlayerInArea = areaTrigger.IsPlayerInArea(Networking.LocalPlayer);
        bool isEnabled = isAreaActive && isLocalPlayerInArea;
        
        // デバッグ情報を表示（状態が変化した時のみ）
        if (doorGimmickSystem != null && doorGimmickSystem.showDebugInfo)
        {
            if (lastMotionDetectionState != isEnabled)
            {
                Debug.Log($"[ProneDetector] モーション検知: {(isEnabled ? "有効" : "無効")} (エリア: {areaTrigger.AreaName}, エリアアクティブ: {isAreaActive}, ローカルプレイヤー在籍: {isLocalPlayerInArea})");
                lastMotionDetectionState = isEnabled;
            }
        }
        
        return isEnabled;
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 左手を体の前に突き出したか判定する
/// （頭の位置と向きを基準に判定）
/// </summary>
public class HandForwardDetectorLeft : MotionDetectorBase
{
    [SerializeField] private float forwardThreshold = 0.3f;
    private bool isForward = false;
    private bool initialized = false;

    // 外部から状態を取得するためのプロパティ
    public bool IsForward => isForward;

    // 中央集権型モーション検出器への参照（推奨）
    [SerializeField] private CentralizedMotionDetector centralizedDetector;

    // 従来の個別参照（後方互換性のため）
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

    protected override void DetectMotion()
    {
        // プレイヤーの頭位置と向きを取得
        Vector3 headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        Quaternion headRot = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
        Vector3 leftHandPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

        // 頭を基準にしたローカル空間での手の位置
        Vector3 localHand = Quaternion.Inverse(headRot) * (leftHandPos - headPos);

        if (!initialized)
        {
            isForward = (localHand.z > forwardThreshold);
            initialized = true;
            return;
        }

        bool wasForward = isForward;

        // 前方向への突き出しを判定（頭基準のローカルZ軸）
        if (!isForward && localHand.z > forwardThreshold)
        {
            isForward = true;
            ShowMotionMessage("左手を前に突き出した");
        }
        else if (isForward && localHand.z <= forwardThreshold)
        {
            isForward = false;
            ShowMotionMessage("左手を前から戻した");
        }

        // 状態変化があったらシステムに通知
        if (wasForward != isForward)
        {
            if (centralizedDetector != null)
            {
                centralizedDetector.SetLeftHandForwardState(isForward);
            }
            else if (doorGimmickSystem != null)
            {
                doorGimmickSystem.SetLeftHandForwardState(isForward);
            }
        }

        // デバッグ表示
        if (debugText != null)
        {
            debugText.text = $"左手ローカルZ: {localHand.z:F3}\n閾値: {forwardThreshold:F3}\n状態: {(isForward ? "前" : "戻し")}";
        }
    }
}

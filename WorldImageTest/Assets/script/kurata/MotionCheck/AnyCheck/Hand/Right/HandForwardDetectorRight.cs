using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 右手を頭（プレイヤー）基準で前に突き出したか判定する
/// </summary>
public class HandForwardDetectorRight : MotionDetectorBase
{
    [SerializeField, Range(0f, 1f)]
    private float forwardThresholdRatio = 0.2f; // 判定のしきい値[m]

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

        Vector3 headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        Quaternion headRot = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
        Vector3 rightHandPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;


        // 頭を基準にしたローカル空間での手の位置
        Vector3 localHand = Quaternion.Inverse(headRot) * (rightHandPos - headPos);

        // 身長を取得して前方閾値を比率で計算
        float playerHeight = EstimatePlayerHeight(); // basePos.y〜headPos.yの2倍
        float forwardThreshold = playerHeight * forwardThresholdRatio;

       

        // 初期化
        if (!initialized)
        {
            isForward = (localHand.z > forwardThreshold);
            initialized = true;
            return;
        }

        bool wasForward = isForward;

        // 手を前に出した／戻した判定
        if (!isForward && localHand.z > forwardThreshold)
        {
            isForward = true;
            ShowMotionMessage("右手を前に突き出した");
        }
        else if (isForward && localHand.z <= forwardThreshold)
        {
            isForward = false;
            ShowMotionMessage("右手を前から戻した");
        }

        // 状態変化を通知
        if (wasForward != isForward)
        {
            if (centralizedDetector != null)
                centralizedDetector.SetRightHandForwardState(isForward);
            else if (doorGimmickSystem != null)
                doorGimmickSystem.SetRightHandForwardState(isForward);
        }

        // デバッグ出力
        if (debugText != null)
        {
            debugText.text = $"右手Z: {localHand.z:F3}\n閾値: {forwardThreshold:F3}\n状態: {(isForward ? "前" : "戻し")}";
        }
    }
}

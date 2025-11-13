using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 左手を体の前に突き出したか判定する
/// （頭の位置と向きを基準、身長に応じて閾値自動調整）
/// </summary>
public class HandForwardDetectorLeft : MotionDetectorBase
{
    [SerializeField, Range(0f, 1f)]
    private float forwardThresholdRatio = 0.2f; // 身長比での前方閾値

    private bool isForward = false;
    private bool initialized = false;

    public bool IsForward => isForward;

    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

    protected override void DetectMotion()
    {
        Vector3 headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
        Quaternion headRot = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
        Vector3 leftHandPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;

        // 頭を基準にしたローカル空間での手の位置
        Vector3 localHand = Quaternion.Inverse(headRot) * (leftHandPos - headPos);

        // 身長を取得して前方閾値を比率で計算
        float playerHeight = EstimatePlayerHeight(); // basePos.y〜headPos.yの2倍
        float forwardThreshold = playerHeight * forwardThresholdRatio;

        if (!initialized)
        {
            isForward = (localHand.z > forwardThreshold);
            initialized = true;
            return;
        }

        bool wasForward = isForward;

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

        if (wasForward != isForward)
        {
            if (centralizedDetector != null)
                centralizedDetector.SetLeftHandForwardState(isForward);
            else if (doorGimmickSystem != null)
                doorGimmickSystem.SetLeftHandForwardState(isForward);
        }

        if (debugText != null)
        {
            debugText.text = $"左手ローカルZ: {localHand.z:F3}\n閾値: {forwardThreshold:F3} (身長比)\n状態: {(isForward ? "前" : "戻し")}";
        }
    }
}

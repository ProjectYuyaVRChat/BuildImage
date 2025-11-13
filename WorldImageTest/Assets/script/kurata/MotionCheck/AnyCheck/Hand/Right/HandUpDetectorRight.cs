using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 右手が頭の高さより上かどうかを判定する
/// </summary>
public class HandUpDetectorRight : MotionDetectorBase
{
    [SerializeField] private float upThreshold = 0.05f; // 少し余裕を持たせた閾値
    private bool isUp = false;
    private bool initialized = false;

    public bool IsUp => isUp;

    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

    protected override void DetectMotion()
    {
        // 現在の頭と右手のワールド座標を取得
        Vector3 handPos = rightHandPos;
        Vector3 headPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

        float heightDiff = handPos.y - headPos.y;

        // デバッグ出力
        if (debugText != null)
        {
            debugText.text = $"右手 - 頭の高さ差: {heightDiff:F3}\n閾値: {upThreshold:F3}\n状態: {(isUp ? "上げた" : "下げた")}";
        }

        if (!initialized)
        {
            isUp = (heightDiff > upThreshold);
            initialized = true;
            return;
        }

        bool wasUp = isUp;

        if (!isUp && heightDiff > upThreshold)
        {
            isUp = true;
            ShowMotionMessage("右手を頭より上に上げた");
        }
        else if (isUp && heightDiff <= 0f)
        {
            isUp = false;
            ShowMotionMessage("右手を頭より下に戻した");
        }

        // 状態が変わったら通知
        if (wasUp != isUp)
        {
            if (centralizedDetector != null)
                centralizedDetector.SetRightHandUpState(isUp);
            else if (doorGimmickSystem != null)
                doorGimmickSystem.SetRightHandUpState(isUp);
        }
    }
}

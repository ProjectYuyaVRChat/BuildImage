using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 左手を体の上に上げたか判定する
/// </summary>
public class HandUpDetectorLeft : MotionDetectorBase
{
    [SerializeField] private float upThreshold = 0.1f;
    private bool isUp = false;
    private bool initialized = false;

    // 外部から状態を取得するためのプロパティ
    public bool IsUp => isUp;
    
    // DoorGimmickSystemへの参照
    [SerializeField] private DoorGimmickSystem doorGimmickSystem;

    protected override void DetectMotion()
    {
        Vector3 localHand = Quaternion.Inverse(baseRot) * (leftHandPos - basePos);

        // デバッグ情報を表示
        if (debugText != null)
        {
            debugText.text = $"左手位置: {localHand.y:F3}\n閾値: {upThreshold:F3}\n状態: {(isUp ? "上げた" : "下げた")}";
        }

        if (!initialized)
        {
            isUp = (localHand.y > upThreshold);
            initialized = true;
            return;
        }

        bool wasUp = isUp;

        if (!isUp && localHand.y > upThreshold)
        {
            isUp = true;
            ShowMotionMessage("左手を上に上げた");
        }
        else if (isUp && localHand.y <= upThreshold)
        {
            isUp = false;
            ShowMotionMessage("左手を上から戻した");
        }
        
        // 状態が変化したらDoorGimmickSystemに通知
        if (wasUp != isUp && doorGimmickSystem != null)
        {
            doorGimmickSystem.SetLeftHandUpState(isUp);
        }
    }
}
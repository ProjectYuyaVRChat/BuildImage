using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class ProneDetector : MotionDetectorBase
{
    private bool isProne = false;
    private float proneThreshold = 0.3f;

    private bool initialized = false;
    private float proneBaseHeight = 0f; // 名前を変えて重複を避ける
    
    // DoorGimmickSystemへの参照
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;
    
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
        
        // 状態が変化したらDoorGimmickSystemに通知
        if (wasProne != isProne && doorGimmickSystem != null)
        {
            doorGimmickSystem.SetProneState(isProne);
        }
    }
}

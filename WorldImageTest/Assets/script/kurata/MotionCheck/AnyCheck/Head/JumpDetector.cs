using UdonSharp;
using TMPro;
using VRC.SDKBase;
using UnityEngine;

public class JumpDetector : MotionDetectorBase
{
    private bool isJumping = false;

    private float aalastHeadHeight = 0f;
    private bool initialized = false;

    [SerializeField] private float aajumpThreshold = 0.08f; // ← 前フレーム差分での判定閾値（0.05〜0.12あたり）

    public bool IsJumping => isJumping;

    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;
    [SerializeField] private DoorAreaTrigger areaTrigger;

    protected override void DetectMotion()
    {
        float currentHeadHeight = headPos.y - basePos.y;
        bool grounded = localPlayer.IsPlayerGrounded();

        if (!initialized)
        {
            aalastHeadHeight = currentHeadHeight;
            initialized = true;
            return;
        }

        float diff = currentHeadHeight - aalastHeadHeight;

        bool wasJumping = isJumping;

        // ★★ 差分でジャンプ判定 ★★
        if (diff > aajumpThreshold && !isJumping)
        {
            isJumping = true;
            ShowMotionMessage("ジャンプ（差分判定）");
        }
        // ★★ 接地して差分が小さいなら着地判定 ★★
        else if (grounded && isJumping && diff < aajumpThreshold * 0.5f)
        {
            isJumping = false;
            ShowMotionMessage("着地");
        }

        // 状態変化があれば通知
        if (wasJumping != isJumping)
        {
            if (centralizedDetector != null)
                centralizedDetector.SetJumpState(isJumping);
            else if (doorGimmickSystem != null)
                doorGimmickSystem.SetJumpState(isJumping);
        }

        aalastHeadHeight = currentHeadHeight;
    }

    protected override bool IsMotionDetectionEnabled()
    {
        if (areaTrigger == null)
            return true;

        return areaTrigger.IsAreaActive;
    }
}
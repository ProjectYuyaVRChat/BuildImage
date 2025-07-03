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
    
    // DoorGimmickSystemへの参照
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;

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
        
        // 状態が変化したらDoorGimmickSystemに通知
        if (wasJumping != isJumping && doorGimmickSystem != null)
        {
            doorGimmickSystem.SetJumpState(isJumping);
        }
        
        lastHeadHight = currentHeadHeight;
    }
}

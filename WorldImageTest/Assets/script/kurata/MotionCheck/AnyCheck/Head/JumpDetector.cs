using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class JumpDetector : MotionDetectorBase
{
    private bool JunpState = false;
    private float JunpHeadHeight = 0f;
    private bool initialized = false;



    [SerializeField] private float jumpThreshold = 0.01f; // HMDジャンプ判定しきい値
    [SerializeField] private float velocityJumpThreshold = 1.2f; // VRChatジャンプ判定しきい値

    public bool IsJumping => JunpState;

    [SerializeField] private CentralizedMotionDetector centralizedDetector;
    [SerializeField] private DoorGimmickSystemNew doorGimmickSystem;
    [SerializeField] private DoorAreaTrigger areaTrigger;

    protected override void DetectMotion()
    {
        float currentHeadHeight = headPos.y;
        Vector3 vel = localPlayer.GetVelocity();
        bool grounded = localPlayer.IsPlayerGrounded();

        float a = headPos.y - basePos.y;

        if (!initialized)
        {
            JunpHeadHeight = currentHeadHeight;

            initialized = true;
            return;
        }

        float diff = currentHeadHeight - JunpHeadHeight;
        baseHeadHeight = Mathf.Min(baseHeadHeight, a);
        float b = a - baseHeadHeight;
        bool wasJumping = JunpState;

        // ---------------------------------------------------
        // ★ VRChatのジャンプボタンで上昇中 + 地面を離れた瞬間に反応
        // ---------------------------------------------------
        if (!grounded && !JunpState && vel.y > velocityJumpThreshold)
        {
            JunpState = true;
            ShowMotionMessage("ジャンプ（VRChat入力検出）");
        }

        // ---------------------------------------------------
        // ★ HMD差分ジャンプ（自然ジャンプ）
        // ---------------------------------------------------
        //if (diff >jumpThreshold && !JunpState)
        ///if(!JunpState && diff > jumpThreshold)
        if (!JunpState && diff > jumpThreshold)
        {
            JunpState = true;
            ShowMotionMessage("ジャンプ（HMD検出）");
        }

        // ---------------------------------------------------
        // ★ 着地判定（地面に触れた + 下降が終わった）
        // ---------------------------------------------------
        //if (grounded && JunpState && vel.y <= 0f)
        if (JunpState && diff < -jumpThreshold)

        {
            JunpState = false;
            ShowMotionMessage("着地");
        }

        // ---------------------------------------------------
        // ★ 状態変化通知（中央検出器 or ドアシステムへ）
        // ---------------------------------------------------
        if (wasJumping != JunpState)
        {
            if (centralizedDetector != null)
                centralizedDetector.SetJumpState(JunpState);
            else if (doorGimmickSystem != null)
                doorGimmickSystem.SetJumpState(JunpState);
        }

        JunpHeadHeight = currentHeadHeight;
    }

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
        return isAreaActive && isLocalPlayerInArea;
    }
}
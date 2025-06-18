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
        lastHeadHight = currentHeadHeight;
    }
}

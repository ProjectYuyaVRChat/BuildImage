using UdonSharp;
using TMPro;
using VRC.SDKBase;
using UnityEngine;

public class JumpDetector : MotionDetectorBase
{ 
    private bool isJumping = false;

    //[SerializeField] private TextMeshProUGUI debugtext; // 子クラスでInspectorからセット

    protected override void DetectMotion()
    {
        bool grounded = localPlayer.IsPlayerGrounded();

        if (!grounded && !isJumping)
        {
            isJumping = true;
            ShowMotionMessage("ジャンプ");
        }
        else if (grounded && isJumping)
        {
            isJumping = false;
            ShowMotionMessage("着地");
        }
    }
}

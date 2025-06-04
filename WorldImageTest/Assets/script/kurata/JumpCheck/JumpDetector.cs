using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class JumpDetector : MotionDetectorBase
{
    // ジャンプ中かどうかのフラグ
    private bool isJumping = false;

    [SerializeField] private TextMeshProUGUI debugText; // デバッグ用のテキスト表示

    protected override void DetectMotion()
    {
        // localPlayer は基底クラスで初期化済み

        // 地面にいるかどうか判定
        bool grounded = localPlayer.IsPlayerGrounded();

        if (!grounded && !isJumping)
        {
            // 地面から離れた瞬間 → ジャンプ開始
            isJumping = true;
            OnJumpStart();
        }
        else if (grounded && isJumping)
        {
            // 地面に着地した瞬間 → ジャンプ終了
            isJumping = false;
            OnJumpEnd();
        }
    }

    private void OnJumpStart()
    {
        Debug.Log("[JumpDetector] ジャンプ開始！");
        // 必要ならここにジャンプ開始時の処理を書く
        debugText.text = "IsJump"; // デバッグ用テキスト更新
    }

    private void OnJumpEnd()
    {
        Debug.Log("[JumpDetector] 着地！");
        // 必要ならここに着地時の処理を書く
        debugText.text = "Islanding"; // デバッグ用テキスト更新
    }
}

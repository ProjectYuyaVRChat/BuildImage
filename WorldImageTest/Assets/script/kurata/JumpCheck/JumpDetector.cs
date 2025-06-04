using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
public class JumpDetector : MotionDetectorBase
{
    private bool isJumping = false;

    [SerializeField] private PlayerRoleManagerSimple roleManager;
    [SerializeField] private TextMeshProUGUI globalNoticeText; // デバッグ用のテキスト表示
    [UdonSynced, FieldChangeCallback(nameof(NoticeText))] private string _noticeText;

    public string NoticeText
    {
        get => _noticeText;
        set
        {
            _noticeText = value;
            if (globalNoticeText != null)
            {
                globalNoticeText.text = value;
            }
        }
    }
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
        }
    }

    private void OnJumpStart()
    {
        string role = roleManager != null ? roleManager.GetPlayerRole(Networking.LocalPlayer) : "Unknown";
        SetGlobalNotice($"{role} がジャンプしました");
    }

    private void SetGlobalNotice(string msg)
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        NoticeText = msg;
        RequestSerialization();
    }


    public override void OnDeserialization()
    {
        NoticeText = _noticeText;
    }
}

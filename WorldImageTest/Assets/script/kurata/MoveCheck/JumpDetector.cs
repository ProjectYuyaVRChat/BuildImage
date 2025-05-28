using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class JumpDetector : UdonSharpBehaviour
{
    public JumpAnnouncer announcer;

    private bool wasGrounded;
    private bool jumpDetected = false;
    private float jumpStartY = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = Networking.LocalPlayer.GetPosition();
        wasGrounded = Networking.LocalPlayer.IsPlayerGrounded();
    }

    void Update()
    {
        var player = Networking.LocalPlayer;
        if (player == null) return;

        bool isGrounded = player.IsPlayerGrounded();
        Vector3 currentPosition = player.GetPosition();

        // 地面から離れたらジャンプ開始
        if (wasGrounded && !isGrounded)
        {
            jumpStartY = currentPosition.y;
        }

        // 空中にいて、一定以上上昇したらジャンプ確定
        if (!isGrounded && currentPosition.y > jumpStartY + 0.15f && !jumpDetected)
        {
            Debug.Log("Jump detected for player: " + player.displayName);
            announcer.ShowJumpMessage(player.displayName);
            jumpDetected = true;
        }

        // 地面に着地したらリセット
        if (isGrounded)
        {
            jumpDetected = false;
        }

        wasGrounded = isGrounded;
        lastPosition = currentPosition;
    }

}

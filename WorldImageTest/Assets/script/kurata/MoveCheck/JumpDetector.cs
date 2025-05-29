using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class JumpDetector : BaseActionDetector
{
    private bool wasGrounded;
    private float jumpStartY;
    private bool jumpDetected;

    void Start()
    {
        var player = Networking.LocalPlayer;
        if (player == null) return;

        wasGrounded = player.IsPlayerGrounded();
    }

    public override void CheckAction(VRCPlayerApi player, ActionManager manager)
    {
        Vector3 pos = player.GetPosition();
        bool isGrounded = player.IsPlayerGrounded();

        if (wasGrounded && !isGrounded)
        {
            jumpStartY = pos.y;
        }

        if (!isGrounded && pos.y > jumpStartY + 0.15f && !jumpDetected)
        {
            manager.TriggerAction("Jump", player);
            jumpDetected = true;
        }

        if (isGrounded) jumpDetected = false;
        wasGrounded = isGrounded;
    }
}

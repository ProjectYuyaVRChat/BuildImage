using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class VibrationHand : UdonSharpBehaviour
{
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        player.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.25f,1,1);
    }
}

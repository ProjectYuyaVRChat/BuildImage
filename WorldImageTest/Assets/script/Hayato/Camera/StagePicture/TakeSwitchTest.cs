using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TakeSwitchTest : UdonSharpBehaviour
{
    public PhotoTaker photoTaker;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        photoTaker.ToggleCapturing();
    }
}

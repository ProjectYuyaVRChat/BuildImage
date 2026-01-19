using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TakeDeleySwitch : UdonSharpBehaviour
{
    public PhotoTaker photoTaker;
    [SerializeField] private float delySeconds = 2f;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        SendCustomEventDelayedSeconds(nameof(Capture),delySeconds);
    }

    public void Capture()
    {
        photoTaker.ToggleCapturing();
    }
}

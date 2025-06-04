using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public abstract class MotionDetectorBase : UdonSharpBehaviour
{
    protected VRCPlayerApi localPlayer;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    void Update()
    {
        if (localPlayer == null) return;

        DetectMotion();
    }

    // 継承先で動作検出を実装する抽象メソッド
    protected abstract void DetectMotion();
}

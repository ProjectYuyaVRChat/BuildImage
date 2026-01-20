using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// 他人の視点風に追従する観戦カメラ
/// </summary>
public class PlayerViewCamera : UdonSharpBehaviour
{
    public VRCPlayerApi targetPlayer; // Inspectorでセットして
    public Vector3 offset = Vector3.zero;
    public bool rotateWithHead = true;
    public bool smooth = true;
    public float smoothSpeed = 10f;

    void Update()
    {
        VRCPlayerApi p = targetPlayer;
        if (p == null) return;

        Vector3 headPos = p.GetBonePosition(HumanBodyBones.Head);
        Quaternion headRot = p.GetBoneRotation(HumanBodyBones.Head);

        if (headPos == Vector3.zero) headPos = p.GetPosition();

        Vector3 desired = rotateWithHead ? headPos + headRot * offset : headPos + offset;

        if (smooth)
            transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * smoothSpeed);
        else
            transform.position = desired;
    }

    // 外からターゲットを設定する関数
    public void SetTarget(VRCPlayerApi newTarget)
    {
        targetPlayer = newTarget;
    }

   
}

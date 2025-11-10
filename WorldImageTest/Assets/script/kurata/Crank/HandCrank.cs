using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class HandCrank : UdonSharpBehaviour
{
    [Header("回転の中心（ハンドルの軸）")]
    public Transform rotationCenter;

    [Header("回転軸（例：Y軸なら Vector3.up）")]
    public Vector3 rotationAxis = Vector3.up;

    [Header("ハンドルの半径")]
    public float radius = 0.3f;

    private VRCPlayerApi localPlayer;
    private VRC_Pickup pickup;

    private float lastAngle;
    private float totalRotation;
    

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        pickup = GetComponent<VRC_Pickup>();
    }

    void Update()
    {
        if (pickup == null || !pickup.IsHeld) return;

        // 手（またはコントローラ）の位置
        Vector3 handPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;

        // 中心からのベクトル
        Vector3 dir = handPos - rotationCenter.position;

        // 指定軸に対して垂直な平面に投影（＝円周上の位置）
        Vector3 projected = Vector3.ProjectOnPlane(dir, rotationAxis).normalized * radius;

        // 円周上に限定したハンドルの位置
        Vector3 targetPos = rotationCenter.position + projected;

        // 実際にハンドルをその位置に移動
        transform.position = targetPos;

        // 回転を軸方向に合わせる（見た目回転）
        transform.LookAt(rotationCenter.position, rotationAxis);

         // 角度を算出して回転量を加算
    Vector3 referenceDir = Vector3.ProjectOnPlane(Vector3.forward, rotationAxis).normalized;
    float angle = Vector3.SignedAngle(referenceDir, projected, rotationAxis);
    float delta = Mathf.DeltaAngle(lastAngle, angle);
    totalRotation += delta;
    lastAngle = angle;

    // 例：1回転で100度進むと光る
    if (Mathf.Abs(totalRotation) >= 360f)
    {
        Debug.Log("1回転した！");
        totalRotation = 0;
    }
    }
}

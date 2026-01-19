using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NearOnlyPickup : UdonSharpBehaviour
{
    public VRC_Pickup pickup;
    public float maxGrabDistance = 0.3f; // ここで近距離に設定

    void Update()
    {
        var localPlayer = Networking.LocalPlayer;
        if (!pickup) return;

        // 既に掴まれている場合は無視
        if (pickup.IsHeld) return;

        // プレイヤーの手との距離の計算
        Vector3 handPos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
        float distance = Vector3.Distance(handPos, pickup.transform.position);

        // 距離が遠すぎる場合は Pickup を無効化
        pickup.pickupable = distance <= maxGrabDistance;
    }
}

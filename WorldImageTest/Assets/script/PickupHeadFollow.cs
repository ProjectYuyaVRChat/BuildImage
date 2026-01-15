using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class PickupHeadFollow : UdonSharpBehaviour
{
    public Vector3 offset = new Vector3(0f, 0.3f, 0f);

    private bool isHeld;
    private VRCPlayerApi localPlayer;
    private VRC_Pickup pickup;

    void Start()
    {
        pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
    }

    public override void OnPickup()
    {
        isHeld = true;
        localPlayer = Networking.LocalPlayer;

        // 手から解放する（重要）
        if (pickup != null)
        {
            pickup.Drop();
        }
    }

    private void LateUpdate()
    {
        if (!isHeld || localPlayer == null) return;

        var head = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

        transform.position = head.position + head.rotation * offset;
        transform.rotation = head.rotation;
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(VRC_Pickup))]
public class HeadFollowOncePickup : UdonSharpBehaviour
{
    [Header("頭からのオフセット")]
    public Vector3 positionOffset = new Vector3(0f, 0f, 0.3f);
    public Vector3 rotationOffset = Vector3.zero;

    private VRCPlayerApi ownerPlayer;
    private bool isActivated = false;

　　[Header("レーザーの設定")]
    public LineRenderer laserLine;
    public float maxLaserDistance = 100f;
    public LayerMask collisionLayers;

    public override void OnPickup()
    {
        // すでに誰かが装着していたら何もしない
        if (isActivated) return;

        ownerPlayer = Networking.LocalPlayer;
        Networking.SetOwner(ownerPlayer, gameObject);

        isActivated = true;
    }

    public override void OnDrop()
    {
        // Dropを無効化（頭追従は続く）
    }

    void Update()
    {
        if (!isActivated) return;
        if (!Networking.IsOwner(gameObject)) return;

        VRCPlayerApi.TrackingData head =
            ownerPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

        transform.position =
            head.position + head.rotation * positionOffset;

        transform.rotation =
            head.rotation * Quaternion.Euler(rotationOffset);
    }
    public override void PostLateUpdate()
    {
        // --- 2. レーザー発射処理 ---
        
        // オブジェクトの回転が変われば、transform.forwardも自動的に変わるため
        // 以下のロジックはそのままで、レーザーの角度も追従します。

        // 元のロジック（-transform.forward方向）
        Vector3 origin = transform.position + 0.1f * transform.forward;
        Vector3 direction = transform.forward;

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, maxLaserDistance, collisionLayers))
        {
            if (laserLine != null)
            {
                laserLine.SetPosition(0, origin);
                laserLine.SetPosition(1, hit.point);
            }
        }
        else
        {
            if (laserLine != null)
            {
                Vector3 endPosition = origin + direction * maxLaserDistance;
                laserLine.SetPosition(0, origin);
                laserLine.SetPosition(1, endPosition);
            }
        }
    }
   public void OnTriggerEnter(Collider other)
{
    if (other == null) return;
    if (!isActivated) return;
    if (!Networking.IsOwner(gameObject)) return;

    if (other.gameObject.name == "DropArea")
    {
        // 頭追従を解除
        isActivated = false;

        // Pickup を強制ドロップ
        VRC_Pickup pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
        if (pickup != null)
        {
            pickup.Drop();
        }

        // 念のため少し前に出す（任意）
        transform.position += transform.forward * 0.3f;
    }
}

}

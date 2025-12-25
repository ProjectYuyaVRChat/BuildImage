using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HeadFixedPickup : UdonSharpBehaviour
{
    [Header("頭固定の設定")]
    [Tooltip("頭の中心からどれくらい離すか (Yをプラスにすると頭上)")]
    public Vector3 positionOffset = new Vector3(0f, 0.3f, 0f);

    [Tooltip("頭に対する回転のズレ (度数法 X, Y, Z)")]
    public Vector3 rotationOffset = Vector3.zero; // ⭐ 追加: 回転調整用

    [Tooltip("頭の回転に追従させるか")]
    public bool syncRotation = true;

    [Header("レーザーの設定")]
    public LineRenderer laserLine;
    public float maxLaserDistance = 100f;
    public LayerMask collisionLayers;

    private VRC_Pickup pickup;

    void Start()
    {
        pickup = (VRC_Pickup)GetComponent(typeof(VRC_Pickup));
    }

    public override void PostLateUpdate()
    {
        // --- 1. 頭への固定処理 ---
        
        if (pickup != null && pickup.IsHeld)
        {
            VRCPlayerApi owner = pickup.currentPlayer;
            if (owner != null)
            {
                // 頭の位置と回転を取得
                Vector3 headPos = owner.GetBonePosition(HumanBodyBones.Head);
                Quaternion headRot = owner.GetBoneRotation(HumanBodyBones.Head);

                // 位置を上書き (頭の位置 + オフセット)
                transform.position = headPos + (headRot * positionOffset);

                // 回転を上書き (頭の回転 × 指定したオフセット回転)
                if (syncRotation)
                {
                    // ⭐ Quaternion.Eulerを使ってVector3の角度を回転情報に変換し、合成します
                    Quaternion offsetRot = Quaternion.Euler(rotationOffset);
                    transform.rotation = headRot * offsetRot;
                }
            }
        }

        // --- 2. レーザー発射処理 ---
        
        // オブジェクトの回転が変われば、transform.forwardも自動的に変わるため
        // 以下のロジックはそのままで、レーザーの角度も追従します。

        // 元のロジック（-transform.forward方向）
        Vector3 origin = transform.position + 0.1f * -transform.forward;
        Vector3 direction = -transform.forward;

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
}

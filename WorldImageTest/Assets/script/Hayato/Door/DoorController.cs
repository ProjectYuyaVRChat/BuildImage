using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DoorController : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("掴むための透明なハンドル")]
    public VRC_Pickup invisibleHandle;
    [Tooltip("見た目のノブ（VisualKnobを指定してください）")]
    public Transform handleAnchor;

    [Header("Settings")]
    public float minAngle = 0f;
    public float maxAngle = 90f;
    public float closingSpeed = 2.0f;

    private Vector3 _baseVector;
    private float _gripOffsetAngle = 0f;
    private float _currentDoorAngle = 0f;

    void Start()
    {
        CalculateBaseVector();
    }

    void CalculateBaseVector()
    {
        // 基準ベクトル計算
        if (handleAnchor == null) return;
        Vector3 dir = handleAnchor.position - transform.position;
        if (transform.parent != null) dir = transform.parent.InverseTransformVector(dir);
        dir.y = 0;
        _baseVector = dir.normalized;
    }

    public override void OnPickup()
    {
        // 掴んだ瞬間の角度差を記録
        float handAngle = GetAngleFromTarget(invisibleHandle.transform.position);
        _gripOffsetAngle = handAngle - _currentDoorAngle;
    }

    public override void OnDrop()
    {
        // 離した瞬間、即座に位置をリセット
        ResetHandlePosition();
    }

    void Update()
    {
        // 持っている時：ドアを動かす
        if (invisibleHandle.IsHeld)
        {
            MoveDoorByHand();
        }
        // 持っていない時：ドアを閉じる処理のみ（位置合わせはLateUpdateに任せる）
        else
        {
            if (closingSpeed > 0)
            {
                _currentDoorAngle = Mathf.MoveTowards(_currentDoorAngle, minAngle, closingSpeed * Time.deltaTime * 100f);
                ApplyRotation(_currentDoorAngle);
            }
        }
    }

    // LateUpdateは全てのアニメーションや移動が終わった後に呼ばれる
    // ここで位置合わせをすることで、ドアが動いた後の位置に確実にハンドルを持ってくる
    void LateUpdate()
    {
        // 持っていないなら、必ず見た目の位置にワープさせる
        if (!invisibleHandle.IsHeld)
        {
            ResetHandlePosition();
        }
    }

    void MoveDoorByHand()
    {
        float currentHandAngle = GetAngleFromTarget(invisibleHandle.transform.position);
        float targetAngle = currentHandAngle - _gripOffsetAngle;
        _currentDoorAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);
        ApplyRotation(_currentDoorAngle);
    }

    void ResetHandlePosition()
    {
        if (handleAnchor != null && invisibleHandle != null)
        {
            invisibleHandle.transform.position = handleAnchor.position;
            invisibleHandle.transform.rotation = handleAnchor.rotation;
            
            // 物理的な速度も殺しておくと安全
            Rigidbody rb = invisibleHandle.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    float GetAngleFromTarget(Vector3 targetPos)
    {
        Vector3 targetDir = targetPos - transform.position;
        if (transform.parent != null) targetDir = transform.parent.InverseTransformVector(targetDir);
        targetDir.y = 0;
        return Vector3.SignedAngle(_baseVector, targetDir, Vector3.up);
    }

    void ApplyRotation(float angle)
    {
        transform.localRotation = Quaternion.Euler(0, angle, 0);
    }
}

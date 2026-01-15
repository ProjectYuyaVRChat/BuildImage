using UdonSharp;
using UnityEngine;

public class SyncRotationY : UdonSharpBehaviour
{
    [Tooltip("Y回転を同期したい対象")]
    public Transform[] syncTargets;

    private float _previousY;

    void Start()
    {
        _previousY = NormalizeAngle(transform.rotation.eulerAngles.y);
    }

    void Update()
    {
        float currentY = NormalizeAngle(transform.rotation.eulerAngles.y);
        float deltaY = currentY - _previousY;

        // -180 ～ 180 に収める（急回転防止）
        deltaY = NormalizeAngle(deltaY);

        if (syncTargets != null)
        {
            foreach (var target in syncTargets)
            {
                if (target == null) continue;

                // 現在の回転を取得
                Vector3 euler = target.rotation.eulerAngles;

                // Yだけ加算
                float newY = NormalizeAngle(euler.y + deltaY);

                target.rotation = Quaternion.Euler(
                    euler.x,
                    newY,
                    euler.z
                );
            }
        }

        _previousY = currentY;
    }

    /// <summary>
    /// 角度を -180 ～ 180 に正規化
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}

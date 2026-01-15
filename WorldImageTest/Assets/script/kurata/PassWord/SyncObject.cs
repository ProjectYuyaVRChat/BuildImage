using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// SyncObject は、アタッチ先の Transform の動きを
/// 配列で指定した他オブジェクト（m_SyncTargets）に相対的に同期するお
/// 持って動かしたいならPickUpレイヤーつけるの忘れずにね♪
/// </summary>
public class SyncObject : UdonSharpBehaviour
{
    [Tooltip("同期先の Transform を複数設定")]
    public Transform[] m_SyncTargets;

    [Header("Position制限設定")]
    [Tooltip("X軸の移動をロック")]
    public bool lockPositionX = false;
    [Tooltip("Y軸の移動をロック")]
    public bool lockPositionY = false;
    [Tooltip("Z軸の移動をロック")]
    public bool lockPositionZ = false;

    [Header("Rotation制限設定")]
    [Tooltip("X軸の回転をロック")]
    public bool lockRotationX = false;
    [Tooltip("Y軸の回転をロック")]
    public bool lockRotationY = false;
    [Tooltip("Z軸の回転をロック")]
    public bool lockRotationZ = false;

    private Vector3 _previousPosition;
    private Quaternion _previousRotation;

    private void Start()
    {
        _previousPosition = transform.position;
        _previousRotation = transform.rotation;

        // 同期ループ対象に自身が混ざっていないかチェック
        if (m_SyncTargets != null)
        {
            foreach (var target in m_SyncTargets)
            {
                if (target == null) continue;
                if (target.GetComponent<SyncObject>() != null)
                {
                    Debug.LogWarning("SyncObjectの同期先「{target.name}」にも同スクリプトが付いてるけど大丈夫そ？");
                    break;
                }
            }
        }
    }

    private void Update()
    {
        //差分計算
        Vector3 positionDelta = transform.position - _previousPosition;
        Quaternion rotationDelta = transform.rotation * Quaternion.Inverse(_previousRotation);

        // Position制限を適用
        Vector3 restrictedPositionDelta = ApplyPositionRestrictions(positionDelta);

        // Rotation制限を適用
        Quaternion restrictedRotationDelta = ApplyRotationRestrictions(rotationDelta);

        if (m_SyncTargets != null)
        {
            foreach (var target in m_SyncTargets)
            {
                if (target == null) continue;

                // スケール比を計算
                Vector3 scaleA = transform.lossyScale;
                Vector3 scaleB = target.lossyScale;
                Vector3 scaleRatio = Vector3.one;
                // 0除算防止
                scaleRatio.x = (Mathf.Abs(scaleA.x) > 1e-6f) ? (scaleB.x / scaleA.x) : 1f;
                scaleRatio.y = (Mathf.Abs(scaleA.y) > 1e-6f) ? (scaleB.y / scaleA.y) : 1f;
                scaleRatio.z = (Mathf.Abs(scaleA.z) > 1e-6f) ? (scaleB.z / scaleA.z) : 1f;

                // スケール比で補正した移動量（制限適用後）
                Vector3 scaledDelta = new Vector3(
                    restrictedPositionDelta.x * scaleRatio.x,
                    restrictedPositionDelta.y * scaleRatio.y,
                    restrictedPositionDelta.z * scaleRatio.z
                );

                // 位置を相対移動（スケール補正後）
                target.position += scaledDelta;

                // 回転は参照元の現在値を直接コピーし、ロックした軸だけを維持
                ApplyRotationWithRestrictions(target);
            }
        }

        _previousPosition = transform.position;
        _previousRotation = transform.rotation;
    }

    /// <summary>
    /// Positionの制限を適用する
    /// </summary>
    /// <param name="positionDelta">元の移動量</param>
    /// <returns>制限適用後の移動量</returns>
    private Vector3 ApplyPositionRestrictions(Vector3 positionDelta)
    {
        Vector3 restrictedDelta = positionDelta;

        if (lockPositionX)
            restrictedDelta.x = 0f;
        if (lockPositionY)
            restrictedDelta.y = 0f;
        if (lockPositionZ)
            restrictedDelta.z = 0f;

        return restrictedDelta;
    }

    /// <summary>
    /// Rotationの制限を適用する（旧メソッド、互換性のため残す）
    /// </summary>
    /// <param name="rotationDelta">元の回転量</param>
    /// <returns>制限適用後の回転量</returns>
    private Quaternion ApplyRotationRestrictions(Quaternion rotationDelta)
    {
        // オイラー角に変換して軸ごとに制限を適用（より正確）
        Vector3 eulerDelta = rotationDelta.eulerAngles;
        
        // オイラー角を-180～180度の範囲に正規化
        eulerDelta.x = NormalizeAngle(eulerDelta.x);
        eulerDelta.y = NormalizeAngle(eulerDelta.y);
        eulerDelta.z = NormalizeAngle(eulerDelta.z);

        // ロックされた軸の角度を0にする
        if (lockRotationX) eulerDelta.x = 0f;
        if (lockRotationY) eulerDelta.y = 0f;
        if (lockRotationZ) eulerDelta.z = 0f;

        // クォータニオンに戻す
        return Quaternion.Euler(eulerDelta);
    }

    /// <summary>
    /// ターゲットの回転に制限を適用して更新する（参照元の現在値を直接コピー）
    /// </summary>
    /// <param name="target">同期先のTransform</param>
    private void ApplyRotationWithRestrictions(Transform target)
    {
        // 参照元の現在の回転をオイラー角に変換
        Vector3 sourceEuler = transform.rotation.eulerAngles;
        Vector3 sourceEulerNormalized = new Vector3(
            NormalizeAngle(sourceEuler.x),
            NormalizeAngle(sourceEuler.y),
            NormalizeAngle(sourceEuler.z)
        );

        // ターゲットの現在の回転をオイラー角に変換
        Vector3 targetEuler = target.rotation.eulerAngles;
        Vector3 targetEulerNormalized = new Vector3(
            NormalizeAngle(targetEuler.x),
            NormalizeAngle(targetEuler.y),
            NormalizeAngle(targetEuler.z)
        );

        // ロックされていない軸は参照元の値、ロックされた軸はターゲットの現在値を維持
        Vector3 newEuler = new Vector3(
            lockRotationX ? targetEulerNormalized.x : sourceEulerNormalized.x,
            lockRotationY ? targetEulerNormalized.y : sourceEulerNormalized.y,
            lockRotationZ ? targetEulerNormalized.z : sourceEulerNormalized.z
        );

        // クォータニオンに戻して設定
        target.rotation = Quaternion.Euler(newEuler);
    }

    /// <summary>
    /// 角度を-180～180度の範囲に正規化
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    #region 制限設定メソッド

    /// <summary>
    /// Position制限を設定する
    /// </summary>
    /// <param name="lockX">X軸をロック</param>
    /// <param name="lockY">Y軸をロック</param>
    /// <param name="lockZ">Z軸をロック</param>
    public void SetPositionRestrictions(bool lockX, bool lockY, bool lockZ)
    {
        lockPositionX = lockX;
        lockPositionY = lockY;
        lockPositionZ = lockZ;
    }

    /// <summary>
    /// Rotation制限を設定する
    /// </summary>
    /// <param name="lockX">X軸をロック</param>
    /// <param name="lockY">Y軸をロック</param>
    /// <param name="lockZ">Z軸をロック</param>
    public void SetRotationRestrictions(bool lockX, bool lockY, bool lockZ)
    {
        lockRotationX = lockX;
        lockRotationY = lockY;
        lockRotationZ = lockZ;
    }

    /// <summary>
    /// すべての制限を解除する
    /// </summary>
    public void ClearAllRestrictions()
    {
        lockPositionX = false;
        lockPositionY = false;
        lockPositionZ = false;
        lockRotationX = false;
        lockRotationY = false;
        lockRotationZ = false;
    }

    /// <summary>
    /// 現在の制限状態を取得する
    /// </summary>
    /// <returns>制限状態の文字列</returns>
    public string GetRestrictionStatus()
    {
        string status = "Position制限: ";
        status += lockPositionX ? "X" : "-";
        status += lockPositionY ? "Y" : "-";
        status += lockPositionZ ? "Z" : "-";

        status += " | Rotation制限: ";
        status += lockRotationX ? "X" : "-";
        status += lockRotationY ? "Y" : "-";
        status += lockRotationZ ? "Z" : "-";

        return status;
    }

    #endregion
}

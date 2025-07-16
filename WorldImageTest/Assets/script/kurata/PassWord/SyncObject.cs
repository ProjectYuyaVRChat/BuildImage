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

                // スケール比で補正した移動量
                Vector3 scaledDelta = new Vector3(
                    positionDelta.x * scaleRatio.x,
                    positionDelta.y * scaleRatio.y,
                    positionDelta.z * scaleRatio.z
                );

                // 位置を相対移動（スケール補正後）
                target.position += scaledDelta;

                // 回転はそのまま
                target.rotation = rotationDelta * target.rotation;//*=はだめよローカルになるから
            }
        }

        _previousPosition = transform.position;
        _previousRotation = transform.rotation;
    }
}

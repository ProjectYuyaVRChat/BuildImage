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

                // 位置を相対移動
                target.position += positionDelta;

                // 回転をワールド空間ベースで同期
                target.rotation = rotationDelta * target.rotation;//*=はだめよローカルになるから
            }
        }

        _previousPosition = transform.position;
        _previousRotation = transform.rotation;
    }
}

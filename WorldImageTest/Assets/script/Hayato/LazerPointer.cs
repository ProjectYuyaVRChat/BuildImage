using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using LayerMask = UnityEngine.LayerMask;
using LineRenderer = UnityEngine.LineRenderer;

public class LazerPointer : UdonSharpBehaviour
{
    // Unity Inspectorから設定する変数
    public LineRenderer laserLine; 
    public float maxLaserDistance = 100f; // レーザーの最大距離
    public LayerMask collisionLayers; // 衝突させたいレイヤーを設定
    
    void Update()
    {
        Vector3 origin = transform.position + 0.1f * -transform.forward;
        // ⭐ この行が、オブジェクトのローカル-Z軸方向を決定しています。
        Vector3 direction = -transform.forward; 

        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, maxLaserDistance, collisionLayers))
        {
            Debug.Log("hitを確認");
            // 衝突した場合: レーザーの終点を衝突点に設定
            if (laserLine != null)
            {
                laserLine.SetPosition(0, origin);
                laserLine.SetPosition(1, hit.point);
            }
        }
        else
        {
            // 衝突しなかった場合: レーザーの終点を最大距離先に設定
            if (laserLine != null)
            {
                Vector3 endPosition = origin + direction * maxLaserDistance;
                laserLine.SetPosition(0, origin);
                laserLine.SetPosition(1, endPosition);
            }
        }
    }
}


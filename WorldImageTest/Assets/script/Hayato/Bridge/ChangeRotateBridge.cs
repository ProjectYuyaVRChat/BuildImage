using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ChangeRotateBridge : UdonSharpBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotateLimit = 30f;
    
    // 💡 修正ポイント1: 初期回転を格納するためのフィールド
    private Quaternion initialLocalRotation;

    void Start()
    {
        // 💡 修正ポイント2: Start() でオブジェクトが持つ現在のローカル回転を取得・保存する
        initialLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        // 1. Z軸の新しいオイラー角を計算する (ここは変更なし)
        float newRotationZ = Mathf.Sin(Time.time * speed) * rotateLimit;
        
        // 2. 目的のZ軸回転のみを持つQuaternionを作成する
        //    XとYは0にして、Zだけを newRotationZ にします
        Quaternion zRotation = Quaternion.Euler(0, 0, newRotationZ);
        
        // 3. 💡 修正ポイント3: 保存しておいた初期回転に、計算したZ軸回転を**合成（乗算）**して設定する
        transform.localRotation = initialLocalRotation * zRotation;
    }
}

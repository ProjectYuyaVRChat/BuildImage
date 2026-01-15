using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Mogura : UdonSharpBehaviour
{
    [Header("設定")]
    [SerializeField] private float upperLimit = 0f;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float downSpeed = 0.7f;
    [SerializeField] private float waitSeconds = 0.2f;

    [Header("参照")]
    // ここにMoguraGameManagerを割り当てる必要があります
    [SerializeField] private MoguraGameManager gameManager; 

    private CapsuleCollider mogura;
    private bool move = false;
    [UdonSynced] private bool up = true;
    private Vector3 startPosition;
    
    private void Start()
    {
        mogura = this.GetComponent<CapsuleCollider>();
        mogura.enabled = false;
        startPosition = transform.position;
    }

    private void Update()
    {
        if (move)
        {
            if (up)
            {
                // 上昇中
                transform.position += Vector3.up * speed * Time.deltaTime;

                if (Networking.IsOwner(gameObject))
                {
                    float currentDistance = transform.position.y - startPosition.y;

                    if (currentDistance >= upperLimit)
                    {
                        up = false; // 下降へ切り替え
                        RequestSerialization();
                    }
                }
            }
            else
            {
                Wait();
            }
        }
    }

    private void Wait()
    {
        SendCustomEventDelayedSeconds(nameof(Down), waitSeconds);
    }

    public void Down()
    {
        transform.position += Vector3.down * downSpeed * Time.deltaTime;
                
        if (Networking.IsOwner(gameObject))
        {
            float currentDistance = transform.position.y - startPosition.y;
                
            // スタート位置に戻ったら停止
            if (currentDistance <= 0)
            {
                move = false; 
                transform.position = startPosition;
                RequestSerialization();
            }
        }
    }

    // Managerから呼ばれてモグラが出現する関数
    public void MoveMogura()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        mogura.enabled = true; // 当たり判定を有効化
        move = true;
        up = true;
        RequestSerialization();
    }
    
    // ハンマーとの当たり判定
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "HummerHead")
        {
            // オーナー権限を取得
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            // 当たり判定を無効化（二重計上防止のため）
            mogura.enabled = false;
            
            // 下降モードに移行
            up = false;
            RequestSerialization();

            // --- 追加部分：マネージャーにヒット通知を送る ---
            if (gameManager != null)
            {
                gameManager.OnMoleHit();
            }
        }
    }
}

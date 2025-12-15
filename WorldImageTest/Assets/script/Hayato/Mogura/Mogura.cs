using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Mogura : UdonSharpBehaviour
{
    [SerializeField] private float upperLimit = 0f;
    [SerializeField] private float speed = 5f;

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
                transform.position += Vector3.up * speed * Time.deltaTime;

                if (Networking.IsOwner(gameObject))
                {
                    float currentDistance = transform.position.y - startPosition.y;

                    if (currentDistance >= upperLimit)
                    {
                        up = false; // 下降へ切り替え
                        RequestSerialization(); // 変更を全員に送信
                    }
                }
            }
            else
            {
                transform.position += Vector3.down * speed * Time.deltaTime;
                
                if (Networking.IsOwner(gameObject))
                {
                    float currentDistance = transform.position.y - startPosition.y;
                
                    if (currentDistance <= 0)
                    {
                        move = false; // 停止
                        transform.position = startPosition; // 位置ズレ補正
                        RequestSerialization(); // 変更を全員に送信
                    }
                }
            }
        }
    }

    //ここを他から呼んで稼働
    public void MoveMogura()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        mogura.enabled = true;
        move = true;
        up = true;
        RequestSerialization();
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "HummerHead")
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            mogura.enabled = false;
            up = false;
            RequestSerialization();
        }
    }
}

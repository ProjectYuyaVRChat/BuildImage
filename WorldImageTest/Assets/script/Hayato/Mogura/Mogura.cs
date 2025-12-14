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
    private bool up = true;
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

                float currentDistance = transform.position.y - startPosition.y;

                if (currentDistance >= upperLimit)
                {
                    up = false;
                }
            }
            else
            {
                transform.position += Vector3.down * speed * Time.deltaTime;
                float currentDistance = transform.position.y - startPosition.y;
                
                if (currentDistance <= 0)
                {
                    move = false;
                }
            }
            
        }
    }

    //ここを他から呼んで稼働
    public void MoveMogura()
    {
        mogura.enabled = true;
        move = true;
        up = true;
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "HummerHead")
        {
            mogura.enabled = false;
            up = false;
        }
    }
}

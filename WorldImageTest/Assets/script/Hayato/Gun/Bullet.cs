using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Bullet : UdonSharpBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    private void Start()
    {
        SendCustomEventDelayedSeconds(nameof(DestroyBullet), lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        DestroyBullet();
    }

    public void DestroyBullet()
    {
        if (Networking.IsOwner(gameObject))
        {
            Destroy(this.gameObject);
        }
    }
}

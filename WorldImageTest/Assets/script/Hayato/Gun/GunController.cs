using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GunController : UdonSharpBehaviour
{
    [SerializeField] private float fireRate = 0.25f;
    private float lastShotTime = 0f;
    [UdonSynced] private bool shoot = false;
    public ShootBullet bullet;

    private void Update()
    {
        if (shoot)
        {
            shoot = false;
            bullet.syncShoot();
        }
    }

    public override void OnPickupUseDown()
    {
        if (Time.time < lastShotTime + fireRate)
        {
            return;
        }
        
        lastShotTime = Time.time;

        shoot = true;
        RequestSerialization();
    }
}

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ShootBullet : UdonSharpBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 75;
    private float lastShotTime = 0f;
    [UdonSynced] private bool shoot = false;

    private void Update()
    {
        if (shoot)
        {
            shoot = false;
            Shoot();
        }
    }

    public void syncShoot()
    {
        shoot = true;
        RequestSerialization();
    }

    private void Shoot()
    {
        if (Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        GameObject newBullet = Instantiate(bulletPrefab);
        
        Networking.SetOwner(Networking.LocalPlayer, bulletPrefab);
        
        bulletPrefab.transform.SetPositionAndRotation(transform.position, transform.rotation);
        
        Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
        bulletRB.velocity = transform.forward * bulletSpeed;
    }
}

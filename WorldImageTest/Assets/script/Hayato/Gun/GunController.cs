using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GunController : UdonSharpBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float bulletSpeed = 100f;
    [SerializeField] private float fireRate = 0.25f;
    private float lastShotTime = 0f;

    public override void OnPickupUseDown()
    {
        if (Time.time < lastShotTime + fireRate)
        {
            return;
        }
        
        lastShotTime = Time.time;

        Shoot();
    }

    private void Shoot()
    {
        if (Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        GameObject newBullet = Instantiate(bulletPrefab);
        
        Networking.SetOwner(Networking.LocalPlayer, newBullet);
        
        newBullet.transform.SetPositionAndRotation(muzzle.position, muzzle.rotation);
        
        Rigidbody bulletRB = newBullet.GetComponent<Rigidbody>();
        bulletRB.velocity = muzzle.forward * bulletSpeed;
    }
}

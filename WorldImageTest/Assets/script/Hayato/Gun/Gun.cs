using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Gun : UdonSharpBehaviour
{
    [Header("設定")]
    [SerializeField] private GameObject bulletPrefab; // 弾のプレハブ
    [SerializeField] private Transform firePoint;     // 弾が出る場所（Muzzle）
    [SerializeField] private float bulletSpeed = 75f;
    [SerializeField] private float fireRate = 0.25f;  // 連射間隔
    
    [UdonSynced(UdonSyncMode.None)] private int _syncShotCount;
    
    private int _localShotCount;
    
    private float _lastShotTime = 0f;

    public override void OnPickupUseDown()
    {
        if (Time.time < _lastShotTime + fireRate) return;
        
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(TriggerShoot));
    }

    public void TriggerShoot()
    {
        _lastShotTime = Time.time;
        
        ShootLocal();
    }
    
    private void ShootLocal()
    {
        GameObject newBullet = Instantiate(bulletPrefab);
        
        newBullet.transform.SetPositionAndRotation(firePoint.position, firePoint.rotation);
        
        newBullet.SetActive(true);
        
        Rigidbody rb = newBullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.velocity = firePoint.forward * bulletSpeed;
        }
        Destroy(newBullet, 3.0f);
    }
}
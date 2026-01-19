using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AirPumpHandle : UdonSharpBehaviour
{
    [Header("このオブジェクトに付いているPickupを指定")]
    public VRC_Pickup pickup;
    
    [Header("上下するパーツを登録")]
    public Transform pumpPiston;
    
    [Header("rigidbodyを登録(ROCK)")]
    public Rigidbody pumpRigidbody;

    private Vector3 basePos;

    void Start()
    {
       if (pumpPiston != null)
          basePos = pumpPiston.localPosition;
    }
    void Update()
    {
        if (pickup == null) return;

        if (pickup.IsHeld)
        {
           // 両手で掴んでいる場合、Y以外を固定
            pumpRigidbody.constraints = RigidbodyConstraints.FreezePositionX |
                                        RigidbodyConstraints.FreezePositionZ |
                                        RigidbodyConstraints.FreezeRotation;

        }
        else
        {
             pumpRigidbody.constraints = RigidbodyConstraints.FreezePosition |
                                         RigidbodyConstraints.FreezeRotation;
           // 元の位置に戻すやつ
           pumpPiston.localPosition = Vector3.Lerp(pumpPiston.localPosition, basePos, Time.deltaTime * 5f);
        }
    }
}

using UdonSharp;
using Unity.VisualScripting;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HapticOnCollision : UdonSharpBehaviour
{
    [Header("振動の設定")]
    [SerializeField] private float duration = 0.2f; // 振動する時間（秒）
    [SerializeField] private float amplitude = 0.5f; // 振幅（強さ 0.0〜1.0）
    [SerializeField] private float frequency = 0.1f; // 周波数（0.0〜1.0）
    
    [SerializeField] private VRC_Pickup pickup;
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Mogura")
        {
            TriggerHaptic();
        }
    }
    
    private void TriggerHaptic()
    {
        if (pickup != null && pickup.IsHeld)
        {
            // 握っているプレイヤーを取得
            VRCPlayerApi player = pickup.currentPlayer;
            if (player != null && player.isLocal)
            {
                // どちらの手で持っているか判定して振動させる
                VRC_Pickup.PickupHand hand = pickup.currentHand;
                player.PlayHapticEventInHand(hand, duration, amplitude, frequency);
            }
        }
    }
}

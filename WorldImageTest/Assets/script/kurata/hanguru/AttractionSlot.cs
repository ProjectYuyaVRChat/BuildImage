using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AttractionSlot : UdonSharpBehaviour
{
    [Header("正解オブジェクト")]
    public GameObject correctObject;   // この場所の正解オブジェクト
    public float pullSpeed = 2f;
    public float rotateSpeed = 180f;
    
    [HideInInspector] public bool isPlaced = false;
    private SlotManager manager;

    public void SetManager(SlotManager slotManager)
    {
        manager = slotManager;
    }

    private void OnTriggerStay(Collider other)
    {
        if (isPlaced) return;

        AttractableObject attractable = other.GetComponent<AttractableObject>();
        if (attractable == null) return;

        // 引き寄せ
        other.transform.position = Vector3.MoveTowards(
            other.transform.position,
            transform.position,
            pullSpeed * Time.deltaTime
        );

        // 回転補正
        other.transform.rotation = Quaternion.RotateTowards(
            other.transform.rotation,
            Quaternion.identity,
            rotateSpeed * Time.deltaTime
        );

        // ここで判定
        if (Vector3.Distance(other.transform.position, transform.position) < 0.05f)
        {
            if (other.gameObject == correctObject)
            {
                Debug.Log($"✔ {gameObject.name} に正しいオブジェクト {other.name} がセットされた");

                isPlaced = true;

                // 完全吸着固定
                other.transform.position = transform.position;
                other.transform.rotation = Quaternion.identity;

                // マネージャーへ報告
                if (manager != null) manager.CheckAllSlots();
            }
        }
    }
}

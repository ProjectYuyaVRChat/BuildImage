
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AttractionZone : UdonSharpBehaviour
{
    [Header("吸い寄せる中心点")]
    //public Transform centerPoint;

    [Header("引き寄せ速度")]
    public float pullSpeed = 2f;

    [Header("正解のオブジェクト")]
    public GameObject correctObject; //正解のオブジェクト
     
    [Header("消すオブジェクト")]
    public GameObject objectToHide;

    private void OnTriggerStay(Collider other)
    {
       AttractableObject attractable = other.GetComponent<AttractableObject>();
        if (attractable == null) return;
       
       // 吸い寄せ処理
        other.transform.position = Vector3.MoveTowards(
            other.transform.position,
           // centerPoint.position,
           transform.position,
            pullSpeed * Time.deltaTime
        );

        // 距離が近くなったら正解判定
        if (Vector3.Distance(other.transform.position, transform.position) < 0.05f)
        {
            // 正解のオブジェクトだった場合
            if (other.gameObject == correctObject)
            {
                if (objectToHide != null)
                {
                    objectToHide.SetActive(false);
                }
            }
        }
    }
}

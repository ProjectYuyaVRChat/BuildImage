using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AttractionZone : UdonSharpBehaviour
{
    public float pullSpeed = 2f;
    public float rotateSpeed = 180f;

    [Header("正解オブジェクト(複数対応)")]
    public GameObject[] correctObjects;

    [Header("全部揃ったら消すオブジェクト")]
    public GameObject objectToHide;

    private bool[] placedFlags; //吸着したか判定用

    private void Start()
    {
        placedFlags = new bool[correctObjects.Length];
    }

    private void OnTriggerStay(Collider other)
    {
        AttractableObject attractable = other.GetComponent<AttractableObject>();
        if (attractable == null) return;

        other.transform.position = Vector3.MoveTowards(
            other.transform.position,
            transform.position,
            pullSpeed * Time.deltaTime
        );

        other.transform.rotation = Quaternion.RotateTowards(
            other.transform.rotation,
            Quaternion.identity,
            rotateSpeed * Time.deltaTime
        );

        // 判定距離以内か
        if (Vector3.Distance(other.transform.position, transform.position) < 0.05f)
        {
            for (int i = 0; i < correctObjects.Length; i++)
            {
                if (other.gameObject == correctObjects[i] && !placedFlags[i])
                {
                    placedFlags[i] = true;
                    Debug.Log($"✔ {other.name} セット完了");

                    // 吸着後位置固定
                    other.transform.position = transform.position;
                    other.transform.rotation = Quaternion.identity;
                }
            }

            // 全部揃ったらイベント実行
            if (AllPlaced() && objectToHide != null)
            {
                objectToHide.SetActive(false);
            }
        }
    }

    private bool AllPlaced()
    {
        foreach (bool placed in placedFlags)
        {
            if (!placed) return false;
        }
        return true;
    }
}

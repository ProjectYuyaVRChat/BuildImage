using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TouchCounter : UdonSharpBehaviour
{
    [Header("触れたらカウントするオブジェクト")]
    public GameObject targetObject;

    [Header("カウント数")]
    public int count = 0;

     // 一度だけカウントするためのフラグ
    private bool counted = false;


   [Header("回転数によってTrueにするオブジェクト")]
    public GameObject activateObject;

    [Header("設定")]
    public float checkInterval = 1f;       // 秒間の回転数を測る間隔
    public int minCount = 1;               // この回数以上で回転数OK
    
    private float timer = 0f;

    void Update()
    {
        // タイマー更新
        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            // 回転数判定
            if (count >= minCount)
            {
                if (activateObject != null)
                    activateObject.SetActive(true); // Trueに
            }
            else
            {
                if (activateObject != null)
                    activateObject.SetActive(false); // Falseに
            }

            count = 0;
            timer = 0f;
        }
    }

     void OnTriggerEnter(Collider other)
    {


        // 触れたオブジェクトが targetObject ならカウント
        if (other.gameObject == targetObject)
        {
            count++;
            Debug.Log("カウント数: " + count);
        }
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class TouchCounter : UdonSharpBehaviour
{
    [Header("触れたらカウントするオブジェクト")]
    public GameObject targetObject;

    [HideInInspector]
    public int count = 0;

    [Header("回転数によってTrueにするオブジェクト")]
    public GameObject activateObject;

    [Header("設定")]
    public float checkInterval = 1f;
    public int minCount = 1;

    private float timer = 0f;

    // ★ 同期フラグ
    [UdonSynced]
    private bool isActive = false;

    void Update()
    {
        // ★ オーナーだけが処理
        if (!Networking.IsOwner(gameObject)) return;

        timer += Time.deltaTime;

        if (timer >= checkInterval)
        {
            bool nextState = count >= minCount;

            if (isActive != nextState)
            {
                isActive = nextState;
                RequestSerialization();
            }

            count = 0;
            timer = 0f;
        }
    }

    public override void OnDeserialization()
    {
        if (activateObject != null)
        {
            activateObject.SetActive(isActive);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // ★ 最後に触った人がオーナーになる
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        // ★ オーナーになった人だけカウント
        if (Networking.IsOwner(gameObject) && other.gameObject == targetObject)
        {
            count++;
            Debug.Log("カウント数: " + count);
        }
    }
}
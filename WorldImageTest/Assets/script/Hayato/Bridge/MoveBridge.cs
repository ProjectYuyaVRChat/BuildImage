using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class MoveBridge : UdonSharpBehaviour
{
    private Transform currentTarget;
    [SerializeField] private Transform targetM;
    [SerializeField] private Transform targetP;
    [SerializeField] private float speed = 1f;
    void Start()
    {
        currentTarget =  targetM;
    }

    // Update is called once per frame
    void Update()
    {
        // 現在のターゲットに向かって移動
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);

        // ターゲットに十分に近づいたら、次のターゲットに切り替える
        if (Vector3.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            // currentTargetがtargetPならtargetMに、targetMならtargetPに切り替える
            currentTarget = (currentTarget == targetP) ? targetM : targetP;
        }
    }
}

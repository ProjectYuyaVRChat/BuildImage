using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class KeyCase : UdonSharpBehaviour
{
    [SerializeField] private Transform rightWall;
    [SerializeField] private Transform leftWall;
    [SerializeField] private float openSpeed = 0;
    
    private Vector3 initialRightLocalPosition;
    private Vector3 initialLeftLocalPosition;

    public bool isMove = false;

    private void Start()
    {
        // Startメソッドで壁の初期ローカル位置を保存
        initialRightLocalPosition = rightWall.localPosition;
        initialLeftLocalPosition = leftWall.localPosition;
    }

    private void Update()
    {
        if (isMove)
        {
            // 右壁の現在のローカル位置が初期ローカル位置から0.5未満であれば移動
            if (Vector3.Distance(initialRightLocalPosition, rightWall.localPosition) < 0.5f)
            {
                rightWall.localPosition += new Vector3(openSpeed * Time.deltaTime, 0, 0);
            }

            // 左壁の現在のローカル位置が初期ローカル位置から0.5未満であれば移動
            if (Vector3.Distance(initialLeftLocalPosition, leftWall.localPosition) < 0.5f)
            {
                leftWall.localPosition += new Vector3(-openSpeed * Time.deltaTime, 0, 0);
            }
        }
    }
}

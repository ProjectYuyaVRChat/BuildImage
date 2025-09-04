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
    
    private Vector3 initialRightPosition;
    private Vector3 initialLeftPosition;

    public bool isMove = false;

    private void Start()
    {
        // Startメソッドで壁の初期位置を保存
        initialRightPosition = rightWall.position;
        initialLeftPosition = leftWall.position;
    }

    private void Update()
    {
        if (isMove)
        {
            // 右壁の現在の位置が初期位置から0.5未満であれば移動
            if (Vector3.Distance(initialRightPosition, rightWall.position) < 0.5f)
            {
                rightWall.position += Vector3.right * openSpeed * Time.deltaTime;
            }

            // 左壁の現在の位置が初期位置から0.5未満であれば移動
            if (Vector3.Distance(initialLeftPosition, leftWall.position) < 0.5f)
            {
                leftWall.position += Vector3.left * openSpeed * Time.deltaTime;
            }
        }
    }
}

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ChangeRotateBridge : UdonSharpBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float rotateLimit = 30f;
    
    private void Update()
    {
        float newRotationZ = Mathf.Sin(Time.time * speed) * rotateLimit;
        
        transform.localRotation  = Quaternion.Euler(0, 0, newRotationZ);
    }
}

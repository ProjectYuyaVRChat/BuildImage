using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDKBase;
using VRC.Udon;


public class Hikidashi : UdonSharpBehaviour
{
    private Vector3 hikiObj;
    private Quaternion hikiRot;
    private float currentZ;
    
    void Start()
    {
        hikiObj = transform.position;
        hikiRot = transform.rotation;
    }

    
    void Update()
    {
        currentZ = transform.position.z;

        Vector3 newPosition = new Vector3(hikiObj.x, hikiObj.y, currentZ);

        transform.position = newPosition;
        transform.Rotate(0,0,0);

        transform.rotation = hikiRot;
    }
}

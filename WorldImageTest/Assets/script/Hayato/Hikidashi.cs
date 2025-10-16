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
    private float currentX;
    private float currentY;
    private float currentZ;
    private Rigidbody rb;
    private Vector3 newPosition;
    public bool x = false;
    public bool y = false;
    public bool z = false;
    
    void Start()
    {
        hikiObj = transform.position;
        hikiRot = transform.rotation;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    
    void Update()
    {
        if (x)
        {
            currentX = transform.position.x;
            newPosition = new Vector3(currentX, hikiObj.y, hikiObj.z);
        }
        
        if (y)
        {
            currentY = transform.position.y;
            newPosition = new Vector3(hikiObj.x, currentY, hikiObj.z);
        }

        if (z)
        {
            currentZ = transform.position.z;
            newPosition = new Vector3(hikiObj.x, hikiObj.y, currentZ);
        }

        transform.position = newPosition;
        transform.Rotate(0,0,0);
        rb.velocity = Vector3.zero;

        transform.rotation = hikiRot;
    }
}

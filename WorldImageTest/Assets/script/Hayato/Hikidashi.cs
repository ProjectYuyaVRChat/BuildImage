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
    public float maxDistance;
    public bool minus = false;
    
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
            if (minus)
            {
                if (maxDistance <= hikiObj.x - currentX)
                {
                    newPosition = new Vector3(hikiObj.x - maxDistance, hikiObj.y, hikiObj.z);
                }

                if (0 <= currentX - hikiObj.x)
                {
                    newPosition = hikiObj;
                }
            }
            else
            {
                if (maxDistance <=currentX - hikiObj.x)
                {
                    newPosition = new Vector3(hikiObj.x + maxDistance, hikiObj.y, hikiObj.z);
                }

                if (0 <= hikiObj.x - currentX)
                {
                    newPosition = hikiObj;
                }
            }
        }
        
        if (y)
        {
            currentY = transform.position.y;
            newPosition = new Vector3(hikiObj.x, currentY, hikiObj.z);
            if (minus)
            {
                if (maxDistance <= hikiObj.y - currentY)
                {
                    newPosition = new Vector3(hikiObj.x, hikiObj.y - maxDistance, hikiObj.z);
                }

                if (0 <= currentY - hikiObj.y)
                {
                    newPosition = hikiObj;
                }
            }
            else
            {
                if (maxDistance <=currentY - hikiObj.y)
                {
                    newPosition = new Vector3(hikiObj.x, hikiObj.y + maxDistance, hikiObj.z);
                }

                if (0 <= hikiObj.y - currentY)
                {
                    newPosition = hikiObj;
                }
            }
        }

        if (z)
        {
            currentZ = transform.position.z;
            newPosition = new Vector3(hikiObj.x, hikiObj.y, currentZ);
            if (minus)
            {
                if (maxDistance <= hikiObj.z - currentZ)
                {
                    newPosition = new Vector3(hikiObj.x, hikiObj.y, hikiObj.z - maxDistance);
                }

                if (0 <= currentZ - hikiObj.z)
                {
                    newPosition = hikiObj;
                }
            }
            else
            {
                if (maxDistance <=currentZ - hikiObj.z)
                {
                    newPosition = new Vector3(hikiObj.x, hikiObj.y, hikiObj.z + maxDistance);
                }

                if (0 <= hikiObj.z - currentZ)
                {
                    newPosition = hikiObj;
                }
            }
        }

        transform.position = newPosition;
        transform.Rotate(0,0,0);
        rb.velocity = Vector3.zero;

        transform.rotation = hikiRot;
    }
}

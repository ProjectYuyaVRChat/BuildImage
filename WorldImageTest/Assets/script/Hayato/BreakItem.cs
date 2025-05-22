using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BreakItem : UdonSharpBehaviour
{
    [UdonSynced] private bool isAlive = true;
    
    private MeshRenderer itemMesh;

    private void Start()
    {
        itemMesh = GetComponent<MeshRenderer>();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "HummerHead")
        {
            isAlive = false;
            
        }
    }
}

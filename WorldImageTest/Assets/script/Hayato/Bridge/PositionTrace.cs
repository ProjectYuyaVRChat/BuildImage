using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;


public class PositionTrace : UdonSharpBehaviour
{
    [SerializeField] private Transform target;

    private void Update()
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
    }
}

using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PassMidPoint : UdonSharpBehaviour
{
    public bool isOn = false;

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        isOn = true;
    }
}

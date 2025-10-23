using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CheckPassArea : UdonSharpBehaviour
{
    [SerializeField] private MeshRenderer passBridgeParts;
    
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        passBridgeParts.enabled = true;
    }
}

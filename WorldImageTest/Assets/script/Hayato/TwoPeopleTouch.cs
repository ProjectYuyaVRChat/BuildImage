using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TwoPeopleTouch : UdonSharpBehaviour
{
    public WarpBlock PastWarpBlock;
    public WarpBlock FutureWarpBlock;

    void Start()
    {
        if (PastWarpBlock == null)
        {
            Debug.LogWarning("PastWarpBlockが入ってない");
        }

        if (FutureWarpBlock == null)
        {
            Debug.LogWarning("FutureWarpBlockが入ってない");
        }
    }
    
    void Update()
    {
        if (PastWarpBlock.isOn && FutureWarpBlock.isOn)
        {
            PastWarpBlock.DoFade();
            FutureWarpBlock.DoFade();
            PastWarpBlock.isOn = false;
            FutureWarpBlock.isOn = false;
        }
    }
}

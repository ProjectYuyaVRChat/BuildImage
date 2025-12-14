using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class MoguraController : UdonSharpBehaviour
{
    public MoguraGameManager startButton;
    
    public override void Interact()
    {
        startButton.StartGame();
    }
}

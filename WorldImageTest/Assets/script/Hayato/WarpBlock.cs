using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WarpBlock : UdonSharpBehaviour
{
    [SerializeField] private Transform WarpPosition;
    private Vector3 PlayerPosition;

    public override void Interact()
    {
        PlayerPosition = Networking.LocalPlayer.GetPosition();
        
    }

    private void Warp()
    {
        
    }
}

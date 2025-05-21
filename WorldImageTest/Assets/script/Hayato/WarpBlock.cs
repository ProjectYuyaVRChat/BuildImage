using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WarpBlock : UdonSharpBehaviour
{
    [UdonSynced]
    private bool isOn = false;
    
    [Header("ワープ先")]
    [SerializeField] private GameObject warpPoint;
    private Quaternion PlayerRotate;
    private Vector3 WarpPosition;
    [Header("暗転UI")]
    public GameObject fadeCanvas;         // Canvas本体
    private Animator fadeAnimator;

    private void Start()
    {
        WarpPosition = warpPoint.transform.position;
        fadeCanvas.SetActive(false);
        fadeAnimator = fadeCanvas.GetComponentInChildren<Animator>();
    }

    public override void Interact()
    {
        DoFade();
        SendCustomEventDelayedSeconds(nameof(Warp),1);
        SendCustomEventDelayedSeconds(nameof(EndFade),2);
    }

    public void Warp()
    {
        Debug.Log("aaaaa");
        PlayerRotate = new Quaternion(0, -180, 0, 0);
        Networking.LocalPlayer.TeleportTo(WarpPosition,PlayerRotate);
    }
    
    public void DoFade()
    {
        fadeCanvas.SetActive(true);
        fadeAnimator.SetTrigger("FadeIn");
    }

    public void EndFade()
    {
        fadeCanvas.SetActive(false);
    }
}

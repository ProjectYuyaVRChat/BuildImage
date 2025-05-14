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
    [SerializeField] private GameObject WarpPoint;
    private Quaternion PlayerRotate;
    private Vector3 WarpPosition;
    [Header("暗転UI")]
    public GameObject fadeCanvas;         // Canvas本体
    private Animator fadeAnimator;

    private void Start()
    {
        WarpPosition = WarpPoint.transform.position;
        fadeCanvas.SetActive(false);
        fadeAnimator = fadeCanvas.GetComponent<Animator>();
    }

    public override void Interact()
    {
        fadeCanvas.SetActive(true);
        PlayerRotate = new Quaternion(0, -180, 0, 0);
        Networking.LocalPlayer.TeleportTo(WarpPosition,PlayerRotate);
    }

    public void DoFade()
    {
        fadeAnimator.SetTrigger("FadeIn");
    }

    public void EndFade()
    {
        fadeCanvas.SetActive(false);
    }
}

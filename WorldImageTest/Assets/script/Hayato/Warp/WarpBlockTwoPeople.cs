using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class WarpBlockTwoPeople : UdonSharpBehaviour
{
    [UdonSynced]
    public bool isOn = false;

    [UdonSynced]
    private int interactingPlayerId = -1;
    
    
    [Header("ワープ先")]
    [SerializeField] private GameObject warpPoint;
    private Quaternion PlayerRotate;
    private Vector3 WarpPosition;
    [Header("暗転UI")]
    public GameObject fadeCanvas;         // Canvas本体
    private Animator fadeAnimator;
    
    private bool isSequenceRunning = false;

    private void Start()
    {
        WarpPosition = warpPoint.transform.position;
        fadeCanvas.SetActive(false);
        fadeAnimator = fadeCanvas.GetComponentInChildren<Animator>();
    }

    public override void Interact()
    {
        if (isOn) return;
        
        // このオブジェクトのオーナーを自分自身に設定し、同期変数を変更する権限を得る
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        
        // 状態を更新
        this.isOn = true;
        this.interactingPlayerId = Networking.LocalPlayer.playerId;
        
        // 同期変数の変更を他のプレイヤーに通知
        RequestSerialization();
    }

    public void Warp()
    {
        // 連続で実行されないようにガード
        if (isSequenceRunning) return;
        
        // 誰も操作していない場合は何もしない
        if (interactingPlayerId == -1) return;
        
        // このブロックを操作した本人だけがワープ処理を実行する
        VRCPlayerApi playerToWarp = VRCPlayerApi.GetPlayerById(interactingPlayerId);
        if (playerToWarp != null && playerToWarp.isLocal)
        {
            isSequenceRunning = true;
            DoFade();
        }
    }
    
    public void DoFade()
    {
        fadeCanvas.SetActive(true);
        fadeAnimator.SetTrigger("FadeIn");
        SendCustomEventDelayedSeconds(nameof(WarpPlayer),1);
        SendCustomEventDelayedSeconds(nameof(EndFade),2);
    }
    
    public void WarpPlayer()
    {
        VRCPlayerApi playerToWarp = VRCPlayerApi.GetPlayerById(interactingPlayerId);
        if (playerToWarp != null && playerToWarp.isLocal)
        {
            playerToWarp.TeleportTo(
                warpPoint.transform.position,
                Quaternion.Euler(0, 0f, 0)
            );
        }
    }

    public void EndFade()
    {
        if (fadeCanvas != null)
        {
            fadeCanvas.SetActive(false);
        }
        isSequenceRunning = false;
    }
    
    public void ResetState()
    {
        this.isOn = false;
        this.interactingPlayerId = -1;
        this.isSequenceRunning = false;
        
        // リセットした状態を全員に同期
        RequestSerialization();
    }
}

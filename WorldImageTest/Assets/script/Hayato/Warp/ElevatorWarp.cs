using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ElevatorWarp : UdonSharpBehaviour
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
    public float EndFadeTime = 2f;

    [SerializeField] private AudioClip elevatorOpenSound;
    [SerializeField] private AudioClip elevatorCloseSound;
    [SerializeField] private AudioClip elevatorUpSound;
    
    private bool isSequenceRunning = false;
    AudioSource audioSource;
    
    [SerializeField] private Animator closeAnimation;

    private void Start()
    {
        WarpPosition = warpPoint.transform.position;
        fadeCanvas.SetActive(false);
        fadeAnimator = fadeCanvas.GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Interact()
    {
        if (isOn) return;
        
        // このオブジェクトのオーナーを自分自身に設定し、同期変数を変更する権限を得る
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        
        // 状態を更新
        this.isOn = true;
        this.interactingPlayerId = Networking.LocalPlayer.playerId;
        
        Warp();
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

    public void ElevatorOpenSE()
    {
        audioSource.PlayOneShot(elevatorOpenSound);
    }
    
    public void ElevatorCloseSE()
    {
        audioSource.PlayOneShot(elevatorCloseSound);
    }
    
    public void ElevatorUpSE()
    {
        audioSource.PlayOneShot(elevatorUpSound);
    }
    
    
    
    public void DoFade()
    {
        ElevatorCloseSE();
        closeAnimation.SetTrigger("DoorCloseTrigger");
        fadeCanvas.SetActive(true);
        SendCustomEventDelayedSeconds(nameof(StartFadeAnimater),3);
        SendCustomEventDelayedSeconds(nameof(WarpPlayer),4);
        SendCustomEventDelayedSeconds(nameof(EndFade),EndFadeTime + 4);
    }

    public void StartFadeAnimater()
    {
        fadeAnimator.SetTrigger("StartFade");
        ElevatorUpSE();
    }
    
    public void WarpPlayer()
    {
        VRCPlayerApi playerToWarp = VRCPlayerApi.GetPlayerById(interactingPlayerId);
        if (playerToWarp != null && playerToWarp.isLocal)
        {
            playerToWarp.TeleportTo(
                warpPoint.transform.position,
                Quaternion.Euler(0, 180f, 0)
            );
        }
    }

    public void EndFade()
    {
        if (fadeCanvas != null)
        {
            ElevatorOpenSE();
            fadeAnimator.SetTrigger("EndFade");
            SendCustomEventDelayedSeconds(nameof(DeleteCanvas),EndFadeTime);
        }
        isSequenceRunning = false;
        
        ResetState();
    }

    public void DeleteCanvas()
    {
        fadeCanvas.SetActive(false);
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

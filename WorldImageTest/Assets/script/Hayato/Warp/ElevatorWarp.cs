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
    
    // 【重要】監視スクリプトから「ON かつ プレイヤーIDも正常か」を確認するためのプロパティ
    public bool IsReady => isOn && interactingPlayerId != -1;

    private int _cachedTargetPlayerId = -1; 
    
    [Header("ワープ先")]
    [SerializeField] private GameObject warpPoint;
    [Header("暗転UI")]
    public GameObject fadeCanvas;         
    private Animator fadeAnimator;
    public float EndFadeTime = 2f;

    [SerializeField] private AudioClip elevatorOpenSound;
    [SerializeField] private AudioClip elevatorCloseSound;
    [SerializeField] private AudioClip elevatorUpSound;
    
    private bool isSequenceRunning = false;
    AudioSource audioSource;
    
    [SerializeField] private Animator closeAnimation;
    [SerializeField] private Animator openAnimation;
    [SerializeField] private float y = 180f;

    private void Start()
    {
        if(fadeCanvas != null) fadeCanvas.SetActive(false);
        if(fadeCanvas != null) fadeAnimator = fadeCanvas.GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Interact()
    {
        if (isOn) return;
        
        // オーナー権限を取得してから変数をセット
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        
        this.isOn = true;
        this.interactingPlayerId = Networking.LocalPlayer.playerId;
        
        RequestSerialization();
    }

    public void Warp()
    {
        if (isSequenceRunning) return;
        // IDが無効なら処理しない（ここが片方だけ失敗する原因のガードになる）
        if (interactingPlayerId == -1) return;
        
        VRCPlayerApi playerToWarp = VRCPlayerApi.GetPlayerById(interactingPlayerId);
        
        // ローカルプレイヤーの場合のみアニメーションとワープを実行
        if (playerToWarp != null && playerToWarp.isLocal)
        {
            isSequenceRunning = true;
            _cachedTargetPlayerId = interactingPlayerId;
            DoFade();
        }
    }

    // --- 以降のアニメーション・音声処理は変更なし ---

    public void ElevatorOpenSE()
    {
        if(audioSource) audioSource.PlayOneShot(elevatorOpenSound);
    }
    
    public void ElevatorCloseSE()
    {
        if(audioSource) audioSource.PlayOneShot(elevatorCloseSound);
    }
    
    public void ElevatorUpSE()
    {
        if(audioSource) audioSource.PlayOneShot(elevatorUpSound);
    }
    
    public void DoFade()
    {
        ElevatorCloseSE();
        if(closeAnimation) closeAnimation.SetTrigger("DoorCloseTrigger");
        if(fadeCanvas) fadeCanvas.SetActive(true);
        SendCustomEventDelayedSeconds(nameof(StartFadeAnimater), 3);
        SendCustomEventDelayedSeconds(nameof(WarpPlayer), 4);
        SendCustomEventDelayedSeconds(nameof(EndFade), EndFadeTime + 4);
    }

    public void StartFadeAnimater()
    {
        if(fadeAnimator) fadeAnimator.SetTrigger("StartFade");
        ElevatorUpSE();
    }
    
    public void WarpPlayer()
    {
        VRCPlayerApi playerToWarp = VRCPlayerApi.GetPlayerById(_cachedTargetPlayerId);
        
        if (playerToWarp != null && playerToWarp.isLocal)
        {
            playerToWarp.TeleportTo(
                warpPoint.transform.position,
                Quaternion.Euler(0, y, 0)
            );
        }
    }

    public void EndFade()
    {
        if (fadeCanvas != null)
        {
            ElevatorOpenSE();
            fadeAnimator.SetTrigger("EndFade");
            SendCustomEventDelayedSeconds(nameof(DeleteCanvas), EndFadeTime);
        }
        
        DoorOpen();
        
        // ローカルのフラグを下ろす
        isSequenceRunning = false;
        
        // 最後に状態をリセット
        ResetState();
    }

    private void DoorOpen()
    {
        if(openAnimation) openAnimation.SetTrigger("Open");
    }

    public void DeleteCanvas()
    {
        if(fadeCanvas) fadeCanvas.SetActive(false);
    }
    
    public void ResetState()
    {
        if(Networking.IsOwner(gameObject))
        {
            this.isOn = false;
            this.interactingPlayerId = -1;
            RequestSerialization();
        }
        
        this.isSequenceRunning = false;
        this._cachedTargetPlayerId = -1;
    }
}
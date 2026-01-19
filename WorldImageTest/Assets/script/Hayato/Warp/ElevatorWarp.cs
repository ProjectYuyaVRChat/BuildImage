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
    
    // 追加: ワープ処理中にIDを保持しておくためのローカル変数
    private int _cachedTargetPlayerId = -1; 
    
    [Header("ワープ先")]
    [SerializeField] private GameObject warpPoint;
    // private Quaternion PlayerRotate; // 使われていないので削除可
    // private Vector3 WarpPosition;    // 使われていないので削除可
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
        // WarpPosition = warpPoint.transform.position; // 使っていないのでコメントアウト
        if(fadeCanvas != null) fadeCanvas.SetActive(false); // nullチェック追加
        if(fadeCanvas != null) fadeAnimator = fadeCanvas.GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public override void Interact()
    {
        if (isOn) return;
        
        Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        
        this.isOn = true;
        this.interactingPlayerId = Networking.LocalPlayer.playerId;
        
        RequestSerialization();
    }

    public void Warp()
    {
        if (isSequenceRunning) return; 
        if (interactingPlayerId == -1) return;
        
        VRCPlayerApi playerToWarp = VRCPlayerApi.GetPlayerById(interactingPlayerId);
        if (playerToWarp != null && playerToWarp.isLocal)
        {
            isSequenceRunning = true;
            
            // 【重要】ここでIDをローカル変数に退避させる
            _cachedTargetPlayerId = interactingPlayerId;
            
            DoFade();
        }
    }

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
        // 【修正】interactingPlayerId ではなく _cachedTargetPlayerId を使う
        // これにより、途中で同期変数がリセットされてもワープできる
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
        
        // ローカルのシーケンスフラグを下ろす
        isSequenceRunning = false;
        
        // 最後に自分で状態をリセットする（これにより監視側のリセットに頼る必要がなくなる）
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
        // オーナーだけが同期変数を変更できる
        if(Networking.IsOwner(gameObject))
        {
            this.isOn = false;
            this.interactingPlayerId = -1;
            RequestSerialization();
        }
        
        // ローカル変数の掃除
        this.isSequenceRunning = false;
        this._cachedTargetPlayerId = -1;
    }
}
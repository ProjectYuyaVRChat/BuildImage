using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)] // 明示的にManual設定
public class EntryKeyReader : UdonSharpBehaviour
{
    [SerializeField] private int openID;
    [SerializeField] private Animator gate;
    private AudioSource audioSource;
    [SerializeField] private AudioClip open;
    [SerializeField] private AudioClip scan;

    // UdonSynced変数が変更されたときに自動でOnDeserialization等を処理するためのフラグ
    [UdonSynced(UdonSyncMode.None)] private bool isGateOpen;
    
    // ローカルでの状態保持用（音が二重に鳴ったりするのを防ぐ）
    private bool _localIsGateOpen;

    private void Start()
    {
        if (open != null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        // 初期状態を同期
        UpdateGateState();
    }

    private void OnTriggerEnter(Collider other)
    {
        var card = other.GetComponent<CardKey>();
        if (card == null) return;

        // 既に開いているなら何もしない（連打防止）
        if (isGateOpen) return;

        int id = card.keyID;
        if(scan != null) audioSource.PlayOneShot(scan);

        if (openID == id)
        {
            // オーナー権限を取得
            if(!Networking.IsOwner(gameObject)) Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            // 変数を更新して同期
            isGateOpen = true;
            RequestSerialization();
            
            // 自分の見た目を更新
            UpdateGateState();
        }
    }

    public override void OnDeserialization()
    {
        // 他プレイヤーが変数を受け取ったときに実行
        UpdateGateState();
    }

    public void UpdateGateState()
    {
        // 変数(isGateOpen)とローカル状態(_localIsGateOpen)が食い違っている時だけ処理する
        if (isGateOpen != _localIsGateOpen)
        {
            if (isGateOpen)
            {
                // 開く処理
                if (open != null) audioSource.PlayOneShot(open);
                
                // TriggerではなくBoolを推奨（ステートマシンで "IsOpen" boolパラメータを作成してください）
                // もしTriggerのままにしたい場合は gate.SetTrigger("Open"); に戻してください
                gate.SetTrigger("Open"); 
            }
            else 
            {
                // (将来的に閉じる処理を入れるならここ)
                //gate.SetBool("IsOpen", false);
            }

            // ローカル状態を最新に更新
            _localIsGateOpen = isGateOpen;
        }
    }
}
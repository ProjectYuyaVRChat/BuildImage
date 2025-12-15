using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MoguraGameManager : UdonSharpBehaviour
{
    [Header("モグラ達のリスト")]
    [SerializeField] private Mogura[] moles; // クラス名がMoguraControllerなら適宜変更してください

    [Header("ゲーム設定")]
    [Tooltip("モグラが出る間隔（秒）")]
    [SerializeField] private float spawnInterval = 1.0f;
    
    [Tooltip("ゲームの制限時間（秒）")]
    [SerializeField] private float gameDuration = 30.0f;

    // --- 同期用変数 ---
    // [UdonSynced]をつけると、オーナーの値が全員にコピーされます
    [UdonSynced] private bool isPlaying = false;
    
    // 次に動かすモグラの番号
    [UdonSynced] private int syncMoleIndex = -1;
    
    // 命令が実行された回数（同じモグラが連続で選ばれた時も反応できるようにカウンターを使う）
    [UdonSynced] private int syncPopCounter = 0;

    // ローカル（自分のPC）での前回のカウンター値
    private int localPopCounter = 0;

    private float spawnTimer = 0f;
    private float totalGameTimer = 0f;

    private void Update()
    {
        // 処理はオーナー（このオブジェクトの所有者）のみが計算する
        // オーナー以外は何もしないで、オーナーからの同期変数が変わるのを待つだけ
        if (!Networking.IsOwner(gameObject)) return;

        // ゲーム中じゃなければ何もしない
        if (!isPlaying) return;

        // 1. 制限時間の管理
        totalGameTimer += Time.deltaTime;
        if (totalGameTimer >= gameDuration)
        {
            EndGame();
            return;
        }

        // 2. モグラの出現管理
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            PopRandomMoleAsOwner(); // オーナーとして抽選を行う
        }
    }

    // スタートボタンのInteractなどから呼ぶ
    public void StartGame()
    {
        // ボタンを押した人をこのオブジェクトのオーナーにする（重要）
        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        isPlaying = true;
        spawnTimer = 0f;
        totalGameTimer = 0f;
        
        // 変数が変わったことを全員に送信
        RequestSerialization();
        
        Debug.Log("Game Started!");
    }

    public void EndGame()
    {
        isPlaying = false;
        RequestSerialization();
        Debug.Log("Game Over!");
    }

    // オーナーだけが実行する抽選処理
    private void PopRandomMoleAsOwner()
    {
        if (moles == null || moles.Length == 0) return;

        // ランダムに番号を決める
        int randomIndex = Random.Range(0, moles.Length);

        // 同期変数を更新
        syncMoleIndex = randomIndex;
        syncPopCounter++; // カウンターを増やして「新しい命令が来た」ことを知らせる

        // 全員にデータを送信
        RequestSerialization();

        // オーナー自身の画面でも動かす（通信ラグなしで即座に動かすため）
        MoveMoleLocal(syncMoleIndex);
        localPopCounter = syncPopCounter; // ローカルカウンタも合わせておく
    }

    // UdonSynced変数が更新されたときに、全プレイヤーで自動的に呼ばれる関数
    public override void OnDeserialization()
    {
        // カウンターの値が変わっていたら、新しいモグラ命令が来たと判断する
        if (syncPopCounter != localPopCounter)
        {
            MoveMoleLocal(syncMoleIndex);
            localPopCounter = syncPopCounter; // 最新のカウンター値に更新
        }
    }

    // 実際にモグラを動かす処理（画面表示用）
    private void MoveMoleLocal(int index)
    {
        if (index >= 0 && index < moles.Length && moles[index] != null)
        {
            moles[index].MoveMogura();
        }
    }
}
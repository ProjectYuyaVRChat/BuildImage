using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Mogura : UdonSharpBehaviour
{
    [Header("設定")]
    [SerializeField] private float upperLimit = 0.5f; // 数値を調整しやすいよう初期値を設定
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float downSpeed = 0.7f;
    [SerializeField] private float waitSeconds = 0.5f;

    [Header("参照")]
    [SerializeField] private MoguraGameManager gameManager; 

    private CapsuleCollider mogura;
    private bool move = false; // ローカルでの動作フラグ
    
    [UdonSynced(UdonSyncMode.None)] 
    private bool up = true; // 同期変数：true=上昇・待機フェーズ、false=下降フェーズ
    
    private Vector3 startPosition;
    private float timer = 0f; // 待機時間計測用
    
    private void Start()
    {
        mogura = this.GetComponent<CapsuleCollider>();
        mogura.enabled = false;
        startPosition = transform.position;
    }

    private void Update()
    {
        // 動作中でなければ処理しない
        if (!move) return;

        // --- 上昇フェーズ ---
        if (up)
        {
            // 現在の高さを計算
            float currentDistance = transform.position.y - startPosition.y;

            // まだ上限に達していない場合、上昇させる
            if (currentDistance < upperLimit)
            {
                transform.position += Vector3.up * speed * Time.deltaTime;
            }
            else
            {
                // ローカルでも見た目を強制的に上限に合わせる（突き抜け防止）
                Vector3 clampedPos = transform.position;
                clampedPos.y = startPosition.y + upperLimit;
                transform.position = clampedPos;

                // 頂上に着いたらタイマーを回す（待機処理）
                timer += Time.deltaTime;

                // オーナーのみが状態を切り替える権限を持つ
                if (Networking.IsOwner(gameObject))
                {
                    if (timer >= waitSeconds)
                    {
                        // 待機完了 -> 下降モードへ
                        up = false;
                        RequestSerialization();
                    }
                }
            }
        }
        // --- 下降フェーズ ---
        else
        {
            transform.position += Vector3.down * downSpeed * Time.deltaTime;

            // スタート位置より下に行ったら終了
            if (transform.position.y <= startPosition.y)
            {
                // 位置を正確にリセット
                transform.position = startPosition;
                move = false;
                
                // 次回のために変数をリセット
                timer = 0f;
            }
        }
    }

    // Managerから呼ばれてモグラが出現する関数
    public void MoveMogura()
    {
        // オーナー権限を取得してから変数を変更
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        mogura.enabled = true; // 当たり判定有効
        
        move = true; // 動作開始
        up = true;   // 上昇モード
        timer = 0f;  // タイマーリセット
        
        RequestSerialization();
    }
    
    // ハンマーとの当たり判定
    public void OnTriggerEnter(Collider other)
    {
        // ハンマー以外は無視 & すでに叩かれて下降中なら無視
        if (other == null || other.gameObject.name != "HummerHead") return;
        if (!up) return; // すでに下がっている最中なら何もしない

        // --- ヒット時の処理 ---
        
        // オーナー権限取得
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        
        // 二重判定防止
        mogura.enabled = false;
        
        // 即座に下降させるため up を false にする
        up = false;
        // 待機時間をスキップしたい場合はタイマーを満了させるか、ロジックで制御
        // ここでは up=false にするだけで Update の else ブロックに入り下降が始まる
        
        RequestSerialization();

        // マネージャーへの通知
        if (gameManager != null)
        {
            gameManager.OnMoleHit();
        }
    }
}
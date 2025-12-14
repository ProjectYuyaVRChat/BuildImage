using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MoguraGameManager : UdonSharpBehaviour
{
    [Header("モグラ達のリスト")]
    // ここにヒエラルキーにある6つのモグラオブジェクトを登録します
    [SerializeField] private Mogura[] moles;

    [Header("ゲーム設定")]
    [Tooltip("モグラが出る間隔（秒）")]
    [SerializeField] private float spawnInterval = 1.0f;
    
    [Tooltip("ゲームの制限時間（秒）")]
    [SerializeField] private float gameDuration = 30.0f;

    private bool isPlaying = false;
    private float spawnTimer = 0f;
    private float totalGameTimer = 0f;

    private void Update()
    {
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
            spawnTimer = 0f; // タイマーリセット
            PopRandomMole(); // ランダムに出す処理へ
        }
    }

    // ゲーム開始ボタンなどからこの関数を呼ぶ
    public void StartGame()
    {
        isPlaying = true;
        spawnTimer = 0f;
        totalGameTimer = 0f;
        Debug.Log("Game Started!");
    }

    // ゲーム終了処理
    public void EndGame()
    {
        isPlaying = false;
        Debug.Log("Game Over!");
    }

    // ランダムなモグラを動かす
    private void PopRandomMole()
    {
        if (moles == null || moles.Length == 0) return;

        // 0番目から(モグラの数-1)番目の間でランダムな数字を選ぶ
        int randomIndex = Random.Range(0, moles.Length);

        // 選ばれたモグラが存在すれば動かす
        if (moles[randomIndex] != null)
        {
            moles[randomIndex].MoveMogura();
        }
    }
}
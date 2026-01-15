using UdonSharp;
using UnityEngine;
using TMPro; // TextMeshProを使うために必要
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class MoguraGameManager : UdonSharpBehaviour
{
    [Header("モグラ達のリスト")]
    [SerializeField] private Mogura[] moles;

    [Header("UI設定")]
    [Tooltip("全ての状態（待機・カウントダウン・スコア・結果）を表示するテキスト")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI statusText2;

    [Header("ゲーム設定")]
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private float gameDuration = 60.0f;
    [SerializeField] private float countdownDuration = 3.0f;
    [SerializeField] private int targetScore = 15;
    [SerializeField] private int maxMoleCount = 30;

    // --- 同期用変数 ---
    [UdonSynced] private bool isPlaying = false;
    [UdonSynced] private bool isCountingDown = false;
    
    [UdonSynced] private int syncMoleIndex = -1;
    [UdonSynced] private int syncPopCounter = 0;
    
    [UdonSynced] private int currentScore = 0;
    [UdonSynced] private int syncCountdownVal = 0;

    // ローカル用変数
    private int localPopCounter = 0;
    private float spawnTimer = 0f;
    private float totalGameTimer = 0f;
    private float currentCountdownTimer = 0f;
    private int spawnedMoleCount = 0;
    private int lastDisplayCount = -1;
    
    [SerializeField] private GimmickManager gimmickManager;
    [UdonSynced] private bool isCleared = false;

    private void Start()
    {
        // 初期表示
        if (statusText != null) statusText.text = "Press Button\nto Start";
        if (statusText2 != null) statusText2.text = "Start?";
    }

    private void Update()
    {
        if (!Networking.IsOwner(gameObject)) return;

        // 1. カウントダウン処理
        if (isCountingDown)
        {
            currentCountdownTimer -= Time.deltaTime;
            int displayCount = Mathf.CeilToInt(currentCountdownTimer);

            // 数字が変わった時だけ更新
            if (displayCount != lastDisplayCount)
            {
                syncCountdownVal = displayCount;
                UpdateUI(); 
                RequestSerialization();
                
                lastDisplayCount = displayCount;
            }

            if (currentCountdownTimer <= 0f)
            {
                isCountingDown = false;
                isPlaying = true;
                
                syncCountdownVal = 0; // 0は「START!」の合図
                UpdateUI();
                RequestSerialization();

                spawnTimer = spawnInterval;
            }
            return;
        }

        // 2. ゲーム中の処理
        if (!isPlaying) return;

        totalGameTimer += Time.deltaTime;

        if (totalGameTimer >= gameDuration)
        {
            EndGame(false);
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            if (spawnedMoleCount < maxMoleCount)
            {
                PopRandomMoleAsOwner();
            }
        }
    }

    public void StartGame()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);

        isCountingDown = true;
        isPlaying = false;
        currentCountdownTimer = countdownDuration;
        lastDisplayCount = -1;
        
        spawnTimer = 0f;
        totalGameTimer = 0f;
        currentScore = 0;
        spawnedMoleCount = 0;
        
        syncCountdownVal = (int)countdownDuration;

        RequestSerialization();
        // UI更新はUpdate内で行われるためここでは呼ばなくても即座に反映されますが
        // 念のため呼んでも良いです
    }

    public void EndGame(bool isClear)
    {
        isPlaying = false;
        isCountingDown = false;
        
        UpdateUI(); 
        RequestSerialization();
    }

    private void PopRandomMoleAsOwner()
    {
        if (moles == null || moles.Length == 0) return;

        int randomIndex = Random.Range(0, moles.Length);
        syncMoleIndex = randomIndex;
        syncPopCounter++;
        spawnedMoleCount++;

        RequestSerialization();
        MoveMoleLocal(syncMoleIndex);
        localPopCounter = syncPopCounter;
    }

    public void OnMoleHit()
    {
        if (!Networking.IsOwner(gameObject))
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(AddScoreOwner));
        }
        else
        {
            AddScoreOwner();
        }
    }

    public void AddScoreOwner()
    {
        if (!isPlaying) return;

        currentScore++;
        UpdateUI(); 
        RequestSerialization();

        if (currentScore >= targetScore)
        {
            EndGame(true);
        }
    }

    public override void OnDeserialization()
    {
        if (syncPopCounter != localPopCounter)
        {
            MoveMoleLocal(syncMoleIndex);
            localPopCounter = syncPopCounter;
        }

        UpdateUI();
    }

    // --- UI更新ロジック（ここを統合しました） ---
    private void UpdateUI()
    {
        if (statusText == null) return;
        if (statusText2 == null) return;

        if (isCountingDown)
        {
            // カウントダウン中は数字だけを大きく表示
            if (syncCountdownVal > 0)
            {
                statusText.text = syncCountdownVal.ToString();
                statusText2.text = syncCountdownVal.ToString();
            }
            else
            {
                statusText.text = "START!";
                statusText2.text = "START!";
            }
        }
        else if (isPlaying)
        {
            // ゲーム中はスコアを表示
            statusText.text = "Score: " + currentScore + " / " + targetScore;
            statusText2.text = "Score: " + currentScore + " / " + targetScore;
        }
        else
        {
            // ゲーム終了後または待機状態
            if (currentScore >= targetScore && totalGameTimer > 0) // クリア後
            {
                statusText.text = "GAME CLEAR!!\nScore: " + currentScore;
                statusText2.text = "GAME CLEAR!!\nScore: " + currentScore;
                ClearGame();
            }
            else if (totalGameTimer >= gameDuration) // 時間切れ
            {
                statusText.text = "TIME UP...\nScore: " + currentScore;
                statusText2.text = "TIME UP...\nScore: " + currentScore;
            }
            else // 初期状態（totalGameTimerがリセットされている場合など）
            {
                statusText.text = "Press Button\nto Start";
                statusText2.text = "Press Button\nto Start";
            }
        }
    }

    private void MoveMoleLocal(int index)
    {
        if (index >= 0 && index < moles.Length && moles[index] != null)
        {
            moles[index].MoveMogura();
        }
    }
    
    private void ClearGame()
    {
        if (!isCleared)
        {
            gimmickManager.ReportClear();
            isCleared = true;
            RequestSerialization();
        }
    }
}

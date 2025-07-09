using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DoorAreaTrigger : UdonSharpBehaviour
{
    [Header("範囲設定")]
    [SerializeField] private DoorGimmickSystemNew targetDoorSystem;
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private string areaName = "ドアエリア";
    
    [Header("キャリブレーション設定")]
    [SerializeField] private CentralizedMotionDetector centralizedMotionDetector;
    [SerializeField] private float calibrationDelay = 2.0f; // キャリブレーション待機秒数
    
    [Header("キャリブレーションUI")]
    [SerializeField] private TMPro.TextMeshProUGUI calibrationUIText;
    [SerializeField] private string calibrationMessage = "体をスキャン中！！";
    [SerializeField] private string calibrationCompleteMessage = "設定完了！";
    [SerializeField] private float calibrationUIDisplayTime = 3.0f; // UI表示時間
    [SerializeField] private float calibrationCompleteDisplayTime = 1.5f; // 完了メッセージ表示時間
    
    [Header("範囲内プレイヤー管理")]
    [SerializeField] private int maxPlayersInArea = 10;
    
    // 範囲内のプレイヤーを管理
    private VRCPlayerApi[] playersInArea;
    private int playerCount = 0;
    
    // ドアシステムの状態
    private bool isAreaActive = false;
    
    void Start()
    {
        // プレイヤー配列を初期化
        playersInArea = new VRCPlayerApi[maxPlayersInArea];
        
        // 初期状態では非アクティブ
        isAreaActive = false;
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorAreaTrigger] {areaName} エリアを初期化しました (初期状態: 非アクティブ)");
        }
    }
    
    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (player == null) return;
        
        // プレイヤーが範囲内に入った
        AddPlayerToArea(player);
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorAreaTrigger] {areaName} エリアに {player.displayName} が入りました");
        }
        // キャリブレーション開始
        SendCustomEventDelayedSeconds(nameof(CalibratePlayerMotions), calibrationDelay);
    }
    
    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (player == null) return;
        
        // プレイヤーが範囲外に出た
        RemovePlayerFromArea(player);
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorAreaTrigger] {areaName} エリアから {player.displayName} が出ました");
        }
    }
    
    private void AddPlayerToArea(VRCPlayerApi player)
    {
        // 既に登録されているかチェック
        for (int i = 0; i < playerCount; i++)
        {
            if (playersInArea[i] == player)
            {
                return; // 既に登録済み
            }
        }
        
        // 新しいプレイヤーを追加
        if (playerCount < maxPlayersInArea)
        {
            playersInArea[playerCount] = player;
            playerCount++;
            
            // 最初のプレイヤーが入ったらエリアをアクティブにする
            if (playerCount == 1)
            {
                ActivateArea();
            }
        }
    }
    
    private void RemovePlayerFromArea(VRCPlayerApi player)
    {
        // プレイヤーを配列から削除
        for (int i = 0; i < playerCount; i++)
        {
            if (playersInArea[i] == player)
            {
                // 配列の最後の要素を削除位置に移動
                playersInArea[i] = playersInArea[playerCount - 1];
                playersInArea[playerCount - 1] = null;
                playerCount--;
                
                // 最後のプレイヤーが出たらエリアを非アクティブにする
                if (playerCount == 0)
                {
                    DeactivateArea();
                }
                
                break;
            }
        }
    }
    
    private void ActivateArea()
    {
        if (targetDoorSystem != null && !isAreaActive)
        {
            isAreaActive = true;
            targetDoorSystem.SetAreaActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorAreaTrigger] {areaName} エリアがアクティブになりました");
            }
        }
    }
    
    private void DeactivateArea()
    {
        if (targetDoorSystem != null && isAreaActive)
        {
            isAreaActive = false;
            targetDoorSystem.SetAreaActive(false);
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorAreaTrigger] {areaName} エリアが非アクティブになりました");
            }
        }
    }
    
    // 外部からアクセス可能なプロパティ
    public bool IsAreaActive => isAreaActive;
    public int PlayerCount => playerCount;
    public string AreaName => areaName;
    
    // 範囲内のプレイヤーリストを取得
    public VRCPlayerApi[] GetPlayersInArea()
    {
        VRCPlayerApi[] result = new VRCPlayerApi[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            result[i] = playersInArea[i];
        }
        return result;
    }
    
    // 特定のプレイヤーが範囲内にいるかチェック
    public bool IsPlayerInArea(VRCPlayerApi player)
    {
        for (int i = 0; i < playerCount; i++)
        {
            if (playersInArea[i] == player)
            {
                return true;
            }
        }
        return false;
    }

    public void CalibratePlayerMotions()
    {
        // キャリブレーションUIを表示
        ShowCalibrationUI();
        
        // CentralizedMotionDetectorを通じてキャリブレーションを実行
        if (centralizedMotionDetector != null)
        {
            centralizedMotionDetector.CalibrateAllDetectors();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[DoorAreaTrigger] {areaName} キャリブレーションを実行しました");
        }
        
        // 指定時間後に完了UIを表示
        SendCustomEventDelayedSeconds(nameof(ShowCalibrationCompleteUI), calibrationUIDisplayTime);
    }
    
    // キャリブレーションUIを表示
    public void ShowCalibrationUI()
    {
        if (calibrationUIText != null)
        {
            calibrationUIText.text = calibrationMessage;
            calibrationUIText.gameObject.SetActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorAreaTrigger] {areaName} キャリブレーションUIを表示: {calibrationMessage}");
            }
        }
    }

    // キャリブレーション完了UIを表示
    public void ShowCalibrationCompleteUI()
    {
        if (calibrationUIText != null)
        {
            calibrationUIText.text = calibrationCompleteMessage;
            calibrationUIText.gameObject.SetActive(true);
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorAreaTrigger] {areaName} キャリブレーション完了UIを表示: {calibrationCompleteMessage}");
            }
        }
        // 一定時間後にUIを非表示
        SendCustomEventDelayedSeconds(nameof(HideCalibrationUI), calibrationCompleteDisplayTime);
    }
    
    // キャリブレーションUIを非表示
    public void HideCalibrationUI()
    {
        if (calibrationUIText != null)
        {
            calibrationUIText.gameObject.SetActive(false);
            
            if (showDebugInfo)
            {
                Debug.Log($"[DoorAreaTrigger] {areaName} キャリブレーションUIを非表示にしました");
            }
        }
    }
} 
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// プレイヤーの視点（頭）を追従するオブジェクト
/// UdonSharp対応版
/// </summary>
public class PlayerViewTracker : UdonSharpBehaviour
{
    [Header("追従設定")]
    [Tooltip("追従するオブジェクト")]
    public Transform targetObject;
    
    [Tooltip("追従するプレイヤーのVRCPlayerApi")]
    public VRCPlayerApi targetPlayer;
    
    [Tooltip("監視対象のプレイヤーID（VRCPlayerApiのplayerId）")]
    public int targetPlayerId = 0;
    
    [Header("追従オプション")]
    [Tooltip("位置を追従するか")]
    public bool trackPosition = true;
    
    [Tooltip("回転を追従するか")]
    public bool trackRotation = true;
    
    [Tooltip("スムージングの強さ（0=瞬時追従、1=追従しない）")]
    [Range(0f, 1f)]
    public float smoothing = 0.1f;
    
    [Tooltip("オフセット（追従オブジェクトからの相対位置）")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Tooltip("回転オフセット")]
    public Vector3 rotationOffset = Vector3.zero;
    
    [Header("制御")]
    [Tooltip("追従が有効かどうか")]
    public bool isTrackingActive = false;
    
    [Tooltip("自動的に最初のプレイヤーを追従するか")]
    public bool autoStartTracking = false;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = true;
    
    [Tooltip("更新頻度（フレーム単位）")]
    public int updateFrequency = 1;
    
    // プライベート変数
    private bool isInitialized = false;
    private int frameCounter = 0;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
        InitializeTracker();
        
        if (autoStartTracking)
        {
            StartTracking();
        }
    }
    
    void Update()
    {
        if (!isTrackingActive || targetObject == null || targetPlayer == null) return;
        
        // 更新頻度を制御
        frameCounter++;
        if (frameCounter % updateFrequency != 0) return;
        
        UpdateTracking();
    }
    
    /// <summary>
    /// トラッカーの初期化
    /// </summary>
    private void InitializeTracker()
    {
        if (targetObject == null)
        {
            Debug.LogError("PlayerViewTracker: 追従するオブジェクト（targetObject）が設定されていません！");
            return;
        }
        
        isInitialized = true;
        
        if (enableDebugLog)
        {
            Debug.Log("PlayerViewTracker: トラッカーが初期化されました");
        }
    }
    
    /// <summary>
    /// 追従の更新
    /// </summary>
    private void UpdateTracking()
    {
        if (targetPlayer == null || !targetPlayer.IsValid())
        {
            if (enableDebugLog)
            {
                Debug.LogWarning("PlayerViewTracker: 追従対象のプレイヤーが無効です");
            }
            return;
        }
        
        // プレイヤーの頭部の位置と回転を取得
        VRCPlayerApi.TrackingData headTracking = targetPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
        
        // 目標位置と回転を計算
        targetPosition = headTracking.position + positionOffset;
        targetRotation = headTracking.rotation * Quaternion.Euler(rotationOffset);
        
        // 位置の追従
        if (trackPosition)
        {
            if (smoothing > 0f)
            {
                targetObject.position = Vector3.Lerp(targetObject.position, targetPosition, 1f - smoothing);
            }
            else
            {
                targetObject.position = targetPosition;
            }
        }
        
        // 回転の追従
        if (trackRotation)
        {
            if (smoothing > 0f)
            {
                targetObject.rotation = Quaternion.Lerp(targetObject.rotation, targetRotation, 1f - smoothing);
            }
            else
            {
                targetObject.rotation = targetRotation;
            }
        }
        
        // デバッグ情報
        if (enableDebugLog && Time.frameCount % 300 == 0) // 5秒に1回
        {
            Debug.Log($"PlayerViewTracker: {targetPlayer.displayName}を追従中");
        }
    }
    
    /// <summary>
    /// 追従を開始
    /// </summary>
    public void StartTracking()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("PlayerViewTracker: トラッカーが初期化されていません");
            return;
        }
        
        if (targetObject == null)
        {
            Debug.LogWarning("PlayerViewTracker: 追従するオブジェクトが設定されていません");
            return;
        }
        
        // プレイヤーを取得
        targetPlayer = VRCPlayerApi.GetPlayerById(targetPlayerId);
        
        if (targetPlayer == null || !targetPlayer.IsValid())
        {
            if (enableDebugLog)
            {
                Debug.LogWarning($"PlayerViewTracker: プレイヤーID {targetPlayerId} が見つかりません");
            }
            return;
        }
        
        isTrackingActive = true;
        
        if (enableDebugLog)
        {
            Debug.Log($"PlayerViewTracker: {targetPlayer.displayName}の追従を開始しました");
        }
    }
    
    /// <summary>
    /// 追従を停止
    /// </summary>
    public void StopTracking()
    {
        isTrackingActive = false;
        targetPlayer = null;
        
        if (enableDebugLog)
        {
            Debug.Log("PlayerViewTracker: 追従を停止しました");
        }
    }
    
    /// <summary>
    /// 追従の切り替え
    /// </summary>
    public void ToggleTracking()
    {
        if (isTrackingActive)
        {
            StopTracking();
        }
        else
        {
            StartTracking();
        }
    }
    
    /// <summary>
    /// 監視対象プレイヤーを設定
    /// </summary>
    /// <param name="playerId">VRCPlayerApiのplayerId</param>
    public void SetTargetPlayer(int playerId)
    {
        targetPlayerId = playerId;
        
        if (isTrackingActive)
        {
            // 追従中の場合、新しいプレイヤーに切り替え
            StartTracking();
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"PlayerViewTracker: 監視対象プレイヤーIDを {playerId} に設定しました");
        }
    }
    
    /// <summary>
    /// VRCPlayerApiを直接設定
    /// </summary>
    /// <param name="player">追従するプレイヤー</param>
    public void SetTargetPlayer(VRCPlayerApi player)
    {
        if (player != null && player.IsValid())
        {
            targetPlayer = player;
            
            if (enableDebugLog)
            {
                Debug.Log($"PlayerViewTracker: {player.displayName}を直接設定しました");
            }
        }
        else
        {
            if (enableDebugLog)
            {
                Debug.LogWarning("PlayerViewTracker: 無効なプレイヤーが指定されました");
            }
        }
    }
    
    /// <summary>
    /// 追従オブジェクトを設定
    /// </summary>
    /// <param name="obj">追従するオブジェクト</param>
    public void SetTargetObject(Transform obj)
    {
        targetObject = obj;
        
        if (enableDebugLog)
        {
            Debug.Log($"PlayerViewTracker: 追従オブジェクトを {obj.name} に設定しました");
        }
    }
    
    /// <summary>
    /// 位置オフセットを設定
    /// </summary>
    /// <param name="offset">位置オフセット</param>
    public void SetPositionOffset(Vector3 offset)
    {
        positionOffset = offset;
        
        if (enableDebugLog)
        {
            Debug.Log($"PlayerViewTracker: 位置オフセットを {offset} に設定しました");
        }
    }
    
    /// <summary>
    /// 回転オフセットを設定
    /// </summary>
    /// <param name="offset">回転オフセット</param>
    public void SetRotationOffset(Vector3 offset)
    {
        rotationOffset = offset;
        
        if (enableDebugLog)
        {
            Debug.Log($"PlayerViewTracker: 回転オフセットを {offset} に設定しました");
        }
    }
    
    /// <summary>
    /// スムージングを設定
    /// </summary>
    /// <param name="value">スムージング値（0-1）</param>
    public void SetSmoothing(float value)
    {
        smoothing = Mathf.Clamp01(value);
        
        if (enableDebugLog)
        {
            Debug.Log($"PlayerViewTracker: スムージングを {smoothing} に設定しました");
        }
    }
    
    /// <summary>
    /// 追従状態を取得
    /// </summary>
    /// <returns>追従がアクティブかどうか</returns>
    public bool IsTrackingActive()
    {
        return isTrackingActive;
    }
    
    /// <summary>
    /// 現在追従しているプレイヤー名を取得
    /// </summary>
    /// <returns>プレイヤー名</returns>
    public string GetCurrentPlayerName()
    {
        if (targetPlayer != null && targetPlayer.IsValid())
        {
            return targetPlayer.displayName;
        }
        return "なし";
    }
    
    /// <summary>
    /// プレイヤーが参加した時の処理
    /// </summary>
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (enableDebugLog)
        {
            Debug.Log($"PlayerViewTracker: {player.displayName}が参加しました");
        }
    }
    
    /// <summary>
    /// プレイヤーが退出した時の処理
    /// </summary>
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        if (targetPlayer != null && targetPlayer.playerId == player.playerId)
        {
            // 追従していたプレイヤーが退出した場合
            StopTracking();
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"PlayerViewTracker: {player.displayName}が退出しました");
        }
    }
}

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// ボタンアクションの種類
/// </summary>
public enum TrackerAction
{
    Start,      // 追従を開始
    Stop,       // 追従を停止
    Toggle      // 追従のON/OFF切り替え
}

/// <summary>
/// プレイヤー視点追従の制御ボタン
/// UdonSharp対応
/// </summary>
public class PlayerViewTrackerButton : UdonSharpBehaviour
{
    [Header("設定")]
    [Tooltip("PlayerViewTrackerコンポーネント")]
    public PlayerViewTracker playerViewTracker;
    
    [Tooltip("監視対象のプレイヤーID（VRCPlayerApiのplayerId）")]
    public int targetPlayerId = 0;
    
    [Tooltip("ボタンを押した時の動作")]
    public TrackerAction action = TrackerAction.Toggle;
    
    [Header("表示設定")]
    [Tooltip("ボタンの表示テキスト")]
    public string buttonText = "追従開始";
    
    [Tooltip("追従が有効な時のボタンテキスト")]
    public string activeButtonText = "追従停止";
    
    [Tooltip("ボタンのテキストコンポーネント")]
    public UnityEngine.UI.Text buttonTextComponent;
    
    [Tooltip("ボタンの画像コンポーネント")]
    public UnityEngine.UI.Image buttonImageComponent;
    
    [Header("色設定")]
    [Tooltip("追従停止時のボタン色")]
    public Color inactiveColor = Color.white;
    
    [Tooltip("追従中のボタン色")]
    public Color activeColor = Color.green;
    
    
    void Start()
    {
        UpdateButtonDisplay();
    }
    
    void Update()
    {
        // ボタンの表示を更新（0.5秒に1回）
        if (Time.frameCount % 30 == 0)
        {
            UpdateButtonDisplay();
        }
    }
    
    /// <summary>
    /// ボタンがクリックされた時の処理
    /// </summary>
    public override void Interact()
    {
        if (playerViewTracker == null)
        {
            Debug.LogWarning("PlayerViewTrackerButton: PlayerViewTrackerが設定されていません");
            return;
        }
        
        switch (action)
        {
            case TrackerAction.Start:
                playerViewTracker.SetTargetPlayer(targetPlayerId);
                playerViewTracker.StartTracking();
                break;
                
            case TrackerAction.Stop:
                playerViewTracker.StopTracking();
                break;
                
            case TrackerAction.Toggle:
                if (playerViewTracker.IsTrackingActive())
                {
                    playerViewTracker.StopTracking();
                }
                else
                {
                    playerViewTracker.SetTargetPlayer(targetPlayerId);
                    playerViewTracker.StartTracking();
                }
                break;
        }
        
        UpdateButtonDisplay();
        Debug.Log($"PlayerViewTrackerButton: {action} が実行されました");
    }
    
    /// <summary>
    /// ボタンの表示を更新
    /// </summary>
    private void UpdateButtonDisplay()
    {
        if (playerViewTracker == null) return;
        
        bool isActive = playerViewTracker.IsTrackingActive();
        
        // テキストの更新
        if (buttonTextComponent != null)
        {
            if (action == TrackerAction.Toggle)
            {
                buttonTextComponent.text = isActive ? activeButtonText : buttonText;
            }
            else
            {
                buttonTextComponent.text = buttonText;
            }
        }
        
        // 色の更新
        if (buttonImageComponent != null)
        {
            buttonImageComponent.color = isActive ? activeColor : inactiveColor;
        }
    }
    
    /// <summary>
    /// 監視対象プレイヤーIDを設定
    /// </summary>
    /// <param name="playerId">プレイヤーID</param>
    public void SetTargetPlayerId(int playerId)
    {
        targetPlayerId = playerId;
        Debug.Log($"PlayerViewTrackerButton: 監視対象プレイヤーIDを {playerId} に設定しました");
    }
    
    /// <summary>
    /// ボタンアクションを設定
    /// </summary>
    /// <param name="newAction">新しいアクション</param>
    public void SetTrackerAction(TrackerAction newAction)
    {
        action = newAction;
        UpdateButtonDisplay();
        Debug.Log($"PlayerViewTrackerButton: アクションを {newAction} に設定しました");
    }
    
    /// <summary>
    /// ボタンテキストを設定
    /// </summary>
    /// <param name="text">ボタンテキスト</param>
    /// <param name="activeText">アクティブ時のテキスト</param>
    public void SetButtonText(string text, string activeText = "")
    {
        buttonText = text;
        if (!string.IsNullOrEmpty(activeText))
        {
            activeButtonText = activeText;
        }
        UpdateButtonDisplay();
    }
    
    /// <summary>
    /// ボタン色を設定
    /// </summary>
    /// <param name="inactive">非アクティブ時の色</param>
    /// <param name="active">アクティブ時の色</param>
    public void SetButtonColors(Color inactive, Color active)
    {
        inactiveColor = inactive;
        activeColor = active;
        UpdateButtonDisplay();
    }
}

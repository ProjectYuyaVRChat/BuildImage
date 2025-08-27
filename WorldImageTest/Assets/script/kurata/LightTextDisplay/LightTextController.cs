using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// ライトテキスト表示のコントローラー
/// 表示の開始・停止を制御する
/// </summary>
public class LightTextController : UdonSharpBehaviour
{
    [Header("システム設定")]
    [Tooltip("制御するライトテキスト表示システム")]
    public LightTextDisplaySystem displaySystem;
    
    [Header("自動開始設定")]
    [Tooltip("シーン開始時に自動で表示を開始するか")]
    public bool autoStartOnSceneLoad = false;
    [Tooltip("自動開始までの遅延時間（秒）")]
    public float autoStartDelay = 2.0f;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報を表示するか")]
    public bool showDebugInfo = true;
    
    private bool hasAutoStarted = false;
    
    void Start()
    {
        if (displaySystem == null)
        {
            Debug.LogError("[LightTextController] 表示システムが設定されていません！");
            return;
        }
        
        if (autoStartOnSceneLoad)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[LightTextController] {autoStartDelay}秒後に自動開始します");
            }
            SendCustomEventDelayedSeconds(nameof(AutoStartDisplay), autoStartDelay);
        }
    }
    
    /// <summary>
    /// 自動開始処理
    /// </summary>
    public void AutoStartDisplay()
    {
        if (hasAutoStarted) return;
        
        hasAutoStarted = true;
        StartDisplay();
    }
    
    /// <summary>
    /// 表示を開始
    /// </summary>
    public void StartDisplay()
    {
        if (displaySystem == null)
        {
            Debug.LogError("[LightTextController] 表示システムが設定されていません！");
            return;
        }
        
        displaySystem.StartDisplay();
        
        if (showDebugInfo)
        {
            Debug.Log("[LightTextController] 表示を開始しました");
        }
    }
    
    /// <summary>
    /// 表示を停止
    /// </summary>
    public void StopDisplay()
    {
        if (displaySystem == null)
        {
            Debug.LogError("[LightTextController] 表示システムが設定されていません！");
            return;
        }
        
        displaySystem.StopDisplay();
        
        if (showDebugInfo)
        {
            Debug.Log("[LightTextController] 表示を停止しました");
        }
    }
    
    /// <summary>
    /// 表示を一時停止・再開（トグル）
    /// </summary>
    public void ToggleDisplay()
    {
        if (displaySystem == null)
        {
            Debug.LogError("[LightTextController] 表示システムが設定されていません！");
            return;
        }
        
        if (displaySystem.IsDisplaying())
        {
            StopDisplay();
        }
        else
        {
            StartDisplay();
        }
    }
    
    /// <summary>
    /// 表示する文章を変更
    /// </summary>
    /// <param name="newText">新しい文章</param>
    public void ChangeDisplayText(string newText)
    {
        if (displaySystem == null)
        {
            Debug.LogError("[LightTextController] 表示システムが設定されていません！");
            return;
        }
        
        displaySystem.SetDisplayText(newText);
        
        if (showDebugInfo)
        {
            Debug.Log($"[LightTextController] 表示テキストを変更: {newText}");
        }
    }
    
    /// <summary>
    /// 表示間隔を変更
    /// </summary>
    /// <param name="newInterval">新しい間隔（秒）</param>
    public void ChangeDisplayInterval(float newInterval)
    {
        if (displaySystem == null)
        {
            Debug.LogError("[LightTextController] 表示システムが設定されていません！");
            return;
        }
        
        displaySystem.SetDisplayInterval(newInterval);
        
        if (showDebugInfo)
        {
            Debug.Log($"[LightTextController] 表示間隔を変更: {newInterval}秒");
        }
    }
    
    /// <summary>
    /// ライトの点灯時間を変更
    /// </summary>
    /// <param name="newDuration">新しい点灯時間（秒）</param>
    public void ChangeLightDuration(float newDuration)
    {
        if (displaySystem == null)
        {
            Debug.LogError("[LightTextController] 表示システムが設定されていません！");
            return;
        }
        
        displaySystem.SetLightDuration(newDuration);
        
        if (showDebugInfo)
        {
            Debug.Log($"[LightTextController] ライト点灯時間を変更: {newDuration}秒");
        }
    }
    
    /// <summary>
    /// 現在の表示状態を取得
    /// </summary>
    /// <returns>表示中ならtrue</returns>
    public bool IsDisplaying()
    {
        if (displaySystem == null) return false;
        return displaySystem.IsDisplaying();
    }
    
    /// <summary>
    /// システムの情報を取得
    /// </summary>
    /// <returns>システム情報の文字列</returns>
    public string GetSystemInfo()
    {
        if (displaySystem == null)
        {
            return "表示システムが設定されていません";
        }
        
        string info = "ライトテキスト表示システム\n";
        info += $"表示状態: {(displaySystem.IsDisplaying() ? "表示中" : "停止中")}\n";
        info += $"現在の文字位置: {displaySystem.GetCurrentCharIndex()}\n";
        
        return info;
    }
} 
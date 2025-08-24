using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

/// <summary>
/// 鍵管理システム
/// 複数の鍵穴を管理し、鍵の状態を追跡する
/// </summary>
public class KeyManager : UdonSharpBehaviour
{
    [Header("鍵穴管理")]
    [Tooltip("管理する鍵穴の配列")]
    public KeyholeSystem[] keyholes;
    
    [Header("UI設定")]
    [Tooltip("全体状態表示用テキスト")]
    public TMP_Text overallStatusText;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報を表示するか")]
    public bool showDebugInfo = true;
    
    // 内部変数
    private bool[] doorStates; // 各扉の開閉状態
    private int totalDoors;
    private int openedDoors;
    
    void Start()
    {
        InitializeSystem();
    }
    
    /// <summary>
    /// システムを初期化
    /// </summary>
    private void InitializeSystem()
    {
        if (keyholes == null || keyholes.Length == 0)
        {
            Debug.LogWarning("[KeyManager] 鍵穴が設定されていません");
            return;
        }
        
        totalDoors = keyholes.Length;
        doorStates = new bool[totalDoors];
        
        // 初期状態を設定
        for (int i = 0; i < totalDoors; i++)
        {
            doorStates[i] = false;
        }
        
        openedDoors = 0;
        
        if (showDebugInfo)
        {
            Debug.Log($"[KeyManager] システム初期化完了: {totalDoors}個の鍵穴を管理");
        }
        
        UpdateOverallStatus();
    }
    
    void Update()
    {
        // 扉の状態をチェック
        CheckDoorStates();
    }
    
    /// <summary>
    /// 扉の状態をチェック
    /// </summary>
    private void CheckDoorStates()
    {
        if (keyholes == null) return;
        
        int currentOpenedDoors = 0;
        
        for (int i = 0; i < keyholes.Length; i++)
        {
            if (keyholes[i] == null) continue;
            
            // 扉が開いているかチェック（簡易的な判定）
            // 実際の実装では、KeyholeSystemに扉の状態を取得するメソッドを追加することを推奨
            bool isDoorOpen = IsDoorOpen(i);
            
            if (isDoorOpen && !doorStates[i])
            {
                // 扉が新しく開いた
                doorStates[i] = true;
                currentOpenedDoors++;
                
                if (showDebugInfo)
                {
                    Debug.Log($"[KeyManager] 鍵穴 {i + 1} の扉が開きました");
                }
            }
            else if (isDoorOpen)
            {
                currentOpenedDoors++;
            }
        }
        
        if (currentOpenedDoors != openedDoors)
        {
            openedDoors = currentOpenedDoors;
            UpdateOverallStatus();
        }
    }
    
    /// <summary>
    /// 扉が開いているかチェック（簡易版）
    /// </summary>
    /// <param name="index">鍵穴のインデックス</param>
    /// <returns>扉が開いているか</returns>
    private bool IsDoorOpen(int index)
    {
        // 実際の実装では、KeyholeSystemに扉の状態を取得するメソッドを追加することを推奨
        // ここでは簡易的に、鍵穴の位置が初期位置から移動しているかで判定
        if (keyholes[index] == null) return false;
        
        // この判定は簡易的なものなので、実際の使用では改善が必要
        return false; // 仮の実装
    }
    
    /// <summary>
    /// 全体の状態を更新
    /// </summary>
    private void UpdateOverallStatus()
    {
        if (overallStatusText == null) return;
        
        string status = $"鍵穴システム\n";
        status += $"開いた扉: {openedDoors}/{totalDoors}\n";
        
        if (openedDoors == totalDoors)
        {
            status += "すべての扉が開きました！";
        }
        else
        {
            status += $"残り {totalDoors - openedDoors} 個の扉";
        }
        
        overallStatusText.text = status;
    }
    
    #region 外部から呼び出せるメソッド
    
    /// <summary>
    /// 指定した鍵穴の扉を強制的に開く
    /// </summary>
    /// <param name="index">鍵穴のインデックス</param>
    public void ForceOpenDoor(int index)
    {
        if (index < 0 || index >= keyholes.Length || keyholes[index] == null)
        {
            Debug.LogWarning($"[KeyManager] 無効な鍵穴インデックス: {index}");
            return;
        }
        
        keyholes[index].ForceOpenDoor();
        
        if (showDebugInfo)
        {
            Debug.Log($"[KeyManager] 鍵穴 {index + 1} の扉を強制開放");
        }
    }
    
    /// <summary>
    /// すべての扉を強制的に開く
    /// </summary>
    public void ForceOpenAllDoors()
    {
        for (int i = 0; i < keyholes.Length; i++)
        {
            if (keyholes[i] != null)
            {
                keyholes[i].ForceOpenDoor();
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("[KeyManager] すべての扉を強制開放");
        }
    }
    
    /// <summary>
    /// 指定した鍵穴の扉をリセット
    /// </summary>
    /// <param name="index">鍵穴のインデックス</param>
    public void ResetDoor(int index)
    {
        if (index < 0 || index >= keyholes.Length || keyholes[index] == null)
        {
            Debug.LogWarning($"[KeyManager] 無効な鍵穴インデックス: {index}");
            return;
        }
        
        keyholes[index].ResetDoor();
        doorStates[index] = false;
        
        if (showDebugInfo)
        {
            Debug.Log($"[KeyManager] 鍵穴 {index + 1} の扉をリセット");
        }
        
        UpdateOverallStatus();
    }
    
    /// <summary>
    /// すべての扉をリセット
    /// </summary>
    public void ResetAllDoors()
    {
        for (int i = 0; i < keyholes.Length; i++)
        {
            if (keyholes[i] != null)
            {
                keyholes[i].ResetDoor();
            }
            doorStates[i] = false;
        }
        
        openedDoors = 0;
        
        if (showDebugInfo)
        {
            Debug.Log("[KeyManager] すべての扉をリセット");
        }
        
        UpdateOverallStatus();
    }
    
    /// <summary>
    /// 鍵穴の情報を取得
    /// </summary>
    /// <returns>鍵穴の情報文字列</returns>
    public string GetKeyholeInfo()
    {
        if (keyholes == null || keyholes.Length == 0)
        {
            return "鍵穴が設定されていません";
        }
        
        string info = $"鍵穴システム情報:\n";
        info += $"総数: {keyholes.Length}\n";
        info += $"開いた扉: {openedDoors}\n\n";
        
        for (int i = 0; i < keyholes.Length; i++)
        {
            if (keyholes[i] != null)
            {
                info += $"鍵穴 {i + 1}: {keyholes[i].GetKeyholeInfo()}\n";
            }
            else
            {
                info += $"鍵穴 {i + 1}: 未設定\n";
            }
        }
        
        return info;
    }
    
    #endregion
} 
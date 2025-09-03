using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// シンプルなCube制御システム
/// 外部から呼び出してCubeを消す
/// </summary>
public class WordInputSystem : UdonSharpBehaviour
{
    [Header("オブジェクト設定")]
    [Tooltip("消したいCube（UdonBehaviour）")]
    public UdonBehaviour targetCube;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグ情報を表示するか")]
    public bool showDebugInfo = true;
    
    // 内部変数
    private bool isDestroyed = false;
    
    void Start()
    {
        InitializeSystem();
    }
    
    /// <summary>
    /// システムを初期化
    /// </summary>
    private void InitializeSystem()
    {
        if (targetCube == null)
        {
            Debug.LogError("[WordInputSystem] 対象のCubeが設定されていません！");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[WordInputSystem] システム初期化完了");
            Debug.Log($"[WordInputSystem] 対象Cube: {targetCube.name}");
        }
    }
    
    /// <summary>
    /// Cubeを消す（外部から呼び出し可能）
    /// </summary>
    public void DestroyCube()
    {
        if (targetCube == null || isDestroyed) return;
        
        // CubeのGameObjectを取得して非アクティブにする
        GameObject cubeObject = targetCube.gameObject;
        if (cubeObject != null)
        {
            cubeObject.SetActive(false);
            isDestroyed = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"[WordInputSystem] Cube '{cubeObject.name}' を非アクティブにしました");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("[WordInputSystem] CubeのGameObjectが見つかりません");
            }
        }
    }
    
    #region 外部から呼び出せるメソッド
    
    /// <summary>
    /// Cubeが消されたかどうかを取得
    /// </summary>
    /// <returns>消されたらtrue</returns>
    public bool IsDestroyed()
    {
        return isDestroyed;
    }
    
    /// <summary>
    /// システムをリセット
    /// </summary>
    public void ResetSystem()
    {
        if (targetCube != null && isDestroyed)
        {
            targetCube.gameObject.SetActive(true);
            isDestroyed = false;
            
            if (showDebugInfo)
            {
                Debug.Log("[WordInputSystem] システムをリセットしました");
            }
        }
    }
    
    #endregion
} 
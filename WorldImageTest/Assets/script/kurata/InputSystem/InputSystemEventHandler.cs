using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// InputSystemのイベントを使ってCubeを消す
/// </summary>
public class InputSystemEventHandler : UdonSharpBehaviour
{
    [Header("イベント設定")]
    [SerializeField, Tooltip("正解時イベント先（シングルパスワード時）\nQuestionNumがマイナスの時はこちらのイベントが発火します\nQuestionNumがマイナスの時かつココに何も入れなければMultiEventを参照します")]
    public UdonBehaviour singleEvent;
    
    [Header("オブジェクト設定")]
    [SerializeField, Tooltip("消したいCube（UdonBehaviour）")]
    public UdonBehaviour targetCube;
    
    [Header("デバッグ設定")]
    [SerializeField, Tooltip("デバッグ情報を表示するか")]
    public bool showDebugInfo = true;
    
    // 内部変数
    private bool isDestroyed = false;
    
    void Start()
    {
        InitializeSystem();
    }
    

    private void InitializeSystem()
    {
        if (targetCube == null)
        {
            Debug.LogError("[InputSystemEventHandler] 対象のCubeが設定されていません！");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"[InputSystemEventHandler] システム初期化完了");
            Debug.Log($"[InputSystemEventHandler] 対象Cube: {targetCube.name}");
            if (singleEvent != null)
            {
                Debug.Log($"[InputSystemEventHandler] シングルイベント: {singleEvent.name}");
            }
        }
    }
    
    /// <summary>
    /// InputSystemの正解イベントが発火した時に呼び出される
    /// </summary>
    public void OnCorrectAnswer()
    {
        if (isDestroyed) return;
        
        if (showDebugInfo)
        {
            Debug.Log("[InputSystemEventHandler] 正解イベントを受信しました！Cubeを消します。");
        }
        
        DestroyCube();
    }
    
    /// <summary>
    /// とりあえずCubeを消す
    /// </summary>
    private void DestroyCube()
    {
        if (targetCube == null || isDestroyed) return;
        
        GameObject cubeObject = targetCube.gameObject;
        if (cubeObject != null)
        {
            cubeObject.SetActive(false);
            isDestroyed = true;
            
            if (showDebugInfo)
            {
                Debug.Log($"[InputSystemEventHandler] Cube '{cubeObject.name}' を非アクティブにしました");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("[InputSystemEventHandler] CubeのGameObjectが見つかりません");
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
                Debug.Log("[InputSystemEventHandler] システムをリセットしました");
            }
        }
    }
    
    #endregion
} 
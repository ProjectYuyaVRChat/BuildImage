using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// DoorGimmickSystemNewのモーション成功時に特定のオブジェクトを有効化するスクリプト
/// UdonSharp対応
/// </summary>
public class MotionSuccessObjectActivator : UdonSharpBehaviour
{
    [Header("DoorGimmickSystemNew設定")]
    [Tooltip("DoorGimmickSystemNewコンポーネント")]
    public DoorGimmickSystemNew doorGimmickSystem;
    
    [Header("ハンドラー設定（直接参照用）")]
    [Tooltip("SequentialMotionHandler（順次モード用）")]
    public SequentialMotionHandler sequentialHandler;
    
    [Tooltip("SimultaneousMotionHandler（同時モード用）")]
    public SimultaneousMotionHandler simultaneousHandler;
    
    [Tooltip("CounterMotionHandler（カウンターモード用）")]
    public CounterMotionHandler counterHandler;
    
    [Header("モーション1成功時の設定")]
    [Tooltip("モーション1が成功したら有効化するオブジェクト")]
    public GameObject motion1SuccessObject;
    
    [Tooltip("モーション1成功時にオブジェクトを有効化するか")]
    public bool enableMotion1Object = true;
    
    [Header("モーション2成功時の設定")]
    [Tooltip("モーション2が成功したら有効化するオブジェクト")]
    public GameObject motion2SuccessObject;
    
    [Tooltip("モーション2成功時にオブジェクトを有効化するか")]
    public bool enableMotion2Object = true;
    
    [Header("モーション3成功時の設定")]
    [Tooltip("モーション3が成功したら有効化するオブジェクト")]
    public GameObject motion3SuccessObject;
    
    [Tooltip("モーション3成功時にオブジェクトを有効化するか")]
    public bool enableMotion3Object = true;
    
    [Header("動作設定")]
    [Tooltip("モーションが失敗したらオブジェクトを無効化するか")]
    public bool disableOnMotionFail = false;
    
    [Tooltip("システムリセット時にオブジェクトを無効化するか")]
    public bool disableOnReset = true;
    
    [Header("デバッグ設定")]
    [Tooltip("デバッグログを表示するか")]
    public bool enableDebugLog = true;
    
    [Tooltip("更新頻度（フレーム単位）")]
    public int updateFrequency = 1;
    
    // 内部状態
    private bool[] previousMotionStates = new bool[3];
    private bool[] currentMotionStates = new bool[3];
    private bool isInitialized = false;
    private int frameCounter = 0;
    
    // ハンドラー参照
    //private SequentialMotionHandler sequentialHandler;
    //private SimultaneousMotionHandler simultaneousHandler;
    //private CounterMotionHandler counterHandler;
    
    void Start()
    {
        InitializeSystem();
    }
    
    void Update()
    {
        if (!isInitialized || doorGimmickSystem == null) return;
        
        // 更新頻度を制御
        frameCounter++;
        if (frameCounter % updateFrequency != 0) return;
        
        CheckMotionStates();
    }
    
    /// <summary>
    /// システムの初期化
    /// </summary>
    private void InitializeSystem()
    {
        if (doorGimmickSystem == null)
        {
            Debug.LogError("MotionSuccessObjectActivator: DoorGimmickSystemNewが設定されていません！");
            return;
        }
        
        // 初期状態でオブジェクトを無効化
        if (disableOnReset)
        {
            SetObjectState(motion1SuccessObject, false);
            SetObjectState(motion2SuccessObject, false);
            SetObjectState(motion3SuccessObject, false);
        }
        
        // 前回の状態を初期化
        for (int i = 0; i < previousMotionStates.Length; i++)
        {
            previousMotionStates[i] = false;
            currentMotionStates[i] = false;
        }
        
        isInitialized = true;
        
        if (enableDebugLog)
        {
            Debug.Log("MotionSuccessObjectActivator: システムが初期化されました");
        }
    }
    
    /// <summary>
    /// モーション状態をチェック
    /// </summary>
    private void CheckMotionStates()
    {
        // 現在のモーション状態を取得
        GetCurrentMotionStates();
        
        // 各モーションの状態変化をチェック
        for (int i = 0; i < 3; i++)
        {
            // モーションが成功した（false → true）
            if (!previousMotionStates[i] && currentMotionStates[i])
            {
                OnMotionSuccess(i + 1);
            }
            // モーションが失敗した（true → false）
            else if (previousMotionStates[i] && !currentMotionStates[i])
            {
                OnMotionFail(i + 1);
            }
        }
        
        // 前回の状態を更新
        for (int i = 0; i < 3; i++)
        {
            previousMotionStates[i] = currentMotionStates[i];
        }
    }
    
    /// <summary>
    /// 現在のモーション状態を取得
    /// </summary>
    private void GetCurrentMotionStates()
    {
        if (doorGimmickSystem == null) return;
        
        // DoorGimmickSystemNewから直接モーション状態を取得（推奨方法）
        currentMotionStates[0] = doorGimmickSystem.IsMotion1Success();
        currentMotionStates[1] = doorGimmickSystem.IsMotion2Success();
        currentMotionStates[2] = doorGimmickSystem.IsMotion3Success();
        
        // ハンドラーが設定されている場合は、それも確認（順次モードやカウンターモード用）
        if (sequentialHandler != null)
        {
            bool[] stepCompleted = sequentialHandler.StepCompleted;
            if (stepCompleted != null && stepCompleted.Length >= 3)
            {
                // 順次モードでは、ステップ完了状態を優先
                currentMotionStates[0] = stepCompleted[0];
                currentMotionStates[1] = stepCompleted[1];
                currentMotionStates[2] = stepCompleted[2];
            }
        }
        else if (counterHandler != null)
        {
            bool[] motionAchieved = counterHandler.MotionAchievedCounter;
            if (motionAchieved != null && motionAchieved.Length >= 3)
            {
                // カウンターモードでは、達成状態を優先
                currentMotionStates[0] = motionAchieved[0];
                currentMotionStates[1] = motionAchieved[1];
                currentMotionStates[2] = motionAchieved[2];
            }
        }
    }
    
    /// <summary>
    /// モーション成功時の処理
    /// </summary>
    /// <param name="motionNumber">モーション番号（1, 2, 3）</param>
    private void OnMotionSuccess(int motionNumber)
    {
        if (enableDebugLog)
        {
            Debug.Log($"MotionSuccessObjectActivator: モーション{motionNumber}が成功しました");
        }
        
        switch (motionNumber)
        {
            case 1:
                if (enableMotion1Object && motion1SuccessObject != null)
                {
                    SetObjectState(motion1SuccessObject, true);
                    if (enableDebugLog)
                    {
                        Debug.Log($"MotionSuccessObjectActivator: モーション1成功オブジェクトを有効化しました: {motion1SuccessObject.name}");
                    }
                }
                break;
                
            case 2:
                if (enableMotion2Object && motion2SuccessObject != null)
                {
                    SetObjectState(motion2SuccessObject, true);
                    if (enableDebugLog)
                    {
                        Debug.Log($"MotionSuccessObjectActivator: モーション2成功オブジェクトを有効化しました: {motion2SuccessObject.name}");
                    }
                }
                break;
                
            case 3:
                if (enableMotion3Object && motion3SuccessObject != null)
                {
                    SetObjectState(motion3SuccessObject, true);
                    if (enableDebugLog)
                    {
                        Debug.Log($"MotionSuccessObjectActivator: モーション3成功オブジェクトを有効化しました: {motion3SuccessObject.name}");
                    }
                }
                break;
        }
    }
    
    /// <summary>
    /// モーション失敗時の処理
    /// </summary>
    /// <param name="motionNumber">モーション番号（1, 2, 3）</param>
    private void OnMotionFail(int motionNumber)
    {
        if (!disableOnMotionFail) return;
        
        if (enableDebugLog)
        {
            Debug.Log($"MotionSuccessObjectActivator: モーション{motionNumber}が失敗しました");
        }
        
        switch (motionNumber)
        {
            case 1:
                if (motion1SuccessObject != null)
                {
                    SetObjectState(motion1SuccessObject, false);
                }
                break;
                
            case 2:
                if (motion2SuccessObject != null)
                {
                    SetObjectState(motion2SuccessObject, false);
                }
                break;
                
            case 3:
                if (motion3SuccessObject != null)
                {
                    SetObjectState(motion3SuccessObject, false);
                }
                break;
        }
    }
    
    /// <summary>
    /// オブジェクトの状態を設定
    /// </summary>
    /// <param name="obj">対象オブジェクト</param>
    /// <param name="active">有効化するかどうか</param>
    private void SetObjectState(GameObject obj, bool active)
    {
        if (obj != null)
        {
            obj.SetActive(active);
        }
    }
    
    /// <summary>
    /// モーション1成功を手動で通知（DoorGimmickSystemNewから呼び出し可能）
    /// </summary>
    public void NotifyMotion1Success()
    {
        if (!currentMotionStates[0])
        {
            currentMotionStates[0] = true;
            OnMotionSuccess(1);
        }
    }
    
    /// <summary>
    /// モーション2成功を手動で通知（DoorGimmickSystemNewから呼び出し可能）
    /// </summary>
    public void NotifyMotion2Success()
    {
        if (!currentMotionStates[1])
        {
            currentMotionStates[1] = true;
            OnMotionSuccess(2);
        }
    }
    
    /// <summary>
    /// モーション3成功を手動で通知（DoorGimmickSystemNewから呼び出し可能）
    /// </summary>
    public void NotifyMotion3Success()
    {
        if (!currentMotionStates[2])
        {
            currentMotionStates[2] = true;
            OnMotionSuccess(3);
        }
    }
    
    /// <summary>
    /// すべてのモーション状態をリセット
    /// </summary>
    public void ResetAllMotions()
    {
        for (int i = 0; i < currentMotionStates.Length; i++)
        {
            previousMotionStates[i] = false;
            currentMotionStates[i] = false;
        }
        
        if (disableOnReset)
        {
            SetObjectState(motion1SuccessObject, false);
            SetObjectState(motion2SuccessObject, false);
            SetObjectState(motion3SuccessObject, false);
        }
        
        if (enableDebugLog)
        {
            Debug.Log("MotionSuccessObjectActivator: すべてのモーション状態をリセットしました");
        }
    }
    
    /// <summary>
    /// 特定のモーション状態をリセット
    /// </summary>
    /// <param name="motionNumber">モーション番号（1, 2, 3）</param>
    public void ResetMotion(int motionNumber)
    {
        if (motionNumber < 1 || motionNumber > 3) return;
        
        int index = motionNumber - 1;
        previousMotionStates[index] = false;
        currentMotionStates[index] = false;
        
        if (disableOnReset)
        {
            switch (motionNumber)
            {
                case 1:
                    SetObjectState(motion1SuccessObject, false);
                    break;
                case 2:
                    SetObjectState(motion2SuccessObject, false);
                    break;
                case 3:
                    SetObjectState(motion3SuccessObject, false);
                    break;
            }
        }
        
        if (enableDebugLog)
        {
            Debug.Log($"MotionSuccessObjectActivator: モーション{motionNumber}の状態をリセットしました");
        }
    }
}
